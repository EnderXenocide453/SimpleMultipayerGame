using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    void Start()
    {
        //����������� � �������, ��������� ���� ������������
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        //������ ������� �����
        SceneManager.LoadScene("Lobby");
    }

    private void OnDisconnectedFromMasterServer()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
}
