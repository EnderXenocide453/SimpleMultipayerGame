using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerController : MonoBehaviour, IPunObservable
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
    //��������� ��������
    private Image _healthIndicator;

    //����������� �������
    private Vector2 _lookDir = Vector2.right;

    //����
    private Transform _eye;
    private float _eyeOffsetY;
    private Joystick _eyeJoystick;

    //�������
    [SerializeField]
    private int _coinsCount = 0;

    //���������� �����
    private bool _isAttack = false;
    private Projectile _curProjectile;

    public delegate void AttackHandler();
    public event AttackHandler onAttackEnds;

    //������� �����
    [SerializeField]
    public Projectile primaryProjectile;
    public AttackHandler primaryAttack;

    //������ �����
    [SerializeField]
    public Projectile extraProjectile;
    public AttackHandler extraAttack;

    //�������� � ���������
    public float speed = 4.0f;
    public float accel = 0.2f;

    //��������, ����
    public float maxHealth = 100;
    public float maxMana = 100;

    private float _curHealth;
    private float _curMana;

    public int playerID;

    void Start()
    {
        DontDestroyOnLoad(this);
        
        _view = GetComponent<PhotonView>();
        _body = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();

        _curHealth = maxHealth;
        _curMana = maxMana;

        //��������� ����� ����
        primaryAttack = StartAutoAttack;
        extraAttack = StartAimShot;

        //���������� ��� ����������� ������ ��� �������� ������
        if (!_view.IsMine) return;

        playerID = PhotonNetwork.LocalPlayer.ActorNumber;

        _moveJoystick = GameObject.Find("MoveJoystick").GetComponentInChildren<Joystick>();
        _healthIndicator = GameObject.Find("HelthBarHolder").GetComponentInChildren<Image>();
        _attackJoystick = GameObject.Find("AttackJoystick").GetComponentInChildren<Joystick>();
        _extraAttackJoystick = GameObject.Find("ExtraAttackJoystick").GetComponentInChildren<Joystick>();
        _defenseButton = GameObject.Find("DefenseButton").GetComponentInChildren<Button>();

        _eye = transform.GetChild(0).GetChild(0);
        Sprite s = _eye.GetComponent<SpriteRenderer>().sprite;
        _eyeOffsetY = (s.rect.height / 2 - s.pivot.y) / s.pixelsPerUnit + _eye.parent.localPosition.y;
        Debug.Log(_eyeOffsetY);
        _eyeJoystick = _moveJoystick;

        //�������� ����� � �������� ����������
        _attackJoystick.onPress += PrimaryAttackBegun;
        _extraAttackJoystick.onPress += ExtraAttackBegun;
    }

    void FixedUpdate()
    {
        if (!(bool)PhotonNetwork.LocalPlayer.CustomProperties["isActive"]) return;

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
        if (!(bool)PhotonNetwork.LocalPlayer.CustomProperties["isActive"]) return;

        WorldProjectile proj = PhotonNetwork.Instantiate("Prefabs/Projectile", _eye.position + Vector3.up * _eyeOffsetY, Quaternion.identity).GetComponent<WorldProjectile>();
        proj.InitProjectile(_curProjectile, _lookDir.magnitude == 0 ? Vector2.right : _lookDir, playerID);

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
        yield return new WaitForEndOfFrame();

        while (_isAttack) {
            attackFunc.Invoke();

            yield return new WaitForSeconds(delay);
        }
    }

    public void Death()
    {
        //if (!view.IsMine) return;

        _anim.speed = 0;
        _body.velocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        _healthIndicator.rectTransform.localScale = Vector2.zero;

        this.enabled = false;
    }

    //[PunRPC]
    //public void HandleDeath()
    //{
    //    view.RPC()
    //}

    public void TakeDamage(float amount) => _view.RPC("TakeDamageRPC", RpcTarget.All, amount);

    [PunRPC]
    public void TakeDamageRPC(float amount) => SetHP(_curHealth - amount);

    public void AddCoin() => _coinsCount++;
    public int GetCoin() => _coinsCount;
    private void SetCoin(int amount) => _coinsCount = amount;

    private void SetHP(float hp)
    {
        _curHealth = Mathf.Clamp(hp, 0, maxHealth);

        if (_view.IsMine) _healthIndicator.transform.localScale = new Vector3(_curHealth / maxHealth, 1, 1);

        if (_curHealth <= 0)
            _anim.SetTrigger("death");
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) {
            stream.SendNext(_curHealth);
            stream.SendNext(_coinsCount);

        } else {
            SetHP((float)stream.ReceiveNext());
            SetCoin((int)stream.ReceiveNext());
        }
    }
}

//public static class GlobalInfo
//{
//    public static Dictionary<int, PlayerController> _players { get; private set; }
//}