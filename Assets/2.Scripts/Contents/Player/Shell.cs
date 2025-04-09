using UnityEngine;

public enum eShellExplosionType
{
    Circle,
    Ellipse,
}
public class Shell : MonoBehaviour
{
    [Header("Camera Shake")]
    [SerializeField] private float _shakeDurtaion = 0.5f;
    [SerializeField] private float _shakeMagnitude = 1f;

    [Header("Shell Info")]
    [SerializeField] private eShellExplosionType _explosinType;
    public eShellExplosionType ShellExplosionType { get => _explosinType; }

    [SerializeField] private GameObject _explosionParticlePrefab = null;
    [SerializeField] private float _durtaion = 3.5f;        // ��ü�� ���ӽð�
    [SerializeField] protected float _radius = 2.5f;
    
    //[SerializeField] private Sprite _debugConflictPoint;

    private Rigidbody2D _rb2D = null;
    protected BoxCollider2D _collider2D = null;

    private float _endTime = 0f;
    private float _power = 1f;
    private bool _isFire = false;

    public void Init()
    {
        _collider2D = GetComponent<BoxCollider2D>();
        _rb2D = GetComponent<Rigidbody2D>();
        _isFire = false;
        _endTime = Time.time + _durtaion;
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
        
        // ��ũ�� ������ �ٶ� �� 180�� �߰�
        if (transform.localScale.x < 0)
        {
            angle += 180f;
        }

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, angle);
    }

    public void Update()
    {
          CheckExplosion();

        // ���ӽð��� ������ ���ֱ�
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
    protected void LateUpdate()
    {
        // ��ź�� �� ������ �������� Ȯ��
        CheckShellMapOut();
    }

    private void CheckShellMapOut()
    {
        if (GameInitializer.Instance == null || GameInitializer.Instance.GetMapSize() == Vector2.zero)
            return;

        Vector2 mapSize = GameInitializer.Instance.GetMapSize();

        // ��ź�� �� ��, �ϴ��� �Ѿ���� Ȯ���ϱ� (���� ����)
        if(transform.position.x < -mapSize.x / 2f || transform.position.x > mapSize.x / 2f || transform.position.y < -mapSize.y / 2f)
        {
            ReleaseShell();
        }    
    }

    public void Fire(float power)
    {
        _power = power;
        _isFire = true;
    }

    public virtual void CheckExplosion()
    {
        Debug.DrawRay(transform.position, Vector2.down * 0.2f, Color.magenta);

        // BoxCollider2D�� �߽ɰ� ũ�⸦ ��������
        Vector2 colliderCenter = (Vector2)transform.position + _collider2D.offset;
        Vector2 colliderSize = _collider2D.size;

        // BoxCast�� ����Ͽ� �÷��̾� �Ǵ� ���� �浹 ����
        RaycastHit2D hit = Physics2D.BoxCast(colliderCenter, colliderSize, transform.eulerAngles.z, Vector2.down, 0f, LayerMask.GetMask("Player", "Ground"));

        // �浹 Ȯ�� ��
        if (hit.collider != null)
        {
            // ��ƼŬ ����
            CreateExplosionParticle();

            // ī�޶� ��鸲
            CamShake();

            // CircleCast�� ����Ͽ� ���� ������ �ִ� ��ü List ��������
            Collider2D[] hitPlayerList = Physics2D.OverlapCircleAll(colliderCenter, _radius, LayerMask.GetMask("Player"));
            Collider2D[] hitGroundList = Physics2D.OverlapCircleAll(colliderCenter, _radius, LayerMask.GetMask("Ground"));
            if (hitPlayerList.Length > 0)
            {
                // ������ �ֱ�
            }

            if (hitGroundList.Length > 0)
            {
                foreach (var hitGround in hitGroundList)
                {
                    Ground ground = hitGround.GetComponent<Ground>();
                    ground.GroundExplosion(colliderCenter, _radius);

                    // ����׿� �� ��������Ʈ ����
                    //DebugExplosionCircle(colliderCenter, _radius);
                }
            }

            ReleaseShell();
        }
    }

    protected void ReleaseShell()
    {
        // ī�޶� ���̻� ��ź�� �ȵ��󰡵���
        GameInitializer.Instance.CurShellTrans = null;

        // �浹�� ��쿡�� Pool
        PoolManager.Instance.Push(gameObject);
    }

    protected void CreateExplosionParticle()
    {
        // ���� ��ƼŬ ���
        if (_explosionParticlePrefab != null)
        {
            GameObject go = PoolManager.Instance.Pop(_explosionParticlePrefab);

            if (go == null)
            {
                go = Instantiate(_explosionParticlePrefab);
                PoolManager.Instance.Push(go);
            }
            go.transform.position = transform.position;

            ParticleSystem particle = go.GetComponent<ParticleSystem>();
            particle.Play();
        }
    }

    protected void CamShake()
    {
        CameraShake camShake = Camera.main.GetComponent<CameraShake>();
       
        if (camShake == null)
            return;

        camShake.Shake(_shakeDurtaion, _shakeMagnitude);
    }
    //private void DebugExplosionCircle(Vector2 position, float radius)
    //{
    //    GameObject debugCircle = new GameObject("ExplosionDebugCircle");
    //    debugCircle.transform.position = position;
    //    debugCircle.transform.localScale = Vector3.one * 0.3f;

    //    SpriteRenderer sr = debugCircle.AddComponent<SpriteRenderer>();
    //    sr.sprite = _debugConflictPoint; // "DebugCircle"�� �̸� ����� �� ���� ��������Ʈ
    //    sr.color = new Color(1, 0, 0, 1); // ������
    //    sr.sortingOrder = 3;
    //}
}
