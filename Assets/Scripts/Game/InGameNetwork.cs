using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEngine;

public class InGameNetwork : MonoBehaviourPunCallbacks
{
    //[SerializeField] private Material mat;
    void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            PlayerSpawn();
        }
        else if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }


    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        //if (newMasterClient == PhotonNetwork.LocalPlayer)
        //{
        //   StartCoroutine(MonsterSpawn());
        //}
    }


    private void PlayerSpawn()
    {
        Vector3 spawnPos = new Vector3(Random.Range(0, 3), 0, Random.Range(0, 3));
        GameObject obj = PhotonNetwork.Instantiate("Player", spawnPos,Quaternion.identity);
        
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player_{newPlayer.ActorNumber} ¿‘¿Â");
    }

    //private IEnumerator MonsterSpawn()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(2f);
    //        Vector3 randPos = new Vector3(Random.Range(0, 5), 1, Random.Range(0, 5));
    //        PhotonNetwork.InstantiateRoomObject("Monster", randPos,Quaternion.identity);
    //    }
    //}
}
