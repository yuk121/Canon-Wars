using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class StartSceneManager : MonoBehaviour
{
    [SerializeField] InputField _idField;
    [SerializeField] InputField _passwordField;
    [SerializeField] Button _loginBtn;
    [SerializeField] Button _signUpBtn;
    [SerializeField] Button _exitBtn;
    [SerializeField] GameObject _singupUI;


    void Start()
    {
        _loginBtn.onClick.AddListener(OnLoginClick);
        _signUpBtn.onClick.AddListener(OnSignUpClick);
        _exitBtn.onClick.AddListener(OnExitClick);
    }



    void OnLoginClick()
    {
        string email = _idField.text;
        string password = _passwordField.text;

        if (string.IsNullOrEmpty(email))
        {
            StartCoroutine(ShowWarningBorder(_idField));
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            StartCoroutine(ShowWarningBorder(_passwordField));
            return;
        }

        //파이어베이스랑 로그인 여부 인증 진행필요

    }

    void OnSignUpClick()
    {
        Debug.Log("회원가입버튼 클릭");
        _singupUI.SetActive(true);
    }

    void OnExitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    IEnumerator ShowWarningBorder(InputField field)                     //아이디 패스워드 입력없이 로그인버튼클릭시 해당필드를 빨간색으로 표시함
    {
        var originalColor = field.GetComponent<Image>().color;
        field.GetComponent<Image>().color = Color.red;
        yield return new WaitForSeconds(0.5f);
        field.GetComponent<Image>().color = originalColor;
    }
}
