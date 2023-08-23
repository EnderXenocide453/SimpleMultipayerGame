using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    //PhotonView �������� ������
    private PhotonView _view;
    //���������� ���� ���������
    private Rigidbody2D _body;

    //�������� �����������
    private Joystick _moveJoystick;
    //�������� ������� �����
    private Joystick _attackJoystick;
    //�������� ���������� �����
    private Joystick _extraAttackJoystick;
    //������ �����
    private Button _defenseButton;

    //�������� ���������
    private Animator _anim;
    //������ ���������
    private SpriteRenderer _sprite;

    //����������� �������
    private Vector2 _lookDir = Vector2.right;

    //����
    private Transform _eye;
    private Joystick _eyeJoystick;

    //�������� � ���������
    public float speed = 4.0f;
    public float accel = 0.2f;

    //��������, ����
    public float maxHealth = 100;
    public float maxMana = 100;

    private float _curHealth;
    private float _curMana;

    //���������� �����
    public delegate void AttackHandler();
    public event AttackHandler onAttackEnds;

    private bool _isAttack = false;
    private Projectile _curProjectile;

    //������� �����
    [SerializeField]
    public Projectile primaryProjectile;
    public AttackHandler primaryAttack;

    //������ �����
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

        //��������� ����� ����
        primaryAttack = StartAutoAttack;
        extraAttack = StartAimShot;

        //�������� ����� � �������� ����������
        _attackJoystick.onPress += PrimaryAttackBegun;
        _extraAttackJoystick.onPress += ExtraAttackBegun;
    }

    void FixedUpdate()
    {
        //���� �������� ��� ����������� �������� ������
        if (_view.IsMine) {
            //����������� �������� ���������
            Move(_moveJoystick.GetAxis());

            _lookDir = _eyeJoystick.GetAxis();
            MoveEye();
        }
    }

    //����������� ���������
    private void Move(Vector2 dir)
    {
        //��������� �������� �������� ��������� accel
        _body.velocity = Vector3.Lerp(_body.velocity, dir * speed, accel);
        //�������� �������� � �������� ��� ����������� �������� ����� ��� ������
        _anim.SetFloat("speed", dir.magnitude * speed);

        //�������, ���� ���� � ������ �������
        if ((dir.x > 0) == _sprite.flipX) Flip();
    }

    //�������� �����
    private void MoveEye()
    {
        _eye.localPosition = Vector3.ClampMagnitude(_lookDir, 0.15f);
    }

    //������� �� ��� X
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
        //����� ������� �� ��������� �� ����� ������ �����
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
        //����� ������� �� ��������� �� ����� ������ �����
        _extraAttackJoystick.onRelease -= ExtraAttackEnds;
        onAttackEnds.Invoke();

        _isAttack = false;
    }

    private void SimpleShot()
    {
        WorldProjectile proj = PhotonNetwork.Instantiate("Prefabs/Projectile", _eye.position, Quaternion.identity).GetComponent<WorldProjectile>();
        proj.InitProjectile(_curProjectile, _lookDir.magnitude == 0 ? _moveJoystick.GetAxis() : _lookDir);

        //������������ �� ������� ��������� ����� �� ������ ���������� ����������� ��������
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