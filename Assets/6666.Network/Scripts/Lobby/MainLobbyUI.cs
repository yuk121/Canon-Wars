using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainLobbyUI : MonoBehaviour
{
    public PlayerSlotUI playerSlotPrefab;
    public GameObject sessionEndedUI;
    public GameObject kickedUI;
    public RectTransform playerScrollContent;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI lobbyCodeText;
    public Toggle readyToggle;
    public Button playButton;
    public Button copyCodeButton;
    public Button sessionEndedButton;
    public Button kickedButton;
    public Button backButton;

    List<PlayerSlotUI> _playerSlots = new();

    void Start()
    {
        LobbyManager instance = LobbyManager.Instance;

        // �غ�
        readyToggle.onValueChanged.AddListener((_) =>
        {
            readyToggle.interactable = false;
            instance.ReadyPlayer(readyToggle.isOn);
        });

        // ���� ����
        playButton.onClick.AddListener(() =>
        {
            instance.StartGameAsHost();
        });

        // �κ� �ڵ� ����
        copyCodeButton.onClick.AddListener(() =>
        {
            GUIUtility.systemCopyBuffer = lobbyCodeText.text;
        });

        // �κ� ������ ��� Ȯ�� ��ư
        sessionEndedButton.onClick.AddListener(() =>
        {
            sessionEndedUI.SetActive(false);
            LeaveMainLobbyUI();
        });

        // �߹�� ��� Ȯ�� ��ư
        kickedButton.onClick.AddListener(() =>
        {
            kickedUI.SetActive(false);
            LeaveMainLobbyUI();
        });
    }

    void OnEnable()
    {
        readyToggle.isOn = false;
        playButton.gameObject.SetActive(true);
        sessionEndedUI.SetActive(false);
        kickedUI.SetActive(false);

        // ���� ���� ��Ȱ��ȭ
        for (int i = 0; i < _playerSlots.Count; i++)
        {
            _playerSlots[i].gameObject.SetActive(false);
            _playerSlots[i].ShowReadyUI(false);
        }

        // �ڷΰ���
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.LeaveLobby();
        });
    }

    public void EnterMainLobbyUI(string lobbyName, string lobbyCode)
    {
        // �κ� ����
        LobbyManager instance = LobbyManager.Instance;
        instance.createLobbyUI.gameObject.SetActive(false);
        instance.sortLobbyUI.gameObject.SetActive(false);
        gameObject.SetActive(true);
        titleText.SetText(lobbyName);
        lobbyCodeText.SetText(lobbyCode);
        playButton.gameObject.SetActive(instance.IsLobbyHost);
        readyToggle.gameObject.SetActive(!instance.IsLobbyHost);
    }

    public void LeaveMainLobbyUI()
    {
        // �κ� ����
        LobbyManager.Instance.startLobbyUI.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    public void RefreshPlayersUI(int playerIndex, bool gameReady)
    {
        LobbyManager instance = LobbyManager.Instance;
        for (int i = 0; i < instance.LobbyPlayerDatas.Count || i < _playerSlots.Count; i++)
        {
            if (i >= _playerSlots.Count)
            {
                // ������ �����ϸ� ���� ����
                PlayerSlotUI playerSlot = Instantiate(playerSlotPrefab, playerScrollContent);
                _playerSlots.Add(playerSlot);
                ShowPlayerSlot(playerSlot, i);
            }
            else if (i < instance.LobbyPlayerDatas.Count)
            {
                // �ƴϸ� ���� ���� Ȱ��ȭ
                ShowPlayerSlot(_playerSlots[i], i);
            }

            // ���� ������ ��Ȱ��ȭ
            if (i >= instance.LobbyPlayerDatas.Count)
            {
                _playerSlots[i].gameObject.SetActive(false);
            }
        }

        float slotHeight = playerSlotPrefab.rectTransform.sizeDelta.y;
        playerScrollContent.sizeDelta = new Vector2(playerScrollContent.sizeDelta.x, instance.LobbyPlayerDatas.Count * slotHeight);
        readyToggle.interactable = readyToggle.isOn == LobbyManager.Instance.LobbyPlayerDatas[playerIndex].ready;
        playButton.interactable = gameReady;
    }

    public void ShowSessionEndedUI()
    {
        // �κ񿡼� �������� ���
        sessionEndedUI.SetActive(true);
    }

    public void ShowKickedUI()
    {
        // �߹�Ǿ��� ���
        kickedUI.SetActive(true);
    }

    public void HideOtherKickButtonsUI(int index)
    {
        // ������ ���� �÷��̾��� �߹� ��ư ��Ȱ��ȭ
        for (int i = 0; i < LobbyManager.Instance.LobbyPlayerDatas.Count; i++)
        {
            if (i != index)
            {
                _playerSlots[i].HideKickButtonUI();
            }
        }
    }

    void ShowPlayerSlot(PlayerSlotUI playerSlot, int i)
    {
        LobbyManager instance = LobbyManager.Instance;
        playerSlot.gameObject.SetActive(true);
        playerSlot.ShowPlayerNameUI(instance.LobbyPlayerDatas[i].name);
        playerSlot.ShowReadyUI(instance.LobbyPlayerDatas[i].ready);
    }
}
