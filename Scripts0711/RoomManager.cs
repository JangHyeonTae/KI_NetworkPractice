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

    [Header("��")]
    [SerializeField] private Sprite[] mapSprite;
    [SerializeField] private GameObject map;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    private int curMapCount;

    //int�� Photon�� ActorNumber�� ��������
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
        // ���� �÷��̾ ���ο� �÷��̾� ���� �� ȣ��
        GameObject obj = Instantiate(playerPanelItemPrefabs);
        obj.transform.SetParent(playerPanelContent);
        PlayerPanelItem item = obj.GetComponent<PlayerPanelItem>();
        //�ʱ�ȭ
        item.Init(player);
        playerPanels.Add(player.ActorNumber, item);
    }

    public void PlayerPanelSpawn()
    {
        // ���� ����������
        if(!PhotonNetwork.IsMasterClient)
        {
            startButton.interactable = false;
            leftButton.interactable = false;
            rightButton.interactable = false;
        }

        // ���� ���� �������� �� ȣ��
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject obj = Instantiate(playerPanelItemPrefabs);
            obj.transform.SetParent(playerPanelContent);
            PlayerPanelItem item = obj.GetComponent<PlayerPanelItem>();
            //�ʱ�ȭ
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
            Debug.LogError("�г��� �������� ����");
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
