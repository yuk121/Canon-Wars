using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingUIController : MonoBehaviour
{
    [Header("로딩 UI 요소")]
    [SerializeField] private Text loadingText;
    [SerializeField] private Image spinner;

    private Coroutine animateCoroutine;
    private bool isLoaded = false;

    public void Show()
    {
        gameObject.SetActive(true);
        Initialize();
    }

    public void Hide()
    {
        if (animateCoroutine != null)
            StopCoroutine(animateCoroutine);

        gameObject.SetActive(false);
    }

    public void Initialize()
    {
        isLoaded = false;

        if (loadingText != null)
            loadingText.text = "로딩 중";

        if (spinner != null)
            spinner.transform.rotation = Quaternion.identity;

        if (animateCoroutine != null)
            StopCoroutine(animateCoroutine);

        animateCoroutine = StartCoroutine(AnimateLoadingText());
    }

    public void SimulateLoading()
    {
        StartCoroutine(FinishLoadingAfterDelay());
    }

    IEnumerator FinishLoadingAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);

        isLoaded = true;

        if (loadingText != null)
            loadingText.text = "로딩 완료!";

        if (spinner != null)
            spinner.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);
        Hide();
    }

    void Update()
    {
        if (!isLoaded && spinner != null)
        {
            spinner.transform.Rotate(Vector3.forward * -200f * Time.deltaTime);
        }
    }

    IEnumerator AnimateLoadingText()
    {
        string baseText = "로딩 중";
        int dotCount = 0;

        while (!isLoaded)
        {
            loadingText.text = baseText + new string('.', dotCount % 4);
            dotCount++;
            yield return new WaitForSeconds(0.5f);
        }
    }
}