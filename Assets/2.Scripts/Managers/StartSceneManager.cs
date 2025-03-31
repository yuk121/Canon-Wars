using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class StartSceneManager : MonoBehaviour
{
    [Header("User Interection Property")]
    [SerializeField] InputField _idField;
    [SerializeField] InputField _passwordField;
    [SerializeField] InputField _nickNameField;

    [Space(5)]
    [SerializeField] Button _loginBtn;
    [SerializeField] Button _signUpBtn;
    [SerializeField] Button _exitBtn;
    [SerializeField] Button _searchPWBtn;
    [SerializeField] Button _createAccountBtn;

    [Space(10)]
    [Header("SYSTEM")]
    [SerializeField] FirebaseManager _fm;

    [Space(10)]
    [Header("UI")]
    [SerializeField] Image _maskImage;

    [Space(10)]
    [SerializeField] GameObject _infoWindowObject;
    [SerializeField] Text _infoText;

    [Space(10)]
    [SerializeField] GameObject _createAccountWindow;


    void Start()
    {
        //���� ����Ʈ ȭ���� Button Action �ο�
        {
            //ȸ������ Button.
            _signUpBtn.onClick.AddListener(
                delegate
                {
                    Check_Empty_BaseInputField(delegate
                    {
                        //password ã�⸦ �̿�
                        _fm.Get_UserPW(_idField.text,
                            delegate
                            {
                                //�̹� ������ ID�� �����Ѵٸ�.
                                if (!string.IsNullOrEmpty(_fm.userVO.UserID))
                                {
                                    _maskImage.raycastTarget = true;
                                    _infoWindowObject.SetActive(true);
                                    _infoText.text = "�̹� �����ϴ� ���̵��!! �ٸ� ���̵� ��������!";
                                }
                                //�������� �ʴ´ٸ� ���� ȭ�� open
                                else
                                {
                                    _maskImage.raycastTarget = true;
                                    _createAccountWindow.SetActive(true);
                                }
                            });
                    });
                });


            _loginBtn.onClick.AddListener(
                delegate
                {
                    Check_Empty_BaseInputField(delegate
                    {
                        OnLoginClick();
                    });
                });

            _searchPWBtn.onClick.AddListener(
                delegate
                {
                    Check_Empty_BaseInputField(delegate
                    {
                        //password ã�⸦ �̿�
                        _fm.Get_UserPW(_idField.text,
                            delegate
                            {
                                //�̹� ������ ID�� �����Ѵٸ�.
                                if (!string.IsNullOrEmpty(_fm.userVO.UserID))
                                {
                                    _maskImage.raycastTarget = true;
                                    _infoWindowObject.SetActive(true);
                                    _infoText.text = $"��й�ȣ : {_fm.userVO.UserPW}";
                                }
                                //�������� �ʴ´ٸ�.
                                else
                                {
                                    _maskImage.raycastTarget = true;
                                    _infoWindowObject.SetActive(true);
                                    _infoText.text = "���� ID��. �ٽ� Ȯ������!";
                                }
                            });
                    });
                });


            _exitBtn.onClick.AddListener(OnExitClick);
        }

        //���� �˾� ȭ�� Button Action �ο�.
        {
            _createAccountBtn.onClick.AddListener(
                delegate
                {
                    if (string.IsNullOrEmpty(_nickNameField.text))
                    {
                        _maskImage.raycastTarget = true;
                        _infoWindowObject.SetActive(true);
                        _infoText.text = "����� �г����� ��������!!";
                        return;
                    }

                    _fm.Create_UserAccount(_idField.text, _passwordField.text, _nickNameField.text);
                });
        }
    }


    /// <summary>
    /// ��ư �۵� ��, ID InputField�� PW InputField�� string Empty�� üũ.
    /// </summary>
    void Check_Empty_BaseInputField(Action action)
    {
        if (string.IsNullOrEmpty(_idField.text))
        {
            _maskImage.raycastTarget = true;
            _infoWindowObject.SetActive(true);
            _infoText.text = "���̵� ���� �Է�����!!";
            return;
        }

        if (string.IsNullOrEmpty(_passwordField.text))
        {
            _maskImage.raycastTarget = true;
            _infoWindowObject.SetActive(true);
            _infoText.text = "��й�ȣ�� ���� �Է�����!!";
            return;
        }

        action.Invoke();
    }

    void OnLoginClick()
    {
        string email = _idField.text;
        string password = _passwordField.text;

        _fm.Login(email, password,
        delegate
        {
            _maskImage.raycastTarget = true;
            _infoWindowObject.SetActive(true);
            _infoText.text = "��ġ���� �ʴ� ���̵� Ȥ�� ��й�ȣ��! Ȯ������!";
        },
        delegate
        {
            SceneFlowManager._Instance.LoadScene("2.LobbyScene"); 
            //�� ��ȯ �ҽ��� ���⿡ ������ �˴ϴ�.
            //�α��� �Ǿ VO Setting �� ���� ������ �Ѿ�ϴ�.
        });
    }

    void OnSignUpClick()
    {
        Debug.Log("ȸ�����Թ�ư Ŭ��");
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
