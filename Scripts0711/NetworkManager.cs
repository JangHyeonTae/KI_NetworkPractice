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
    [Header("로딩")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI stateText;

    [Header("닉네임 입력")]
    [SerializeField] private GameObject nickNamePanel;
    [SerializeField] private TMP_InputField nickNameField;
    [SerializeField] private Button nickNameAdmitButton;

    [Header("방 생성(로비)")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private TMP_InputField roomNameField;
    [SerializeField] private Button roomNameAdmitButton;

    [Header("방 리스트(로비)")]
    [SerializeField] private GameObject roomListItemPrefab;
    [SerializeField] private Transform roomListContent;
    private Dictionary<string, GameObject> roomListItemsDic = new();

    [Header("룸 매니저")]
    [SerializeField] private RoomManager roomManager;

    void Start()
    {
        //서버에 연결
        PhotonNetwork.ConnectUsingSettings();
        nickNameAdmitButton.onClick.AddListener(NicknameAdmit);
        roomNameAdmitButton.onClick.AddListener(CreateRoom);
    }

    //마스터 서버에 연결(ID가 마스터서버 주소)
    public override void OnConnectedToMaster()
    { 
        Debug.Log("마스터 연결");
        if(loadingPanel.activeSelf)
        {
            loadingPanel.SetActive(false);
        }
        else //방에서 나왔을 때 여기로온다?
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
            Debug.LogError(" 닉네임 미입력 입니다 ");
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

        RoomOptions options = new RoomOptions(){ MaxPlayers = 8};
        PhotonNetwork.CreateRoom(roomNameField.text, options);
        roomNameField.text = null;
    }

    public override void OnCreatedRoom()
    {
        roomNameAdmitButton.interactable = true;
        Debug.Log("방 생성 완료");
    }

    public override void OnJoinedRoom()
    {
        lobbyPanel.SetActive(false);
        roomManager.PlayerPanelSpawn();
        Debug.Log("방 참가 완료");
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
        // 매개변수를 순회
        foreach (RoomInfo info in roomList)
        {
            // 방이 삭제될때
            if(info.RemovedFromList)
            {
                // 딕셔너리에 값이 있는지 확인
                if (roomListItemsDic.TryGetValue(info.Name, out GameObject obj))
                {
                    // 리스트 삭제
                    Destroy(obj);
                    // 딕셔너리에서 삭제
                    roomListItemsDic.Remove(info.Name);
                }
                continue;
            }

            // roomListItems에 해당 방이 있을때
            if(roomListItemsDic.ContainsKey(info.Name))
            {
                // 플레이어 수가 변경되는 경우
                roomListItemsDic[info.Name].GetComponent<RoomListItem>().Init(info);
            }
            else // 해당 방이 없을때 로비에 새로 입장했거나, 방이 새로 생성되었을때
            {
                // 방 리스트 오브젝트를 생성
                GameObject roomListItem = Instantiate(roomListItemPrefab);
                // 스크롤뷰의 뷰포트에 넣어주는 작업
                roomListItem.transform.SetParent(roomListContent);
                // 초기화
                roomListItem.GetComponent<RoomListItem>().Init(info);
                // 딕셔너리에 해당 방 정보를 추가
                roomListItemsDic.Add(info.Name, roomListItem);
            }
        }
    }

    private void Update()
    {
        stateText.text = $"Current State : {PhotonNetwork.NetworkClientState}";
    }
}
