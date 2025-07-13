using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using Unity.VisualScripting;
using System;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPanelItemPrefabs;
    [SerializeField] private Transform playerPanelContent;

    [SerializeField] private Button startButton;
    [SerializeField] private Button leaveButton;

    [Header("맵")]
    [SerializeField] private Sprite[] mapSprite;
    [SerializeField] private GameObject map;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    private int curMapCount;

    //int에 Photon의 ActorNumber를 넣을거임
    private Dictionary<int, PlayerPanelItem> playerPanels = new();

    private void Start()
    {
        curMapCount = 0;
        map.GetComponent<Image>().sprite = mapSprite[curMapCount];
        leaveButton.onClick.AddListener(LeaveRoom);
        leftButton.onClick.AddListener(LeftMap);
        rightButton.onClick.AddListener(RightMap);
    }


    public void PlayerPanelSpawn(Player player)
    {
        // 기존 플레이어가 새로운 플레이어 입장 시 호출
        GameObject obj = Instantiate(playerPanelItemPrefabs);
        obj.transform.SetParent(playerPanelContent);
        PlayerPanelItem item = obj.GetComponent<PlayerPanelItem>();
        //초기화
        item.Init(player);
        playerPanels.Add(player.ActorNumber, item);
    }

    public void PlayerPanelSpawn()
    {
        // 내가 마스터인지
        if(!PhotonNetwork.IsMasterClient)
        {
            startButton.interactable = false;
            leftButton.interactable = false;
            rightButton.interactable = false;
        }

        // 내가 새로 입장했을 때 호출
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject obj = Instantiate(playerPanelItemPrefabs);
            obj.transform.SetParent(playerPanelContent);
            PlayerPanelItem item = obj.GetComponent<PlayerPanelItem>();
            //초기화
            item.Init(player);
            playerPanels.Add(player.ActorNumber, item);
        }
    }

    public void PlayerPanelDestroy(Player player)
    {
        if (playerPanels.TryGetValue(player.ActorNumber, out PlayerPanelItem panel))
        {
            Destroy(panel.gameObject);
            playerPanels.Remove(player.ActorNumber);
        }
        else
        {
            Debug.LogError("패널이 존재하지 않음");
        }
    }

    public void LeaveRoom()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Destroy(playerPanels[player.ActorNumber].gameObject);
        }

        playerPanels.Clear();

        PhotonNetwork.LeaveRoom();
    }

    public void LeftMap()
    {
        curMapCount--;
        if (curMapCount < 0)
        {
            curMapCount = mapSprite.Length-1;
        }
        map.GetComponent<Image>().sprite = mapSprite[curMapCount];
    }

    public void RightMap()
    {
        curMapCount++;
        if (curMapCount >= mapSprite.Length)
        {
            curMapCount = 0;
        }
        map.GetComponent<Image>().sprite = mapSprite[curMapCount];
    }

}
