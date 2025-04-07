using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyUI : MonoBehaviour
{
    public Toggle privateToggle;
    public TMP_Dropdown gameModeDropDown;
    public TMP_InputField nameInputField;
    public Button createButton;
    public Button backButton;

    void Start()
    {
        LobbyManager instance = LobbyManager.Instance;

        // �κ� �̸� �Է�
        nameInputField.onValueChanged.AddListener((_) =>
        {
            // �ؽ�Ʈ�� ������ ��ư Ȱ��ȭ
            createButton.interactable = !string.IsNullOrEmpty(nameInputField.text);
        });

        // �κ� ����
        createButton.onClick.AddListener(() =>
        {
            EGameMode gameMode = (EGameMode)Enum.Parse(typeof(EGameMode), $"Mode{gameModeDropDown.options[gameModeDropDown.value].text}");
            instance.CreateLobby(gameMode, nameInputField.text, privateToggle.isOn);
        });
    }

    void OnEnable()
    {
        // �ڷΰ���
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            LobbyManager.Instance.startLobbyUI.gameObject.SetActive(true);
        });
    }
}
