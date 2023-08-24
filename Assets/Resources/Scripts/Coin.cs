using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Coin : MonoBehaviour
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
}
