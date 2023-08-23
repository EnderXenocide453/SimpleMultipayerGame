using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WorldProjectile : MonoBehaviour
{
    private Collider2D _collider;
    private SpriteRenderer _sprite;

    private Projectile _projectile;

    private Vector3 _dir;

    private bool _ready = false;

    public void InitProjectile(Projectile config, Vector2 dir)
    {
        _collider = GetComponent<Collider2D>();
        _sprite = GetComponent<SpriteRenderer>();
        _sprite.sprite = config.sprite;

        _projectile = config;
        _ready = true;
        _dir = dir.normalized;
    }

    private void FixedUpdate()
    {
        if (!_ready) return;

        Move();
    }

    private void Move()
    {
        transform.position += _dir * _projectile.speed * Time.fixedDeltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
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