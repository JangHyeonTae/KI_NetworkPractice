using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class PlayerPanelItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI readyText;
    [SerializeField] private Image hostImage;
    [SerializeField] private Image readyButtonImage;
    [SerializeField] private Button readyButton;

    private bool isReady;
    public void Init(Player player)
    {
        nicknameText.text = player.NickName;
        hostImage.enabled = player.IsMasterClient;
        readyButton.interactable = player.IsLocal;

        if (!player.IsLocal)
            return;

        isReady = false;
        //프로퍼티 업데이트를 왜 init마다 해주는지
        ReadyPropertyUpdate();

        readyButton.onClick.RemoveListener(ReadyButtonClick);
        readyButton.onClick.AddListener(ReadyButtonClick);
    }


    public void ReadyButtonClick()
    {
        isReady = !isReady;

        readyText.text = isReady ? "Ready" : "Click Ready";
        readyButtonImage.color = isReady ? Color.green : Color.gray;
        ReadyPropertyUpdate();
    }

    public void ReadyPropertyUpdate()
    {
        ExitGames.Client.Photon.Hashtable playerProperty = new();
        playerProperty["Ready"] = isReady;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperty);
    }

    public void ReadyCheck(Player player)
    {
        //만약 현재 플레이어가 Ready라는 프로퍼티를 가지고있지 않을경우 문제가 생김
        if (player.CustomProperties.TryGetValue("Ready", out object value))
        {
            readyText.text = (bool)value ? "Ready" : "Click Ready";
            readyButtonImage.color = (bool)value ? Color.green : Color.gray;
        }
    }
}
