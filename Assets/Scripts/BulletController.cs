using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public int damage = 10;
    private float speed = 10;
    private Rigidbody2D rb;
    private PhotonView _photonView;

    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody2D>();
        Move();
    }
     void Move()
    {
        rb.velocity =  transform.up * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_photonView.IsMine) return;
        if (other.gameObject.TryGetComponent<PhotonPlayerController>(out var controller))
        {
            controller.photonView.RPC("TakeDamage", RpcTarget.All, damage);
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}