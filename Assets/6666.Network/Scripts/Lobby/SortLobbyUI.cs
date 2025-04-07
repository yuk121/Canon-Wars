using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SortLobbyUI : MonoBehaviour
{
    public LobbySlotUI lobbySlotPrefab;
    public GameObject joinFailedUI;
    public RectTransform lobbyScrollContent;
    public TMP_Dropdown gameModeDropDown;
    public TMP_InputField lobbyCodeInputField;
    public Button refreshButton;
    public Button joinPrivateButton;
    public Button joinFailConfirmButton;
    public Button backButton;

    List<LobbySlotUI> _lobbySlots = new();

    EGameMode GameMode => (EGameMode)Enum.Parse(typeof(EGameMode), $"Mode{gameModeDropDown.options[gameModeDropDown.value].text}");

    void Start()
    {
        // ����� �κ� ����
        joinPrivateButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.JoinLobby(lobbyCodeInputField.text, -1);
        });

        // �κ� ��� ���ΰ�ħ
        refreshButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.RefreshPublicLobbies(GameMode);
        });

        // ��� Ȯ��
        joinFailConfirmButton.onClick.AddListener(() =>
        {
            joinFailedUI.SetActive(false);
            LobbyManager.Instance.RefreshPublicLobbies(GameMode);
        });
    }

    void OnEnable()
    {
        LobbyManager instance = LobbyManager.Instance;
        instance.RefreshPublicLobbies(GameMode);

        // ���� ���� ��Ȱ��ȭ
        for (int i = 0; i < _lobbySlots.Count; i++)
        {
            _lobbySlots[i].gameObject.SetActive(false);
        }

        // �ڷΰ���
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            instance.startLobbyUI.gameObject.SetActive(true);
        });
    }

    public void RefreshLobbiesUI()
    {
        LobbyManager instance = LobbyManager.Instance;
        for (int i = 0; i < instance.PublicLobbyDatas.Count || i < _lobbySlots.Count; i++)
        {
            if (i >= _lobbySlots.Count)
            {
                // ������ �����ϸ� ���� ����
                LobbySlotUI lobbySlot = Instantiate(lobbySlotPrefab, lobbyScrollContent);
                _lobbySlots.Add(lobbySlot);
                ShowLobbySlot(lobbySlot, i);
            }
            else if (i < instance.PublicLobbyDatas.Count)
            {
                // �ƴϸ� ���� ���� Ȱ��ȭ
                ShowLobbySlot(_lobbySlots[i], i);
            }

            // ���� ������ ��Ȱ��ȭ
            if (i >= instance.PublicLobbyDatas.Count)
            {
                _lobbySlots[i].gameObject.SetActive(false);
            }
        }

        float slotHeight = lobbySlotPrefab.rectTransform.sizeDelta.y;
        lobbyScrollContent.sizeDelta = new Vector2(lobbyScrollContent.sizeDelta.x, instance.PublicLobbyDatas.Count * slotHeight);
    }

    public void ShowJoinFailedUI()
    {
        // �κ� ���� ����
        LobbyManager instance = LobbyManager.Instance;
        instance.sortLobbyUI.joinFailedUI.SetActive(true);
        instance.loadingUI.SetActive(false);
    }

    void ShowLobbySlot(LobbySlotUI lobbySlot, int i)
    {
        LobbyManager instance = LobbyManager.Instance;
        string gameMode = instance.PublicLobbyDatas[i].gameMode.ToString();
        int startChar = 4;
        lobbySlot.gameObject.SetActive(true);
        lobbySlot.ShowLobbyUI(instance.PublicLobbyDatas[i].name, gameMode[startChar..]);
    }
}
