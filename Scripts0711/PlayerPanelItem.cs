using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class PlayerPanelItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nickNameText;
    [SerializeField] private TextMeshProUGUI readyText;

    [SerializeField] private Image hostImage;
    [SerializeField] private Image readyButtonImage;
    [SerializeField] private Button readyButton;

    public void Init(Player player)
    {
        nickNameText.text = player.NickName;
        hostImage.enabled = player.IsMasterClient;
        readyButton.interactable = player.IsLocal;
    }
}
