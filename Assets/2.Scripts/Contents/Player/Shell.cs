using UnityEngine;

public enum eShellExplosionType
{
    Circle,
    Ellipse,
}
public class Shell : MonoBehaviour
{
    [SerializeField] private eShellExplosionType _explosinType;
    public eShellExplosionType ShellExplosionType { get => _explosinType; }
    [SerializeField] private float _durtaion = 3.5f;        // 객체의 지속시간
    [SerializeField] protected float _radius = 2.5f;

    private Rigidbody2D _rb2D = null;
    protected BoxCollider2D _collider2D = null;

    private float _endTime = 0f;
    private float _power = 1f;
    private bool _isFire = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        _collider2D = GetComponent<BoxCollider2D>();
    }

    public void FixedUpdate()
    {
        if (_isFire)
        {
            _isFire = false;
            Vector2 fireDir = transform.right * Mathf.Sign(transform.localScale.x);
            _rb2D.AddForce(fireDir * _power, ForceMode2D.Impulse);
        }

        float angle = Mathf.Atan2(_rb2D.linearVelocity.y, _rb2D.linearVelocity.x) * Mathf.Rad2Deg;
        
        // 탱크가 좌측을 바라볼 때 180도 추가
        if (transform.localScale.x < 0)
        {
            angle += 180f;
        }

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, angle);
    }

    public void Update()
    {
          CheckExplosion();

        // 지속시간이 끝나면 없애기
        if (Time.time >= _endTime)
        {
            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.Push(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    public void Init()
    {
        _rb2D = GetComponent<Rigidbody2D>();
        _isFire = false;
        _endTime = Time.time + _durtaion;
    }

    public void Fire(float power)
    {
        _power = power;
        _isFire = true;
    }

    public virtual void CheckExplosion()
    {
        Debug.DrawRay(transform.position, Vector2.down * 0.2f, Color.magenta);

        // BoxCollider2D의 중심과 크기를 가져오기
        Vector2 colliderCenter = (Vector2)transform.position + _collider2D.offset;
        Vector2 colliderSize = _collider2D.size+(Vector2.one * 0.05f);

        // BoxCast를 사용하여 플레이어 또는 땅의 충돌 감지
        RaycastHit2D hit = Physics2D.BoxCast(colliderCenter, colliderSize, transform.eulerAngles.z, Vector2.down, 0f, LayerMask.GetMask("Player", "Ground"));

        // 충돌 확인 시
        if (hit.collider != null)
        {
            // CircleCast를 사용하여 폭발 범위에 있는 객체 List 가져오기
            Collider2D[] hitPlayerList = Physics2D.OverlapCircleAll(colliderCenter, _radius, LayerMask.GetMask("Player"));
            Collider2D[] hitGroundList = Physics2D.OverlapCircleAll(colliderCenter, _radius, LayerMask.GetMask("Ground"));
            if (hitPlayerList.Length > 0)
            {
                // 데미지 주기
            }

            if (hitGroundList.Length > 0)
            {
                foreach (var hitGround in hitGroundList)
                {
                    Ground ground = hitGround.GetComponent<Ground>();
                    ground.GroundExplosion(colliderCenter, _radius);
                }
            }

            // 충돌한 경우에만 Pool
            PoolManager.Instance.Push(gameObject);
        }
    }
}
