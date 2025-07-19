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

    [Header("닉네임 관련")] [SerializeField] private GameObject nicknamePanel;
    [SerializeField] private TMP_InputField nicknameField;
    [SerializeField] private Button nicknameAdmitButton;

    [Header("로비 관련")] [SerializeField] private GameObject lobbyPanel;
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
        Debug.Log("마스터 연결");
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
            Debug.LogError("닉네임 입력값 없음");
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
        Debug.Log("로비 참가");
    }

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameField.text))
        {
            Debug.LogError("방 이름 입력 없음");
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
        Debug.Log("방 생성 완료");
    }

    public override void OnJoinedRoom()
    {
        lobbyPanel.SetActive(false);
        roomManager.PlayerPanelSpawn();
        Debug.Log("방 참가 완료");
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
        // 매개변수인 roomList를 순회
        foreach (RoomInfo info in roomList)
        {
            // 방이 삭제될때.
            if (info.RemovedFromList)
            {
                // 딕셔너리에 값이 있는지 확인.
                if (roomListItems.TryGetValue(info.Name, out GameObject obj))
                {
                    // 리스트 삭제
                    Destroy(obj);
                    // 딕셔너리에서 삭제
                    roomListItems.Remove(info.Name);
                }

                continue;
            }

            // roomListItems 에 해당 방이 있을때.
            if (roomListItems.ContainsKey(info.Name))
            {
                // 플레이어 수가 변경되는 경우.
                roomListItems[info.Name].GetComponent<RoomListItem>().Init(info);
            }
            else // 해당 방이 없을때. 로비에 새로 입장했거나, 방이 새로 생성되었을때.
            {
                // 방 리스트 오브젝트를 생성
                GameObject roomListItem = Instantiate(roomListItemPrefabs);
                // 스크롤뷰의 뷰포트에 넣어주는 작업
                roomListItem.transform.SetParent(roomListContent);
                // 초기화
                roomListItem.GetComponent<RoomListItem>().Init(info);
                // 딕셔너리에 해당 방 정보를 추가.
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