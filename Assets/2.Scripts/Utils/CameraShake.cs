using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Coroutine _corCamShake;
    public void Shake(float duration, float magnitude)
    {
        if (_corCamShake != null)
            StopCoroutine(_corCamShake);

        _corCamShake = StartCoroutine(CorShake(duration,magnitude));
    }

    private IEnumerator CorShake(float duration, float magnitude)
    {
        Vector3 originPos = transform.localPosition;
        // ��鸲 ������ �ð�
        float endTime= Time.time + duration;

        while(Time.time <= endTime)
        {
            Vector2 offset = Random.insideUnitCircle * magnitude;
            Vector3 shakePos = originPos + new Vector3(offset.x, offset.y, originPos.z);

            transform.localPosition = shakePos;

            yield return null;
        }

        transform.localPosition = originPos;
    }
}
