using UnityEngine;

[CreateAssetMenu(fileName = "TankData", menuName = "Tank/Tank Data", order = 1)]
public class TankDataSO : ScriptableObject
{
    [Header("기본 탱크 정보")]
    public string _tankName;         // 탱크 이름
    public int _hp;              // 체력
    public int _atk;              // 공격력
    public float _speed;             // 이동 속도

    [Header("탱크 이미지")]
    public Sprite _tankSprite;       // 탱크 본체 이미지

    [Header("미사일 관련 정보")]
    public Sprite _missileSprite;    // 미사일 이미지
    public float _missileSpeed;      // 미사일 속도
    public GameObject _missileEffect; // 미사일 터질 때 효과 프리팹
}
