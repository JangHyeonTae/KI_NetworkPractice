using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomListItem : MonoBehaviour
{
    //���̸�
    [SerializeField] private TextMeshProUGUI roomNameText;
    //���� �ο�
    [SerializeField] private TextMeshProUGUI roomPlayerCountText;
    //��ư�� ���� �� ����
    [SerializeField] private Button joinButton;

    private string roomName;

    public void Init(RoomInfo info)
    {
        roomName = info.Name;
        roomNameText.text = $"Room Name : {roomName}";
        // ���� �� �÷��̾� ��/ �� �ִ� �÷��̾� ��
        roomPlayerCountText.text = $"{info.PlayerCount} / {info.MaxPlayers}";

        joinButton.onClick.AddListener(JoinRoom);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(roomName);
        joinButton.onClick.RemoveListener(JoinRoom);
    }

}
