using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPlayer : MonoBehaviourPun
{
    //��������������� �����
    public string playerPath;

    //������ ����� ������
    public List<Vector2> spawnPoints;

    private void Start()
    {
        Vector2 pos = new Vector2(Random.Range(-2, 2), Random.Range(-1, 1));
        PhotonNetwork.Instantiate(playerPath, pos, Quaternion.identity);
    }
}
