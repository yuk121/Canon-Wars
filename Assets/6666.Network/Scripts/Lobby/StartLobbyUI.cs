using UnityEngine;
using UnityEngine.UI;

public class StartLobbyUI : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject background;
    public Button hostButton;
    public Button joinButton;
    public Button[] backButtons;

    void Start()
    {
        LobbyManager instance = LobbyManager.Instance;

        // 호스트
        hostButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            instance.createLobbyUI.gameObject.SetActive(true);
        });

        // 참가
        joinButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            instance.sortLobbyUI.gameObject.SetActive(true);
        });
    }

    void OnEnable()
    {
        background.SetActive(false);

        for (int i = 0; i < backButtons.Length; i++)
        {
            // 뒤로가기
            backButtons[i].onClick.RemoveAllListeners();
            backButtons[i].onClick.AddListener(() =>
            {
                startPanel.SetActive(false);
            });
        }
    }

    void OnDisable()
    {
        background.SetActive(true);
    }
}
