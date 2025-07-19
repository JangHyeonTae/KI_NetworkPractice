using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum MapType
{
    City,Desert,Sea
}

public class RoomListItem : MonoBehaviour
{
    // 방 이름
    [SerializeField] private TextMeshProUGUI roomNameText;
    // 입장 인원
    [SerializeField] private TextMeshProUGUI playerCountText;

    [SerializeField] private TextMeshProUGUI mapText;
    // 버튼을 통한 방 참가.
    [SerializeField] private Button joinButton;

    private string roomName;

    public void Init(RoomInfo info)
    {
        roomName = info.Name;
        roomNameText.text = $"Room Name : {roomName}";
        playerCountText.text = $"{info.PlayerCount} / {info.MaxPlayers}";
        mapText.text = $"Map : {(MapType)info.CustomProperties["Map"]}";
        
        joinButton.onClick.AddListener(JoinRoom);
    }

    public void JoinRoom()
    {
        if(PhotonNetwork.InLobby)
            PhotonNetwork.JoinRoom(roomName);
        
        joinButton.onClick.RemoveListener(JoinRoom);
    }
}
