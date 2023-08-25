using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Coin : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player;

        if (collision.gameObject.TryGetComponent<PlayerController>(out player)) {
            player.AddCoin();

            PhotonNetwork.Destroy(GetComponent<PhotonView>());
        }
    }

    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }
}
