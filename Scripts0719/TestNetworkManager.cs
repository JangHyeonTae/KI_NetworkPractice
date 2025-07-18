using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class TestNetworkManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomOrCreateRoom();
    }

    public override void OnCreatedRoom()
    {
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("입장 완료");
        PhotonNetwork.LocalPlayer.NickName = $"Player_{PhotonNetwork.LocalPlayer.ActorNumber}";
        PlayerSpawn();
        if(PhotonNetwork.IsMasterClient)
         StartCoroutine(MonsterSpawn());
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (newMasterClient == PhotonNetwork.LocalPlayer)
        {
            StartCoroutine(MonsterSpawn());
        }
    }

    private IEnumerator MonsterSpawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            Vector3 spawnPos = new Vector3(Random.Range(-5, 5), 1, Random.Range(-5, 5));
            PhotonNetwork.InstantiateRoomObject("Monster", spawnPos, Quaternion.identity);
        }
    }

    private void PlayerSpawn()
    {
        Vector3 spawnPos = new Vector3(Random.Range(0, 5), 1, Random.Range(0, 5));
        PhotonNetwork.Instantiate("Player", spawnPos, Quaternion.identity);
    }
    public override void OnPlayerEnteredRoom(Player player)
    {
        Debug.Log($"{player.NickName} 입장 완료");
    }
}
