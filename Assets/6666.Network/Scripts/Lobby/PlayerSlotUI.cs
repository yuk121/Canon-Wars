using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSlotUI : MonoBehaviour
{
    public RectTransform rectTransform;
    public TextMeshProUGUI nameText;
    public Toggle readyToggle;
    public Button profileButton;
    public Button kickButton;

    void OnEnable()
    {
        LobbyManager instance = LobbyManager.Instance;
        HideKickButtonUI();
        int index = transform.GetSiblingIndex();

        // ������ ����
        profileButton.onClick.RemoveAllListeners();
        profileButton.onClick.AddListener(() =>
        {
            instance.mainLobbyUI.HideOtherKickButtonsUI(index);
            kickButton.gameObject.SetActive(!kickButton.gameObject.activeSelf);
            kickButton.interactable = instance.IsLobbyHost && !instance.IsPlayer(index);
        });

        // �߹�
        kickButton.onClick.RemoveAllListeners();
        kickButton.onClick.AddListener(() =>
        {
            instance.KickPlayer(index);
            HideKickButtonUI();
        });
    }

    public void ShowPlayerNameUI(string value)
    {
        nameText.SetText(value);
    }

    public void ShowReadyUI(bool value)
    {
        readyToggle.isOn = value;
    }

    public void HideKickButtonUI()
    {
        kickButton.gameObject.SetActive(false);
    }
}
