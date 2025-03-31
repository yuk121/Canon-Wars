using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;


public class SceneFlowManager : MonoBehaviour
{
    public static SceneFlowManager _Instance;

    [SerializeField] GameObject loadingUI;

    void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        loadingUI.SetActive(true);

        yield return new WaitForSeconds(0.5f); // 살짝 기다리기

        var op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
        {
            yield return null;
        }

        loadingUI.SetActive(false);
    }
}
