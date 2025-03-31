using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private const float MAX_POWER = 20f;
    private const float MIN_POWER = 0.1f;

    [Header("Body")]
    [SerializeField] private float _moveAngleThreshold = 70f;
    [SerializeField] private float _speed = 4f;

    [Header("Artillery")]
    [SerializeField] private Transform _artilleryTrans;
    [SerializeField] private Transform _bulletPos;
    [SerializeField] private float _artilleryRotateSpeed = 10f;
    [SerializeField] private float _minAngleArtillery = 0f;
    [SerializeField] private float _maxAngleArtillery = 85f;

    [Header("Shell")]
    [SerializeField] private List<GameObject> _shellList = new List<GameObject>();
    [SerializeField] private float _maxShellPower = 20f;
    [SerializeField] private float _minShellPower = 0.1f;
    [SerializeField] private float _curShellPower = 1f;
    [SerializeField] private float _powerGaugeSpeed = 1f;

    private Rigidbody2D _rb2D = null;               // Tank RigidBody 2D
    private CircleCollider2D _colider2D = null;     // Tank CircleCollider 2D
    private GameObject _curShell = null;            // 현재 선택된 포탄    

    private float _dirY = 0;
    private float _dirX = 0;
    private bool _isMoveAngle = false;
    private float _curGauge = 0f;
    private float _prevShellPower = 0f;           // 이전에 쏜 파워 값

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();
    }

    public void Init()
    {
        _rb2D = GetComponent<Rigidbody2D>();
        _colider2D = GetComponent<CircleCollider2D>();

        // Default
        _curShell = _shellList[0];
    }
    // Update is called once per frame

    private void Update()
    {
        // 포탄 변경
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (_shellList[0] == null)
            {
                Debug.LogWarning("Shell 1 is Null");
            }
            else
            {
                // 1번 폭탄
                _curShell = _shellList[0];
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (_shellList[1] == null)
            {
                Debug.LogWarning("Shell 2 is Null");
            }
            else
            {
                // 2번 폭탄
                _curShell = _shellList[1];
            }
        }

        // 파워게이지 증가 시키기
        if (Input.GetKey(KeyCode.Space))
        {
            _curGauge += Time.deltaTime * _powerGaugeSpeed;
            _curShellPower = Mathf.Lerp(_minShellPower, _maxShellPower, _curGauge);
        }

        // 포탄 발사
        if (Input.GetKeyUp(KeyCode.Space))
        {
            _curGauge = 0f;
            Shell shell = GenerationShell();

            if (shell == null)
            {
                Debug.LogWarning("Generation Shell is Null !!!");
            }

            // 발사
            shell.Fire(_curShellPower);

            // 발사한 포탄 저장
            _curShell = shell.gameObject;
            // 이전 포탄 파워 값 저장
            _prevShellPower = _curShellPower;
        }

        _dirY = Input.GetAxis("Vertical");
        // 새로운 회전값 계산 (현재 회전 + 입력값)

        float currentZ = _artilleryTrans.localEulerAngles.z;

        if (currentZ > 180f)
            currentZ -= 360f; // 180° 이상이면 -180° ~ 0°로 변환

        float newZ = Mathf.Clamp(currentZ + _dirY * Time.deltaTime * _artilleryRotateSpeed, _minAngleArtillery, _maxAngleArtillery);
        Quaternion quaternion = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.eulerAngles.y, newZ);
        _artilleryTrans.localRotation = Quaternion.Lerp(_artilleryTrans.localRotation, quaternion, Time.deltaTime * 10);

        // 땅 위에 있는지 확인
        if (IsGround())
        {
            _dirX = Input.GetAxis("Horizontal");

            // 포 각도 조절중에는 움직일 수 없음
            if (_dirY == 0)
               transform.Translate(_dirX * _speed * Time.deltaTime, 0, 0, Space.World);

            if (_dirX != 0f)
            {
                Vector3 newScale = transform.localScale;
                newScale.x = Mathf.Abs(newScale.x) * Mathf.Sign(_dirX); // 좌우 반전
                transform.localScale = newScale;
            }
        }
        else
        {
            // 공중일땐 수평 
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    private Shell GenerationShell()
    {
        GameObject go = PoolManager.Instance.Pop(_curShell.gameObject);

        if (go == null)
        {
            go = Instantiate(_curShell, _bulletPos.position, Quaternion.identity);
            PoolManager.Instance.Push(go);
        }
            
        go.transform.position = _bulletPos.position;
        go.transform.rotation = Quaternion.Euler(0, 0, _artilleryTrans.eulerAngles.z);
        
        // 포탄 방향 

        Vector3 newScale = go.transform.localScale;
        newScale.x = Mathf.Abs(newScale.x) * Mathf.Sign(transform.localScale.x); // 좌우 반전
        go.transform.localScale = newScale;

        Shell shell = go.GetComponent<Shell>();

        switch(shell.ShellExplosionType)
        {
            case eShellExplosionType.Circle:
                shell.Init();
                return shell;
            
            case eShellExplosionType.Ellipse:
                ShellEllipse shellEllipse = go.GetComponent<ShellEllipse>();
                shellEllipse.Init();
                return shellEllipse;
        }
        return null;
    }

    private bool IsGround()
    {
        // CircleCollider2D 중심 가져오기
        Vector2 center = (Vector2)_colider2D.bounds.center;

        // 항상 콜라이더의 기울기에 따라 ray 시작 위치를 계산
        Vector2 ray1 = center + (Vector2)(-transform.right * _colider2D.radius); // 왼쪽 끝
        Vector2 ray2 = center + (Vector2)(transform.right * _colider2D.radius);  // 오른쪽 끝

        float distance = 0.9f;// 0.75f;

        // 시각화 
        Debug.DrawRay(ray1, -transform.up * distance, Color.red);
        Debug.DrawRay(ray2, -transform.up * distance, Color.blue);

        // 방향
        Vector2 direction = (Vector2)transform.up * -1f;

        // Raycast
        RaycastHit2D leftHit = Physics2D.Raycast(ray1, direction, distance, LayerMask.GetMask("Ground"));
        RaycastHit2D rightHit = Physics2D.Raycast(ray2, direction, distance, LayerMask.GetMask("Ground"));

        //Linecast 
        //RaycastHit2D area = Physics2D.Linecast(ray1 + direction * distance, ray2 + direction * distance, LayerMask.GetMask("Ground"));
        RaycastHit2D area = Physics2D.BoxCast(transform.position, new Vector2(_colider2D.radius * 2, _colider2D.radius * 2), 0f,direction, distance,LayerMask.GetMask("Ground"));

        Debug.DrawLine(ray1 + direction * distance, ray2 + direction * distance, Color.magenta);

        return leftHit.collider != null || rightHit.collider || area.collider != null; //hits.Length > 0f;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ground")) 
            return; 

        Vector2 averageNormal = Vector2.zero;
        int contactCount = collision.contactCount;

        // 모든 충돌 지점의 법선 벡터 합산
        for (int i = 0; i < contactCount; i++)
        {
            averageNormal += collision.contacts[i].normal;
        }

        if (contactCount > 0)
        {
            averageNormal /= contactCount; // 평균값 계산
            averageNormal.Normalize(); // 단위 벡터화

            // 목표 회전 각도 계산
            float targetAngle = Mathf.Atan2(averageNormal.y, averageNormal.x) * Mathf.Rad2Deg - 90f;

            _isMoveAngle = CanMoveAngle(targetAngle);
 
            if (_isMoveAngle)
            {          
                transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, targetAngle);
            }
        }
    }

    // 움직일 수 있는 각도인지 확인하는 함수
    private bool CanMoveAngle(float targetAngle)
    {
        // 목표 각도도 같은 방식으로 변환
        if (targetAngle > 180f)
        {
            targetAngle -= 360f;
        }

        // 허용된 기울기 내에 있는지 확인
        return Mathf.Abs(targetAngle) <= _moveAngleThreshold;
    }

    void OnDrawGizmos()
    {
        //if(_colider2D != null)
        //{
        //    Vector2 pos = new Vector2(transform.position.x + _colider2D.offset.x, transform.position.y + _colider2D.offset.y);
        //    Gizmos.DrawCube(pos, new Vector2(_colider2D.radius * 2, _colider2D.radius *2));
        //}
    }
}
