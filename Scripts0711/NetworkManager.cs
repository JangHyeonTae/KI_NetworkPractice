using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using Photon.Pun.Demo.Cockpit;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("�ε�")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI stateText;

    [Header("�г��� �Է�")]
    [SerializeField] private GameObject nickNamePanel;
    [SerializeField] private TMP_InputField nickNameField;
    [SerializeField] private Button nickNameAdmitButton;

    [Header("�� ����(�κ�)")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private TMP_InputField roomNameField;
    [SerializeField] private Button roomNameAdmitButton;

    [Header("�� ����Ʈ(�κ�)")]
    [SerializeField] private GameObject roomListItemPrefab;
    [SerializeField] private Transform roomListContent;
    private Dictionary<string, GameObject> roomListItemsDic = new();

    [Header("�� �Ŵ���")]
    [SerializeField] private RoomManager roomManager;

    void Start()
    {
        //������ ����
        PhotonNetwork.ConnectUsingSettings();
        nickNameAdmitButton.onClick.AddListener(NicknameAdmit);
        roomNameAdmitButton.onClick.AddListener(CreateRoom);
    }

    //������ ������ ����(ID�� �����ͼ��� �ּ�)
    public override void OnConnectedToMaster()
    { 
        Debug.Log("������ ����");
        if(loadingPanel.activeSelf)
        {
            loadingPanel.SetActive(false);
        }
        else //�濡�� ������ �� ����ο´�?
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
        if (string.IsNullOrWhiteSpace(nickNameField.text))
        {
            Debug.LogError(" �г��� ���Է� �Դϴ� ");
            return;
        }

        PhotonNetwork.NickName = nickNameField.text;
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        nickNamePanel.SetActive(false);
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

        RoomOptions options = new RoomOptions(){ MaxPlayers = 8};
        PhotonNetwork.CreateRoom(roomNameField.text, options);
        roomNameField.text = null;
    }

    public override void OnCreatedRoom()
    {
        roomNameAdmitButton.interactable = true;
        Debug.Log("�� ���� �Ϸ�");
    }

    public override void OnJoinedRoom()
    {
        lobbyPanel.SetActive(false);
        roomManager.PlayerPanelSpawn();
        Debug.Log("�� ���� �Ϸ�");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if(newPlayer != PhotonNetwork.LocalPlayer)
            roomManager.PlayerPanelSpawn(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer != PhotonNetwork.LocalPlayer)
            roomManager.PlayerPanelDestroy(otherPlayer);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // �Ű������� ��ȸ
        foreach (RoomInfo info in roomList)
        {
            // ���� �����ɶ�
            if(info.RemovedFromList)
            {
                // ��ųʸ��� ���� �ִ��� Ȯ��
                if (roomListItemsDic.TryGetValue(info.Name, out GameObject obj))
                {
                    // ����Ʈ ����
                    Destroy(obj);
                    // ��ųʸ����� ����
                    roomListItemsDic.Remove(info.Name);
                }
                continue;
            }

            // roomListItems�� �ش� ���� ������
            if(roomListItemsDic.ContainsKey(info.Name))
            {
                // �÷��̾� ���� ����Ǵ� ���
                roomListItemsDic[info.Name].GetComponent<RoomListItem>().Init(info);
            }
            else // �ش� ���� ������ �κ� ���� �����߰ų�, ���� ���� �����Ǿ�����
            {
                // �� ����Ʈ ������Ʈ�� ����
                GameObject roomListItem = Instantiate(roomListItemPrefab);
                // ��ũ�Ѻ��� ����Ʈ�� �־��ִ� �۾�
                roomListItem.transform.SetParent(roomListContent);
                // �ʱ�ȭ
                roomListItem.GetComponent<RoomListItem>().Init(info);
                // ��ųʸ��� �ش� �� ������ �߰�
                roomListItemsDic.Add(info.Name, roomListItem);
            }
        }
    }

    private void Update()
    {
        stateText.text = $"Current State : {PhotonNetwork.NetworkClientState}";
    }
}
