using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private Transform myTrans;
    [SerializeField] private Color[] color;
    [SerializeField] private SkinnedMeshRenderer skinRenderer;
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform muzzlePos;


    [SerializeField] Camera cam;
    Animator animator;
    Rigidbody rigid;

    private bool isWalk;
    public bool IsWalk { get { return isWalk; } set { isWalk = value; OnWalk?.Invoke(isWalk); }  }
    public event Action<bool> OnWalk;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
    }
    private void OnEnable()
    {
        OnWalk += SetWalk;
    }
    private void OnDisable()
    {
        OnWalk -= SetWalk;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(color[PhotonNetwork.LocalPlayer.ActorNumber].r);
            stream.SendNext(color[PhotonNetwork.LocalPlayer.ActorNumber].g);
            stream.SendNext(color[PhotonNetwork.LocalPlayer.ActorNumber].b);
        }
        else
        {
            color[PhotonNetwork.LocalPlayer.ActorNumber].r = (float)stream.ReceiveNext();
            color[PhotonNetwork.LocalPlayer.ActorNumber].g = (float)stream.ReceiveNext();
            color[PhotonNetwork.LocalPlayer.ActorNumber].b = (float)stream.ReceiveNext();
        }
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            cam.gameObject.SetActive(true);

            cam = Camera.main;
        }
        else
        {
            cam.gameObject.SetActive(false);
        }

    }

    private void Update()
    {
        skinRenderer.material.color = color[PhotonNetwork.LocalPlayer.ActorNumber];
        if (photonView.IsMine)
        {
            Move(); 
            LookAround();
            if (Input.GetMouseButtonDown(0))
            {
                photonView.RPC("Fire", RpcTarget.AllViaServer,PhotonNetwork.LocalPlayer.ActorNumber, photonView.ViewID);
            }
        }
    }

    [PunRPC]
    private void Fire(int playerNum,int id, PhotonMessageInfo info)
    {
        float lag = Mathf.Abs((float)PhotonNetwork.Time - (float)info.SentServerTime);

        GameObject obj = Instantiate(bullet, muzzlePos.position, muzzlePos.rotation);
        obj.GetComponent<MeshRenderer>().material.color = color[playerNum];
        obj.GetComponent<Bullet>().Init(lag,id);
    }


    private void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 camForward = myTrans.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = myTrans.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 moveDir = camForward * z + camRight * x;

        IsWalk = moveDir.sqrMagnitude > 0.01f;

        rigid.MovePosition(rigid.position + moveDir.normalized * moveSpeed * Time.deltaTime);
    }

    public void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        Vector2 mouseDir = new Vector2(mouseX, mouseY);
        Vector3 camAngle = myTrans.rotation.eulerAngles;

        float x = camAngle.x - mouseDir.y;
        if (x < 180f)
        {
            x = Mathf.Clamp(x, -1f, 70);
        }
        else
        {
            x = Mathf.Clamp(x, 335f, 361f);
        }

        myTrans.rotation = Quaternion.Euler(x, camAngle.y + mouseDir.x, camAngle.z);
    }

    private void SetWalk(bool value)
    {
       animator.SetBool("IsWalk", value);
    }

}
