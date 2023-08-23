using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    //PhotonView текущего игрока
    private PhotonView _view;
    //Физическое тело персонажа
    private Rigidbody2D _body;

    //Джойстик перемещения
    private Joystick _moveJoystick;
    //Джойстик обычной атаки
    private Joystick _attackJoystick;
    //Джойстик прицельной атаки
    private Joystick _extraAttackJoystick;
    //Кнопка блока
    private Button _defenseButton;

    //Аниматор персонажа
    private Animator _anim;
    //Спрайт персонажа
    private SpriteRenderer _sprite;

    //Направление взгляда
    private Vector2 _lookDir = Vector2.right;

    //Глаз
    private Transform _eye;
    private Joystick _eyeJoystick;

    //Скорость и ускорение
    public float speed = 4.0f;
    public float accel = 0.2f;

    //Здоровье, мана
    public float maxHealth = 100;
    public float maxMana = 100;

    private float _curHealth;
    private float _curMana;

    //Переменные атаки
    public delegate void AttackHandler();
    public event AttackHandler onAttackEnds;

    private bool _isAttack = false;
    private Projectile _curProjectile;

    //Обычная атака
    [SerializeField]
    public Projectile primaryProjectile;
    public AttackHandler primaryAttack;

    //Особая атака
    [SerializeField]
    public Projectile extraProjectile;
    public AttackHandler extraAttack;

    void Start()
    {
        DontDestroyOnLoad(this);

        _view = GetComponent<PhotonView>();
        _body = GetComponent<Rigidbody2D>();

        _moveJoystick = GameObject.Find("MoveJoystick").GetComponentInChildren<Joystick>();
        _attackJoystick = GameObject.Find("AttackJoystick").GetComponentInChildren<Joystick>();
        _extraAttackJoystick = GameObject.Find("ExtraAttackJoystick").GetComponentInChildren<Joystick>();
        _defenseButton = GameObject.Find("DefenseButton").GetComponentInChildren<Button>();

        _anim = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();

        _eye = transform.GetChild(0).GetChild(0);
        _eyeJoystick = _moveJoystick;

        //Установка видов атак
        primaryAttack = StartAutoAttack;
        extraAttack = StartAimShot;

        //Привязка атаки к событиям джойстиков
        _attackJoystick.onPress += PrimaryAttackBegun;
        _extraAttackJoystick.onPress += ExtraAttackBegun;
    }

    void FixedUpdate()
    {
        //Если персонаж под управлением текущего игрока
        if (_view.IsMine) {
            //Переместить согласно джойстику
            Move(_moveJoystick.GetAxis());

            _lookDir = _eyeJoystick.GetAxis();
            MoveEye();
        }
    }

    //Перемещение персонажа
    private void Move(Vector2 dir)
    {
        //Изменение скорости соглавно ускорению accel
        _body.velocity = Vector3.Lerp(_body.velocity, dir * speed, accel);
        //Передача скорости в аниматор для определения анимации покоя или ходьбы
        _anim.SetFloat("speed", dir.magnitude * speed);

        //Поворот, если идет в другую сторону
        if ((dir.x > 0) == _sprite.flipX) Flip();
    }

    //Вращение глаза
    private void MoveEye()
    {
        _eye.localPosition = Vector3.ClampMagnitude(_lookDir, 0.15f);
    }

    //Поворот по оси X
    private void Flip()
    {
        _sprite.flipX = !_sprite.flipX;
    }

    private void PrimaryAttackBegun()
    {
        if (_isAttack) return;

        _eyeJoystick = _attackJoystick;

        _isAttack = true;
        _curProjectile = primaryProjectile;
        _attackJoystick.onRelease += PrimaryAttackEnds;

        primaryAttack.Invoke();
    }

    private void PrimaryAttackEnds()
    {
        //Чтобы функция не сработала во время особой атаки
        _attackJoystick.onRelease -= PrimaryAttackEnds;
        onAttackEnds?.Invoke();

        _isAttack = false;
    }

    private void ExtraAttackBegun()
    {
        if (_isAttack) return;

        _eyeJoystick = _extraAttackJoystick;

        _isAttack = true;
        _curProjectile = extraProjectile;
        _extraAttackJoystick.onRelease += ExtraAttackEnds;

        extraAttack?.Invoke();
    }

    private void ExtraAttackEnds()
    {
        //Чтобы функция не сработала во время особой атаки
        _extraAttackJoystick.onRelease -= ExtraAttackEnds;
        onAttackEnds.Invoke();

        _isAttack = false;
    }

    private void SimpleShot()
    {
        WorldProjectile proj = PhotonNetwork.Instantiate("Prefabs/Projectile", _eye.position, Quaternion.identity).GetComponent<WorldProjectile>();
        proj.InitProjectile(_curProjectile, _lookDir.magnitude == 0 ? _moveJoystick.GetAxis() : _lookDir);

        //Отписываемся от события окончания атаки на случай применения прицельного выстрела
        onAttackEnds -= SimpleShot;
    }

    private void StartAutoAttack()
    {
        StartCoroutine(CycleAttack(SimpleShot, _curProjectile.cooldown));
    }

    private void StartAimShot()
    {
        onAttackEnds += SimpleShot;
    }

    private IEnumerator CycleAttack(AttackHandler attackFunc, float delay)
    {
        while (_isAttack) {
            attackFunc.Invoke();

            yield return new WaitForSeconds(delay);
        }
    }
}

//public static class GlobalInfo
//{
//    public static Dictionary<int, PlayerController> _players { get; private set; }
//}