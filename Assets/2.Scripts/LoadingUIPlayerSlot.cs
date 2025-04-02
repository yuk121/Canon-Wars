using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingUIPlayerSlot : MonoBehaviour
{
    [SerializeField] Text _playerNameText;
    [SerializeField] Image _tankImage;
    [SerializeField] Text _winLossText;
    [SerializeField] Text _loadingStatusText;

    Coroutine _dotCoroutine;

    // �ʱ� �¾�
    public void Setup(string nickname, Sprite tankSprite, int wins, int losses)
    {
        _playerNameText.text = nickname;
        _tankImage.sprite = tankSprite;
        _winLossText.text = $"{wins}��  {losses}��";
        _loadingStatusText.text = "�ε� ��";
        StartLoadingTextAnimation();
    }

    // �ε��� �ִϸ��̼� ����
    public void StartLoadingTextAnimation()
    {
        if (_dotCoroutine != null)
            StopCoroutine(_dotCoroutine);

        _dotCoroutine = StartCoroutine(AnimateDots());
    }

    IEnumerator AnimateDots()
    {
        string baseText = "�ε� ��";
        int dotCount = 0;

        while (true)
        {
            _loadingStatusText.text = baseText + new string('.', dotCount % 4);
            dotCount++;
            yield return new WaitForSeconds(0.5f);
        }
    }

    // �ε� �Ϸ� �� ȣ��
    public void SetLoaded()
    {
        if (_dotCoroutine != null)
            StopCoroutine(_dotCoroutine);

        _loadingStatusText.text = "�ε� �Ϸ�!!";
    }

}
