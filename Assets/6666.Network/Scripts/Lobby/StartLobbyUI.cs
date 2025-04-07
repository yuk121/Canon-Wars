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

        // ȣ��Ʈ
        hostButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            instance.createLobbyUI.gameObject.SetActive(true);
        });

        // ����
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
            // �ڷΰ���
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
