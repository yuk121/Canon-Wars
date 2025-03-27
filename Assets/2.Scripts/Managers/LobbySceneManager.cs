using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LobbySceneManager : MonoBehaviour
{
    [SerializeField] Button _startBtn;
    [SerializeField] GameObject _startPanel;
    [SerializeField] Button _infoBtn;
    [SerializeField] GameObject _infoPanel;
    [SerializeField] Button _optionBtn;
    [SerializeField] GameObject _optionPanel;
    [SerializeField] Button _exitBtn;


    void Start()
    {
        _startBtn.onClick.AddListener(OpenStartPanel);
        _infoBtn.onClick.AddListener(OpenInfoPanel);
        _optionBtn.onClick.AddListener(OpenOptionPanel);
        _exitBtn.onClick.AddListener(OnExitClick);     
    }

    void OpenStartPanel()
    {
        Debug.Log("시작하기 버튼 클릭");
        //_startPanel.SetActive(true);
    }

    void OpenInfoPanel()
    {
        _infoPanel.SetActive(true);
    }

    void OpenOptionPanel()
    {
        _optionPanel.SetActive(true);
    }

    void OnExitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}
