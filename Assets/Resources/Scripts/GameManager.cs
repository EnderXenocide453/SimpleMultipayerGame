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
        GeneratePlayerFeatures();

        Vector2 pos = new Vector2(Random.Range(-2, 2), Random.Range(-1, 1));
        PhotonNetwork.Instantiate(playerPath, spawnPoints[PhotonNetwork.PlayerList.Length - 1].position, Quaternion.identity);

        _scoreList = new List<PlayerInfo>();

        _countAlive = PhotonNetwork.PlayerList.Length;

        PlayerNumbering.OnPlayerNumberingChanged += () =>
        {
            Debug.Log(PhotonNetwork.PlayerList.Length);

            SetPlayersActivity(PhotonNetwork.PlayerList.Length >= minPlayers);

            int delta = PhotonNetwork.PlayerList.Length - _countAlive;
            SetAliveCount(_countAlive + delta);
        };
    }

    private void GeneratePlayerFeatures()
    {
        Player player = PhotonNetwork.LocalPlayer;
        player.NickName = "Amogus" + player.ActorNumber;
    }

    private void SetPlayersActivity(bool value)
    {
        _gameStarted = value;
        waitField.enabled = !value;

        Hashtable playerActive = new Hashtable();
        playerActive.Add("isActive", value);

        foreach (var player in PhotonNetwork.PlayerList) {
            player.SetCustomProperties(playerActive);
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
    }

    public void RegistrateDeath(int playerID)
    {
        SetAliveCount(_countAlive - 1);
        _scoreList.Add(new PlayerInfo(PhotonNetwork.PlayerList[playerID - 1].NickName, (int)PhotonNetwork.PlayerList[playerID - 1].CustomProperties["coins"]));
    }
}

public static class EventCodes
{
    public static byte deathEvent = 0;
}