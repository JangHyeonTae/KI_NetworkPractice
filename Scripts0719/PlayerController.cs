using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotSpeed;
    [SerializeField] private SkinnedMeshRenderer renderer;
    [SerializeField] private Color color;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform muzzlePoint;

    private Animator animator;
    private Vector3 networkPosition;
    private Quaternion networkRotation;

    private bool isWalk;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // stream.SendNext(transform.position);
           // stream.SendNext(transform.rotation);
            // stream.SendNext(isWalk);
            stream.SendNext(color.r);
            stream.SendNext(color.g);
            stream.SendNext(color.b);
        }
        else if (stream.IsReading)
        {
            // networkPosition = (Vector3)stream.ReceiveNext();
            // networkRotation = (Quaternion)stream.ReceiveNext();
            // isWalk = (bool)stream.ReceiveNext();
            color.r = (float)stream.ReceiveNext();
            color.g = (float)stream.ReceiveNext();
            color.b = (float)stream.ReceiveNext();
        }
    }

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        renderer.material.color = color;
        animator.SetBool("isWalk", isWalk);
        if (photonView.IsMine)
        {
            Move();

            if (Input.GetMouseButtonDown(0))
            {
                photonView.RPC("Fire", RpcTarget.AllViaServer);
            }
        }
        else
        {

            //transform.position = networkPosition;
            //transform.rotation = networkRotation;
          //  transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10);
          //  transform.rotation = Quaternion.Lerp(transform.rotation,networkRotation,Time.deltaTime * 10);
        }
    }

    [PunRPC]
    private void Fire(PhotonMessageInfo info)
    {
        float lag = Mathf.Abs((float)PhotonNetwork.Time - (float)info.SentServerTime);
        Debug.Log(lag);
        GameObject newBullet = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
        newBullet.GetComponent<Bullet>().ApplyLagCompensation(lag);
    }

    private void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 moveDir = new Vector3(x, 0, z).normalized;
        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);

        if (moveDir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir),
                rotSpeed * Time.deltaTime);
            isWalk = true;
        }
        else
        {
            isWalk = false;
        }
    }
}