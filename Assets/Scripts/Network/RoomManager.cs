using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPanelPrefab;
    [SerializeField] private Transform playerPanelParent;

    [SerializeField] private Button startButton;
    [SerializeField] private Button leaveButton;

    [SerializeField] private Button mapLeftButton;
    [SerializeField] private Button mapRightButton;
    [SerializeField] private Sprite[] mapSprite;
    [SerializeField] private Image mapImage;


    public Dictionary<int, PlayerPanelItem> playerPanelDic = new();


    private int curMapIndex;
    private void Start()
    {
        startButton.onClick.AddListener(GameStart);
        leaveButton.onClick.AddListener(LeaveRoom);
        mapLeftButton.onClick.AddListener(ClickMapLeft);
        mapRightButton.onClick.AddListener(ClickMapRight);
    }

    public void PlayerPanelSpawn(Player player)
    {
        if (playerPanelDic.TryGetValue(player.ActorNumber, out PlayerPanelItem panelItem))
        {
            startButton.interactable = true;
            mapLeftButton.interactable = true;
            mapRightButton.interactable = true;
            panelItem.Init(player);

            return;
        }

        // ���� �÷��̾ ���ο� �÷��̾� ���� �� ȣ��
        GameObject obj = Instantiate(playerPanelPrefab);
        obj.transform.SetParent(playerPanelParent);
        PlayerPanelItem item = obj.GetComponent<PlayerPanelItem>();
        item.Init(player);
        playerPanelDic.Add(player.ActorNumber, item);
    }

    // ��� ��� �÷��̾���� �����ϴ� �Լ���
    // �׷��� ȭ����ȯ PhotonNetwork.AutomaticallySyncScene �̰� ����
    // .true�� ��� �÷��̾� ȭ����ȯ
    public void PlayerPanelSpawn()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        // ���� ���� �������� �� ȣ��
        if (!PhotonNetwork.IsMasterClient)
        {
            startButton.interactable = false;
            mapLeftButton.interactable = false;
            mapRightButton.interactable = false;
            MapChange();
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject obj = Instantiate(playerPanelPrefab);
            obj.transform.SetParent(playerPanelParent);
            PlayerPanelItem item = obj.GetComponent<PlayerPanelItem>();
            item.Init(player);
            playerPanelDic.Add(player.ActorNumber, item);
        }
    }

    public void PlayerPanelDestroy(Player player)
    {
        if (playerPanelDic.TryGetValue(player.ActorNumber, out PlayerPanelItem obj))
        {
            Destroy(obj.gameObject);
            playerPanelDic.Remove(player.ActorNumber);
        }
        else
        {
            Debug.LogError("�г� ����");
        }
    }

    public void LeaveRoom()
    {
        // �׳� PhotonNetwork.LeaveRoom�� �Ұ�� �׳� ������ �����ٶ� �����Ͱ� ��������
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Destroy(playerPanelDic[player.ActorNumber].gameObject);
        }

        playerPanelDic.Clear();

        PhotonNetwork.LeaveRoom();
    }

    public void GameStart()
    {
        if (PhotonNetwork.IsMasterClient && AllPlayerReady())
        {
            PhotonNetwork.LoadLevel("GameScene");
            Debug.Log("Click");
        }
    }

    public bool AllPlayerReady()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!player.CustomProperties.TryGetValue("Ready", out object value) || !(bool)value)
            {
                return false;
            }
        }
        return true;
    }

    public void ClickMapLeft()
    {
        curMapIndex--;
        if (curMapIndex <= -1)
        {
            curMapIndex = mapSprite.Length - 1;
        }

        ExitGames.Client.Photon.Hashtable roomProperty = new ExitGames.Client.Photon.Hashtable();
        roomProperty["Map"] = curMapIndex;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperty);

        MapChange();
    }

    public void ClickMapRight()
    {
        curMapIndex++;
        if (curMapIndex >= mapSprite.Length)
        {
            curMapIndex = 0;
        }

        ExitGames.Client.Photon.Hashtable roomProperty = new ExitGames.Client.Photon.Hashtable();
        roomProperty["Map"] = curMapIndex;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperty);

        MapChange();
    }

    public void MapChange()
    {
        curMapIndex = (int)PhotonNetwork.CurrentRoom.CustomProperties["Map"];
        mapImage.sprite = mapSprite[curMapIndex];
    }
}
