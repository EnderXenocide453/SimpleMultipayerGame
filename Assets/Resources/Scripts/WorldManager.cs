using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using ExitGames.Client.Photon;

public class WorldManager : MonoBehaviourPun
{
    //Инстанциируемый игрок
    public string playerPath;
    //Список точек спавна
    public List<Transform> spawnPoints;
    //Количество игроков для старта игры
    [Range(1, 4)]
    public int minPlayers = 2;

    private int _oldCount = 0;
    private int _countAlive = 0;
    [SerializeField]
    private List<PlayerInfo> _scoreList;

    public TMPro.TMP_Text waitField;

    private void Start()
    {
        Vector2 pos = new Vector2(Random.Range(-2, 2), Random.Range(-1, 1));
        PhotonNetwork.Instantiate(playerPath, spawnPoints[PhotonNetwork.PlayerList.Length - 1].position, Quaternion.identity);

        _scoreList = new List<PlayerInfo>();

        PlayerNumbering.OnPlayerNumberingChanged += () =>
        {
            SetPlayersActivity(PhotonNetwork.PlayerList.Length >= minPlayers);

            int delta = PhotonNetwork.PlayerList.Length - _oldCount;
            SetAliveCount(_countAlive + delta);
        };
    }

    private void SetPlayersActivity(bool value)
    {
        waitField.enabled = !value;

        Hashtable playerActive = new Hashtable();
        playerActive.Add("isActive", value);

        foreach (var player in PhotonNetwork.PlayerList) {
            player.SetCustomProperties(playerActive);
        }
    }

    [System.Serializable]
    public struct PlayerInfo
    {
        string nickName;
        int coinsCount;

        public PlayerInfo(string name, int count)
        {
            nickName = name;
            coinsCount = count;
        }
    }

    private void SetAliveCount(int count)
    {
        _countAlive = count;

        if (_countAlive < 2) {
            //Завершаем игру, вывешиваем таблицу
        }
    }

    public void RegistrateDeath(PlayerInfo info)
    {
        SetAliveCount(_countAlive - 1);
        _scoreList.Add(info);
    }
}
