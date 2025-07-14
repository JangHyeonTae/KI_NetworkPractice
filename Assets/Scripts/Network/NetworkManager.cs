using Photon.Pun;
using Photon.Pun.Demo.Cockpit;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{

    [Header("���� ��Ʈ��ũ ���� �� �ε�")]
    [SerializeField] private TextMeshProUGUI curStateText;
    [SerializeField] private GameObject loadingPanel;

    [Header("�г��� ����")]
    [SerializeField] private GameObject nickNamePanel;
    [SerializeField] private TMP_InputField nickNameInput;
    [SerializeField] private Button nickNameButton;

    [Header("�� ���� ���� - �κ�")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private TMP_InputField roomCreateInput;
    [SerializeField] private Button roomCreateButton;

    [Header("�� ����Ʈ ���� - �κ�")]
    [SerializeField] private GameObject roomItemPrefab;
    [SerializeField] private Transform roomItemParent;
    private Dictionary<string, GameObject> roomListDic = new();

    [SerializeField] private RoomManager roomManager;
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        nickNameButton.onClick.AddListener(SetNickname);
        roomCreateButton.onClick.AddListener(CreateRoom);
    }

    public override void OnConnectedToMaster()
    {
        if (loadingPanel.activeSelf)
        {
            loadingPanel.SetActive(false);
        }
        else
        {
            PhotonNetwork.JoinLobby();
        }
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        PhotonNetwork.ConnectUsingSettings();
    }

    private void SetNickname()
    {
        if (string.IsNullOrWhiteSpace(nickNameInput.text))
        {
            return;
        }

        PhotonNetwork.NickName = nickNameInput.text;
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        nickNamePanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomCreateInput.text))
        {
            return;
        }

        roomCreateButton.interactable = false;
        //�� ������ ������ �� �ִ� RoomOptions
        RoomOptions roomOptions = new RoomOptions() { MaxPlayers = 5 };

        //RoomListItem���� mapText�� ���� ����¸� �����ֱ����� customProperty����
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "Map" };
        PhotonNetwork.CreateRoom(roomCreateInput.text, roomOptions);
        roomCreateInput.text = null;
    }
    
    public override void OnCreatedRoom()
    {
        roomCreateButton.interactable = true;
        ExitGames.Client.Photon.Hashtable roomProperty = new();
        roomProperty["Map"] = 0;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperty);
    }

    public override void OnJoinedRoom()
    {
        lobbyPanel.SetActive(false);
        roomManager.PlayerPanelSpawn();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //���� �÷��̾ �濡 ������ ��
        if(newPlayer != PhotonNetwork.LocalPlayer)
            roomManager.PlayerPanelSpawn(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //�ٸ� �÷��̾ ��������?
        if (otherPlayer != PhotonNetwork.LocalPlayer)
        {
            roomManager.PlayerPanelDestroy(otherPlayer);
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                if(roomListDic.TryGetValue(info.Name, out GameObject obj))
                {
                    Destroy(obj);
                    roomListDic.Remove(info.Name);
                }

                continue;
            }

            // ��ųʸ����� ���� ���� �̸��� �ش��ϴ� ���� ���� ��
            // �ߺ��ؼ� ������� Instantiate�� �ΰ��� �Ǳ⶧���� �̹� ��������
            // ��ųʸ��� �ش� ������Ʈ���� init�� ���ش�.
            if (roomListDic.ContainsKey(info.Name))
            {
                // �÷��̾� ���� ����Ǵ� ���
                roomListDic[info.Name].GetComponent<RoomListItem>().Init(info);
            }
            else // �ش� ���� ������� �κ� ���� �����߰ų�, ���� ���� ���� ������
            {
                //���� �������
                GameObject obj = Instantiate(roomItemPrefab);
                obj.transform.SetParent(roomItemParent);
                obj.GetComponent<RoomListItem>().Init(info);
                roomListDic.Add(info.Name, obj);
            }
        }
    }

    //Room���� ������Ƽ ������Ʈ
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        roomManager.MapChange();
    }

    //Player���� ������Ƽ ������Ʈ
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        roomManager.playerPanelDic[targetPlayer.ActorNumber].ReadyCheck(targetPlayer);
    }

    //������ Ŭ���̾�Ʈ�� ����ɶ�
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        roomManager.PlayerPanelSpawn(newMasterClient);
    }

    private void Update()
    {
        curStateText.text = $"currentState : {PhotonNetwork.NetworkClientState}";
    }
}
