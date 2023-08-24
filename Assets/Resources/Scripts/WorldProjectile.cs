using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WorldProjectile : MonoBehaviour
{
    private Collider2D _collider;
    private SpriteRenderer _sprite;
    private Projectile _projectile;
    private Rigidbody2D _body;

    private Vector3 _dir;

    private bool _ready = false;
    private int _playerID;

    public void InitProjectile(Projectile config, Vector2 dir, int playerID)
    {
        _body = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _sprite = GetComponent<SpriteRenderer>();
        _sprite.sprite = config.sprite;

        _playerID = playerID;
        _projectile = config;
        _ready = true;
        _dir = dir.normalized;

        _body.velocity = _dir * _projectile.speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player;

        if (collision.gameObject.TryGetComponent<PlayerController>(out player)) {
            if (player.playerID != _playerID) player.TakeDamage(_projectile.damage);
            else return;
        }

        PhotonNetwork.Destroy(GetComponent<PhotonView>());
    }
}

[System.Serializable]
public class Projectile
{
    public float speed = 2.0f, 
        damage = 10.0f, 
        cooldown = 0.2f;
    public Sprite sprite;
}