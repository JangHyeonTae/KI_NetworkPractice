using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum MapType
{
    City,
    Desert,
    Sea
}

public class RoomListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private TextMeshProUGUI mapText;
    [SerializeField] private Button joinButton;

    private string roomName;

    public void Init(RoomInfo info)
    {
        roomName = info.Name;
        roomNameText.text = $"Room : {roomName}";
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
