using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;
using Hashtable = ExitGames.Client.Photon.Hashtable;

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
        Debug.Log("로컬");
        ReadyPropertyUpdate();
        
        readyButton.onClick.RemoveListener(ReadyButtonClick);
        readyButton.onClick.AddListener(ReadyButtonClick);
    }

    public void ReadyButtonClick()
    {
        isReady = !isReady;

        readyText.text = isReady ? "Ready" : "Click Ready";
        readyButtonImage.color = isReady ? Color.green : Color.grey;
        ReadyPropertyUpdate();
    }

    public void ReadyPropertyUpdate()
    {
        ExitGames.Client.Photon.Hashtable playerProperty = new Hashtable();
        playerProperty["Ready"] = isReady;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperty);
    }

    public void ReadyCheck(Player player)
    {
        if (player.CustomProperties.TryGetValue("Ready", out object value))
        {
            readyText.text = (bool)value ? "Ready" : "Click Ready";
            readyButtonImage.color = (bool)value ? Color.green : Color.grey;
        }
    }
    
}
