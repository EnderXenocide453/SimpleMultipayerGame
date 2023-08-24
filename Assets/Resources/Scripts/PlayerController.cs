using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerController : MonoBehaviour, IPunObservable
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
    //Индикатор здоровья
    private Image _healthIndicator;

    //Направление взгляда
    private Vector2 _lookDir = Vector2.right;

    //Глаз
    private Transform _eye;
    private float _eyeOffsetY;
    private Joystick _eyeJoystick;

    //Монетки
    [SerializeField]
    private int _coinsCount = 0;

    //Переменные атаки
    private bool _isAttack = false;
    private Projectile _curProjectile;

    public delegate void AttackHandler();
    public event AttackHandler onAttackEnds;

    //Обычная атака
    [SerializeField]
    public Projectile primaryProjectile;
    public AttackHandler primaryAttack;

    //Особая атака
    [SerializeField]
    public Projectile extraProjectile;
    public AttackHandler extraAttack;

    //Скорость и ускорение
    public float speed = 4.0f;
    public float accel = 0.2f;

    //Здоровье, мана
    public float maxHealth = 100;
    public float maxMana = 100;

    private float _curHealth;
    private float _curMana;

    public int playerID;
    public GameManager manager;

    public delegate void DeathHandler();
    public event DeathHandler onDeath;

    void Start()
    {
        DontDestroyOnLoad(this);
        
        _view = GetComponent<PhotonView>();
        _body = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();

        _curHealth = maxHealth;
        _curMana = maxMana;

        //Установка видов атак
        primaryAttack = StartAutoAttack;
        extraAttack = StartAimShot;

        manager = GameObject.Find("GameManager").GetComponent<GameManager>();

        //Дальнейший код выполняется только для текущего игрока
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

        //Привязка атаки к событиям джойстиков
        _attackJoystick.onPress += PrimaryAttackBegun;
        _extraAttackJoystick.onPress += ExtraAttackBegun;
    }

    void FixedUpdate()
    {
        if (!(bool)PhotonNetwork.LocalPlayer.CustomProperties["isActive"]) return;

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
        if (!(bool)PhotonNetwork.LocalPlayer.CustomProperties["isActive"]) return;

        WorldProjectile proj = PhotonNetwork.Instantiate("Prefabs/Projectile", _eye.position + Vector3.up * _eyeOffsetY, Quaternion.identity).GetComponent<WorldProjectile>();
        proj.InitProjectile(_curProjectile, _lookDir.magnitude == 0 ? Vector2.right : _lookDir, playerID);

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
        yield return new WaitForEndOfFrame();

        while (_isAttack) {
            attackFunc.Invoke();

            yield return new WaitForSeconds(delay);
        }
    }

    public void Death()
    {
        _anim.speed = 0;

        if (_view.IsMine) {
            object[] datas = new object[] { playerID };
            PhotonNetwork.RaiseEvent(EventCodes.deathEvent, datas, Photon.Realtime.RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendUnreliable);
            _healthIndicator.rectTransform.localScale = Vector2.zero;
            _view.RPC("RPC_Death", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
            PhotonNetwork.LocalPlayer.CustomProperties["isActive"] = false;
            manager.RegistrateDeath(PhotonNetwork.LocalPlayer.ActorNumber);
        }
        _body.velocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
    }

    [PunRPC]
    public void RPC_Death(int id)
    {
        manager.RegistrateDeath(id);
    }

    //[PunRPC]
    //public void HandleDeath()
    //{
    //    view.RPC()
    //}

    public void TakeDamage(float amount) => _view.RPC("TakeDamageRPC", RpcTarget.All, amount);

    [PunRPC]
    public void TakeDamageRPC(float amount) => SetHP(_curHealth - amount);

    public void AddCoin()
    {
        SetCoin(_coinsCount + 1);
    }
    public int GetCoin() => _coinsCount;
    private void SetCoin(int amount)
    {
        if (_view.IsMine) {
            _coinsCount = amount;

            if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("coins")) {
                ExitGames.Client.Photon.Hashtable prop = new ExitGames.Client.Photon.Hashtable();
                prop.Add("coins", amount);
                PhotonNetwork.SetPlayerCustomProperties(prop);
            } else
                PhotonNetwork.LocalPlayer.CustomProperties["coins"] = amount;
        }
    }

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
            stream.SendNext(playerID);

        } else {
            SetHP((float)stream.ReceiveNext());
            SetCoin((int)stream.ReceiveNext());
            int id = (int)stream.ReceiveNext();
            playerID = id == 0 ? playerID : id;
        }
    }
}

//public static class GlobalInfo
//{
//    public static Dictionary<int, PlayerController> _players { get; private set; }
//}