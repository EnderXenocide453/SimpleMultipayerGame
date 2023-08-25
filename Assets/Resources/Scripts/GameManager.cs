using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class GameManager : MonoBehaviourPun
{
    public GameManager Instance;

    public GameObject tableRow;
    public Transform content;

    //Инстанциируемый игрок
    public string playerPath;
    //Список точек спавна
    public List<Transform> spawnPoints;
    //Количество игроков для старта игры
    [Range(1, 4)]
    public int minPlayers = 2;

    public TMPro.TMP_Text waitField;

    [SerializeField]
    private int _countAlive = 0;

    [SerializeField]
    private List<PlayerInfo> _scoreList;

    private bool _gameStarted = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            PlayerFeaturesFabric.seed = Random.Range(0, int.MaxValue);
        else {
            object[] objects = new object[] { PhotonNetwork.LocalPlayer.ActorNumber };

            RaiseEventOptions options = RaiseEventOptions.Default;
            options.Receivers = ReceiverGroup.Others;
            options.TargetActors = new int[] { PhotonNetwork.MasterClient.ActorNumber };

            PhotonNetwork.RaiseEvent(EventCodes.getFeatureRequest, objects, RaiseEventOptions.Default, SendOptions.SendUnreliable);
        }


        Vector2 pos = new Vector2(Random.Range(-2, 2), Random.Range(-1, 1));
        PhotonNetwork.Instantiate(playerPath, spawnPoints[PhotonNetwork.PlayerList.Length - 1].position, Quaternion.identity);

        _scoreList = new List<PlayerInfo>();

        _countAlive = PhotonNetwork.PlayerList.Length;

        Hashtable playerProperties = new Hashtable();
        playerProperties.Add("isActive", false);
        playerProperties.Add("isAlive", true);
        playerProperties.Add("coins", 0);

        foreach (var player in PhotonNetwork.PlayerList) {
            player.SetCustomProperties(playerProperties);
        }

        PlayerNumbering.OnPlayerNumberingChanged += () =>
        {
            SetPlayersActivity(PhotonNetwork.PlayerList.Length >= minPlayers);

            int delta = PhotonNetwork.PlayerList.Length - _countAlive;
            SetAliveCount(_countAlive + delta);

            if (delta > 0) {
                PhotonNetwork.PlayerList[PhotonNetwork.PlayerList.Length - 1].SetCustomProperties(playerProperties);
            }
        };
    }

    private void SetPlayersActivity(bool value)
    {
        _gameStarted = value;
        waitField.enabled = !value;

        Hashtable playerProperties = new Hashtable();
        playerProperties.Add("isActive", value);

        foreach (var player in PhotonNetwork.PlayerList) {
            player.SetCustomProperties(playerProperties);
        }
    }

    [System.Serializable]
    public class PlayerInfo
    {
        public string nickName;
        public int coinsCount;

        public PlayerInfo(string name, int count)
        {
            nickName = name;
            coinsCount = count;
        }
    }

    private void SetAliveCount(int count)
    {
        Debug.Log("Now alive: " + count);

        _countAlive = count;

        if (_countAlive < 2 && _gameStarted) {
            content.parent.localPosition = Vector3.zero;

            Transform row;

            for (int i = 0; i < _scoreList.Count; i++) {
                row = Instantiate(tableRow, content).transform;

                row.GetChild(0).GetComponent<TMPro.TMP_Text>().text = (_scoreList.Count + _countAlive - i).ToString();
                row.GetChild(1).GetComponent<TMPro.TMP_Text>().text = _scoreList[i].nickName;
                row.GetChild(2).GetComponent<TMPro.TMP_Text>().text = _scoreList[i].coinsCount.ToString();
            }

            foreach (Player player in PhotonNetwork.PlayerList) {
                if ((bool)player.CustomProperties["isAlive"]) {
                    row = Instantiate(tableRow, content).transform;

                    row.GetChild(0).GetComponent<TMPro.TMP_Text>().text = "1";
                    row.GetChild(1).GetComponent<TMPro.TMP_Text>().text = player.NickName;
                    row.GetChild(2).GetComponent<TMPro.TMP_Text>().text = player.CustomProperties["coins"].ToString();

                    break;
                }
            }

            
        }
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }

    private void NetworkingClient_EventReceived(EventData obj)
    {
        if (obj.Code == EventCodes.deathEvent) {
            object[] objects = (object[])obj.CustomData;
            RegistrateDeath((int)objects[0]);
        } 
        //Запрос на получение seed
        else if (obj.Code == EventCodes.getFeatureRequest) {
            //получаем id отправителя
            object[] objects = (object[])obj.CustomData;
            int id = (int)objects[0];

            //Отправляем сгенерированные фичи
            objects = new object[] { PlayerFeaturesFabric.seed };

            RaiseEventOptions options = RaiseEventOptions.Default;
            options.Receivers = ReceiverGroup.Others;
            options.TargetActors = new int[] { id };

            PhotonNetwork.RaiseEvent(EventCodes.sendFeature, objects, RaiseEventOptions.Default, SendOptions.SendUnreliable);
        } 
        //Получение seed
        else if (obj.Code == EventCodes.sendFeature && !PlayerFeaturesFabric.isInit) {
            //принимаем фичи
            object[] objects = (object[])obj.CustomData;
            PlayerFeaturesFabric.seed = (int)objects[0];
            PlayerFeaturesFabric.Reinit();
        }
    }

    public void RegistrateDeath(int playerID)
    {
        PhotonNetwork.PlayerList[playerID - 1].CustomProperties["isAlive"] = false;

        _scoreList.Add(new PlayerInfo(PhotonNetwork.PlayerList[playerID - 1].NickName, (int)PhotonNetwork.PlayerList[playerID - 1].CustomProperties["coins"]));
        SetAliveCount(_countAlive - 1);
    }
}

public static class EventCodes
{
    public static byte deathEvent = 0;
    public static byte getFeatureRequest = 1;
    public static byte sendFeature = 2;
}

public static class PlayerFeaturesFabric
{
    public static int seed = 0;
    public static bool isInit { get; private set; } = false;

    private static List<PlayerFeatures> _playerFeatures;
    private static List<(string, Color)> _colors;

    private static System.Random _rand;

    private static void InitFabric()
    {
        _rand = new System.Random(seed);

        SetColors();
        GenerateFeatures();

        isInit = true;
    }

    public static void Reinit()
    {
        InitFabric();
    }

    public static List<PlayerFeatures> GetPlayerFeatures()
    {
        if (!isInit) InitFabric();

        return _playerFeatures;
    }

    private static void SetColors()
    {
        _colors = new List<(string, Color)>();

        _colors.Add(("Серый", Color.gray));
        _colors.Add(("Белый", Color.white));
        _colors.Add(("Зеленый", Color.green));
        _colors.Add(("Красный", Color.red));
        _colors.Add(("Синий", Color.blue));
        _colors.Add(("Бирюзовый", Color.cyan));
        _colors.Add(("Желтый", Color.yellow));
        _colors.Add(("Пурпурный", Color.magenta));
    }

    private static void GenerateFeatures()
    {
        _playerFeatures = new List<PlayerFeatures>();

        foreach (var color in _colors) {
            string name;
            Color bodyColor;

            (name, bodyColor) = color;
            Color eyeColor = _colors[_rand.Next(0, _colors.Count)].Item2;

            _playerFeatures.Add(new PlayerFeatures(name, bodyColor, eyeColor));
        }
    }
}

public class PlayerFeatures
{
    public string nickName;
    public Color bodyColor;
    public Color eyeColor;

    public PlayerFeatures(string name, Color bodyColor, Color eyeColor)
    {
        nickName = name;
        this.bodyColor = bodyColor;
        this.eyeColor = eyeColor;
    }
}