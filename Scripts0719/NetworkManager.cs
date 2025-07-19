using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI stateText;

    [Header("�г��� ����")] [SerializeField] private GameObject nicknamePanel;
    [SerializeField] private TMP_InputField nicknameField;
    [SerializeField] private Button nicknameAdmitButton;

    [Header("�κ� ����")] [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private TMP_InputField roomNameField;
    [SerializeField] private Button roomNameAdmitButton;
    [SerializeField] private GameObject roomListItemPrefabs;
    [SerializeField] private Transform roomListContent;
    private Dictionary<string, GameObject> roomListItems = new Dictionary<string, GameObject>();

    [SerializeField] private RoomManager roomManager;
    [SerializeField] private ChatManager chatManager;

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        nicknameAdmitButton.onClick.AddListener(NicknameAdmit);
        roomNameAdmitButton.onClick.AddListener(CreateRoom);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("������ ����");
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

    public void NicknameAdmit()
    {
        if (string.IsNullOrWhiteSpace(nicknameField.text))
        {
            Debug.LogError("�г��� �Է°� ����");
            return;
        }

        PhotonNetwork.NickName = nicknameField.text;
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        nicknamePanel.SetActive(false);
        lobbyPanel.SetActive(true);
        Debug.Log("�κ� ����");
    }

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameField.text))
        {
            Debug.LogError("�� �̸� �Է� ����");
            return;
        }

        roomNameAdmitButton.interactable = false;

        RoomOptions options = new RoomOptions { MaxPlayers = 8 };
        options.CustomRoomPropertiesForLobby = new string[] { "Map" };
        PhotonNetwork.CreateRoom(roomNameField.text, options);
        roomNameField.text = null;
    }

    public override void OnCreatedRoom()
    {
        roomNameAdmitButton.interactable = true;
        ExitGames.Client.Photon.Hashtable roomProperty = new Hashtable();
        roomProperty["Map"] = 0;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperty);
        Debug.Log("�� ���� �Ϸ�");
    }

    public override void OnJoinedRoom()
    {
        lobbyPanel.SetActive(false);
        roomManager.PlayerPanelSpawn();
        Debug.Log("�� ���� �Ϸ�");
    }

    public override void OnLeftRoom()
    {
        foreach (Transform tr in chatManager.ChatContent)
        {
            Destroy(tr.gameObject);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (newPlayer != PhotonNetwork.LocalPlayer)
            roomManager.PlayerPanelSpawn(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer != PhotonNetwork.LocalPlayer)
            roomManager.PlayerPanelDestroy(otherPlayer);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // �Ű������� roomList�� ��ȸ
        foreach (RoomInfo info in roomList)
        {
            // ���� �����ɶ�.
            if (info.RemovedFromList)
            {
                // ��ųʸ��� ���� �ִ��� Ȯ��.
                if (roomListItems.TryGetValue(info.Name, out GameObject obj))
                {
                    // ����Ʈ ����
                    Destroy(obj);
                    // ��ųʸ����� ����
                    roomListItems.Remove(info.Name);
                }

                continue;
            }

            // roomListItems �� �ش� ���� ������.
            if (roomListItems.ContainsKey(info.Name))
            {
                // �÷��̾� ���� ����Ǵ� ���.
                roomListItems[info.Name].GetComponent<RoomListItem>().Init(info);
            }
            else // �ش� ���� ������. �κ� ���� �����߰ų�, ���� ���� �����Ǿ�����.
            {
                // �� ����Ʈ ������Ʈ�� ����
                GameObject roomListItem = Instantiate(roomListItemPrefabs);
                // ��ũ�Ѻ��� ����Ʈ�� �־��ִ� �۾�
                roomListItem.transform.SetParent(roomListContent);
                // �ʱ�ȭ
                roomListItem.GetComponent<RoomListItem>().Init(info);
                // ��ųʸ��� �ش� �� ������ �߰�.
                roomListItems.Add(info.Name, roomListItem);
            }
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        roomManager.MapChange();
    }

    public override void OnPlayerPropertiesUpdate(Player target,
        ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        roomManager.playerPanels[target.ActorNumber].ReadyCheck(target);
    }

    public override void OnMasterClientSwitched(Player newClientPlayer)
    {
        roomManager.PlayerPanelSpawn(newClientPlayer);
    }

    private void Update()
    {
        stateText.text = $"Current State : {PhotonNetwork.NetworkClientState}";
    }
}