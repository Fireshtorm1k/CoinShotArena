using System;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PhotonPlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Slider healthBar;
    [SerializeField] private LayerMask ground;
    [SerializeField] private TMP_Text nameText;
    
   [NonSerialized] public string name;
   [NonSerialized] public int playerScore;

   private int hp = 100;
    private float speed = 5.0f;
    private Vector2 _touchDirection = Vector2.zero;

    #region MainComponents
    private SpriteRenderer _renderer;
    private PhotonView _photonView;
    private Rigidbody2D _rigidbody2D;
    private Animator _animator;
    private PlayerInput _input;
    #endregion
    
    private bool _isGrounded;
    
    public static Action OnPlayerDie;

    private void Start()    
    {
        _photonView = GetComponent<PhotonView>();
        _animator = GetComponent<Animator>();
        _renderer = GetComponent<SpriteRenderer>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _input = GetComponent<PlayerInput>();
        if (_photonView.IsMine)
        {
            _photonView.RPC("SetName", RpcTarget.AllBuffered, PhotonNetwork.NickName);
            SetRandomColor();
        }
    }

    void SetRandomColor()
    {
        float x = Random.value;
        float y = Random.value;
        float z = Random.value;
        Vector3 color = new Vector3(x, y, z);
        if (_photonView.IsMine)
        {
            _photonView.RPC("SetColor", RpcTarget.AllBuffered, color, _photonView.ViewID);
        }
    }
    [PunRPC]
    public void SetColor(Vector3 color, int viewID)
    {
        PhotonView view = PhotonView.Find(viewID);
        if(!view)return;
        view.GetComponent<SpriteRenderer>().color = new Color(color.x,color.y,color.z);
    }

    [PunRPC]
    void SetName(string name)
    {
        this.name = name;
        nameText.text = name;
    }
    private void Update()
    {
        if (!_photonView.IsMine) return;
        if (_input.actions["Shoot"].triggered)
        {
            Shoot();
        }
        if (_input.actions["Jump"].triggered && _isGrounded)
        {
            _rigidbody2D.AddForce(Vector2.up * 6, ForceMode2D.Impulse);
        }
    }


    private void FixedUpdate()
    {
        if(!_photonView.IsMine) return;
       _isGrounded= Physics2D.OverlapCircle(
            new Vector3(transform.position.x, transform.position.y - _renderer.bounds.extents.y, transform.position.z),
            0.2f, ground);
        HandleMovement();
    }
    
    [PunRPC]
    void ChangeDirection(bool direction)
    {
        _renderer.flipX = direction;
    }

    private void HandleMovement()
    {
        float horizontal = _input.actions["Move"].ReadValue<Vector2>().x;
        if (horizontal != 0)
        {
            if (_renderer.flipX != horizontal < 0)
            {
                _photonView.RPC("ChangeDirection",RpcTarget.All, horizontal<0);
            }
        }
        _animator.SetFloat("SpeedMulti", _rigidbody2D.velocity.x);
        _rigidbody2D.velocity = new Vector2(horizontal * speed, _rigidbody2D.velocity.y);
    }

    private void Shoot()
    {
        if (bulletPrefab && _photonView.IsMine)
        {
            if (_renderer.flipX)
            {
                PhotonNetwork.Instantiate(bulletPrefab.name, new Vector2(transform.position.x - 0.55f, transform.localPosition.y+0.33f), Quaternion.Euler(0,0,90));
            }
            else
            {
               PhotonNetwork.Instantiate(bulletPrefab.name, new Vector2(transform.position.x + 0.55f, transform.localPosition.y+0.33f), Quaternion.Euler(0,0,-90));
            }
        }
    }
    [PunRPC]
    public void TakeDamage(int damage)
    {
        if (!_photonView.IsMine) return;
        hp -= damage;
        if (hp <= 0)
        {
            _photonView.RPC("Die", RpcTarget.AllBuffered, _photonView.ViewID);
            return;
        }
        _photonView.RPC("UpdateHealth", RpcTarget.AllBuffered, hp);
    }
    [PunRPC]
    public void UpdateHealth(int newHp)
    {
        hp = newHp;
        healthBar.value = (float)newHp / 100;
    }

    [PunRPC]
    void Die(int viewId)
    {
        PhotonView view = PhotonView.Find(viewId);
        PhotonNetwork.RemoveBufferedRPCs(viewId, "TakeDamage");
        if (view.IsMine)
        {
            PhotonNetwork.Destroy(view);
        }
        OnPlayerDie?.Invoke();  
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coin") && other.gameObject.TryGetComponent(out PhotonView view))
        {
            if (_photonView.IsMine)
            {
                _photonView.RPC("DestroyCoin", RpcTarget.MasterClient, view.ViewID);
                playerScore += 1;
                _photonView.RPC("UpdateScore", RpcTarget.AllBuffered, playerScore);
            }
        }
    }
    [PunRPC]
    void DestroyCoin(int id)
    {
        PhotonView targetView = PhotonView.Find(id);
        if (targetView)
        {
            PhotonNetwork.Destroy(targetView.gameObject);
        }
    }
    [PunRPC]
    void UpdateScore(int newScore)
    {
        playerScore = newScore;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
        }
    }
}
