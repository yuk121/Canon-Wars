using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbySlotUI : MonoBehaviour
{
    public RectTransform rectTransform;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI gameModeText;
    public Button joinButton;

    void OnEnable()
    {
        LobbyManager instance = LobbyManager.Instance;

        // 참가 버튼
        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(() =>
        {
            instance.JoinLobby(null, transform.GetSiblingIndex());
        });
    }

    public void ShowLobbyUI(string name, string gameMode)
    {
        nameText.SetText(name);
        gameModeText.SetText(gameMode);
    }
}
