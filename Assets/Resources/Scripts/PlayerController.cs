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
    }

    void FixedUpdate()
    {
        if (_view.IsMine) {
            Move(_moveJoystick.GetAxis());
        }
    }

    private void Move(Vector2 dir)
    {
        _body.velocity = Vector3.Lerp(_body.velocity, dir * speed, accel);
    }
}
