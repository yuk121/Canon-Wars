using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFlowManager : MonoBehaviour
{
    public static SceneFlowManager _instance;

    [Header("·Îµù UI ÇÁ¸®ÆÕ")]
    [SerializeField] GameObject loadingUIPrefab;

    private GameObject pooledLoadingUI;
    private LoadingUIController uiController;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            InitializeLoadingUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeLoadingUI()
    {
        if (loadingUIPrefab != null)
        {
            pooledLoadingUI = Instantiate(loadingUIPrefab);
            uiController = pooledLoadingUI.GetComponent<LoadingUIController>();
            pooledLoadingUI.transform.SetParent(null);
            pooledLoadingUI.SetActive(false);
            DontDestroyOnLoad(pooledLoadingUI);
        }
    }

    public void LoadScene(string sceneName)
    {
        bool showLoading = sceneName.Contains("3.IngameScene");
        StartCoroutine(LoadSceneAsync(sceneName, showLoading));
    }

    IEnumerator LoadSceneAsync(string sceneName, bool showLoading)
    {
        if (showLoading && uiController != null)
            uiController.Show();

        yield return new WaitForSeconds(0.5f);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
            yield return null;

        if (showLoading && uiController != null)
            uiController.SimulateLoading();
    }
}
