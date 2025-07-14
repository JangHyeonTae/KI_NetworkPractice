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

    [Header("현재 네트워크 상태 및 로딩")]
    [SerializeField] private TextMeshProUGUI curStateText;
    [SerializeField] private GameObject loadingPanel;

    [Header("닉네임 설정")]
    [SerializeField] private GameObject nickNamePanel;
    [SerializeField] private TMP_InputField nickNameInput;
    [SerializeField] private Button nickNameButton;

    [Header("방 생성 설정 - 로비")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private TMP_InputField roomCreateInput;
    [SerializeField] private Button roomCreateButton;

    [Header("방 리스트 설정 - 로비")]
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
        //방 생성시 설정할 수 있는 RoomOptions
        RoomOptions roomOptions = new RoomOptions() { MaxPlayers = 5 };

        //RoomListItem에서 mapText의 현재 방상태를 보여주기위해 customProperty설정
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
        //새로 플래이어가 방에 들어왔을 때
        if(newPlayer != PhotonNetwork.LocalPlayer)
            roomManager.PlayerPanelSpawn(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //다른 플레이어가 나갔을때?
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

            // 딕셔너리에서 원래 룸의 이름에 해당하는 방이 있을 때
            // 중복해서 있을경우 Instantiate가 두개가 되기때문에 이미 있을때는
            // 딕셔너리의 해당 오브젝트에서 init만 해준다.
            if (roomListDic.ContainsKey(info.Name))
            {
                // 플레이어 수가 변경되는 경우
                roomListDic[info.Name].GetComponent<RoomListItem>().Init(info);
            }
            else // 해당 방이 없을경우 로비에 새로 입장했거나, 방이 새로 생성 됐을때
            {
                //방을 만들어줌
                GameObject obj = Instantiate(roomItemPrefab);
                obj.transform.SetParent(roomItemParent);
                obj.GetComponent<RoomListItem>().Init(info);
                roomListDic.Add(info.Name, obj);
            }
        }
    }

    //Room관련 프로퍼티 업데이트
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        roomManager.MapChange();
    }

    //Player관련 프로퍼티 업데이트
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        roomManager.playerPanelDic[targetPlayer.ActorNumber].ReadyCheck(targetPlayer);
    }

    //마스터 클라이언트가 변경될때
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        roomManager.PlayerPanelSpawn(newMasterClient);
    }

    private void Update()
    {
        curStateText.text = $"currentState : {PhotonNetwork.NetworkClientState}";
    }
}
