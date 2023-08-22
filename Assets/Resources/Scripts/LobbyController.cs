using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class LobbyController : MonoBehaviourPunCallbacks
{
    public TMP_InputField createField;
    public TMP_InputField joinField;

    public TMP_Text msgField;

    public void CreateRoom()
    {
        if (createField.text.Length == 0) {
            ShowMessage("Название комнаты не может быть пустым!");
            return;
        } else if (createField.text.Length > 32) {
            ShowMessage("Название комнаты не может содержать более 32 символов!");
            return;
        }

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 4;
        PhotonNetwork.CreateRoom(createField.text, options);
    }

    public void JoinRoom()
    {
        if (joinField.text.Length == 0) {
            ShowMessage("Название комнаты не может быть пустым!");
            return;
        } else if (joinField.text.Length > 32) {
            ShowMessage("Название комнаты не может содержать более 32 символов!");
            return;
        }

        PhotonNetwork.JoinRoom(joinField.text);
    }

    public override void OnJoinedRoom()
    {
        SceneManager.LoadScene("Game");
    }

    public void ShowMessage(string msg) => msgField.text = msg;
}
