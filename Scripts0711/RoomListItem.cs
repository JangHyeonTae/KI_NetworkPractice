using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomListItem : MonoBehaviour
{
    //방이름
    [SerializeField] private TextMeshProUGUI roomNameText;
    //입장 인원
    [SerializeField] private TextMeshProUGUI roomPlayerCountText;
    //버튼을 통한 방 참가
    [SerializeField] private Button joinButton;

    private string roomName;

    public void Init(RoomInfo info)
    {
        roomName = info.Name;
        roomNameText.text = $"Room Name : {roomName}";
        // 현재 방 플레이어 수/ 방 최대 플레이어 수
        roomPlayerCountText.text = $"{info.PlayerCount} / {info.MaxPlayers}";

        joinButton.onClick.AddListener(JoinRoom);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(roomName);
        joinButton.onClick.RemoveListener(JoinRoom);
    }

}
