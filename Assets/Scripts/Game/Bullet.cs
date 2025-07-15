using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviourPun
{
    [SerializeField] private Rigidbody rigid;
    [SerializeField] private float bulletSpeed;
    private int playerID;

    private void Start()
    {
        rigid.AddForce(transform.forward * bulletSpeed, ForceMode.Impulse);
        if (gameObject != null)
        {
            Destroy(gameObject, 3f);
        }
    }
    public void Init(float lag, int id)
    {
        rigid.velocity = transform.forward * bulletSpeed;
        rigid.position += rigid.velocity * lag;
        playerID = id;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !photonView.IsMine && other.GetComponent<PhotonView>().ViewID != playerID)
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }
    }
}
