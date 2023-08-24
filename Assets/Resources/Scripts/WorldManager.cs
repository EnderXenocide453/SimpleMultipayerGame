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
    public List<Vector2> spawnPoints;
    //Количество игроков для старта игры
    [Range(1, 4)]
    public int minPlayers = 2;

    public int count = 0;

    public TMPro.TMP_Text waitField;

    private void Start()
    {
        Vector2 pos = new Vector2(Random.Range(-2, 2), Random.Range(-1, 1));
        PhotonNetwork.Instantiate(playerPath, pos, Quaternion.identity);

        PlayerNumbering.OnPlayerNumberingChanged += () => SetPlayersActivity(PhotonNetwork.PlayerList.Length >= minPlayers);
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
}
