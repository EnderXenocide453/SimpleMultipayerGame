using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    private PhotonView _view;
    private Rigidbody2D _body;

    private Joystick _moveJoystick;
    private Joystick _attackJoystick;
    private Joystick _extraAttackJoystick;
    private Button _defenseButton;

    private Animator _anim;
    private SpriteRenderer _sprite;

    private Vector2 _lookDir = Vector2.right;

    private Transform _eye;

    private delegate void AttackDelegate();
    private AttackDelegate _currentAttack;

    public float speed = 4.0f;
    public float accel = 0.2f;

    void Start()
    {
        _view = GetComponent<PhotonView>();
        _body = GetComponent<Rigidbody2D>();

        _moveJoystick = GameObject.Find("MoveJoystick").GetComponentInChildren<Joystick>();
        _attackJoystick = GameObject.Find("AttackJoystick").GetComponentInChildren<Joystick>();
        _extraAttackJoystick = GameObject.Find("ExtraAttackJoystick").GetComponentInChildren<Joystick>();
        _defenseButton = GameObject.Find("DefenseButton").GetComponentInChildren<Button>();

        _anim = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();

        _eye = transform.GetChild(0).GetChild(0);
    }

    void FixedUpdate() //Поменять на Update?
    {
        if (_view.IsMine) {
            Move(_moveJoystick.GetAxis());

            _lookDir = _attackJoystick.GetAxis();
            MoveEye();
        }
    }

    private void Move(Vector2 dir)
    {
        _body.velocity = Vector3.Lerp(_body.velocity, (dir + new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized) * speed, accel);
        _anim.SetFloat("speed", dir.magnitude * speed);
        //Поворот, если идет в другую сторону
        if ((dir.x > 0) == _sprite.flipX) Flip();
    }

    private void MoveEye()
    {
        _eye.localPosition = Vector3.ClampMagnitude(_lookDir, 0.15f);
    }

    private void Flip()
    {
        _sprite.flipX = !_sprite.flipX;
    }

    private void SimpleShot()
    {

    }

    private void ExtraAttack()
    {

    }
}
