using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    private enum eMapType
    {
        None = -1,
        Bridge,
        //Jungle,
        //Desert,
        //Ice,
        Max
    }

    [SerializeField] private List<GameObject> _mapPrefabList = new List<GameObject>();
    [SerializeField] private List<Texture2D> _mapGroundTextureList = new List<Texture2D>();
    [SerializeField] private Ground _mapGroundPrefab = null;

    #region Random Map Generation
    [Header("Map Size")]
    [SerializeField] private float _mapSizeWidthRatio = 2.5f;         // �⺻������ ī�޶� ���� ũ�⿡ ���缭 ����

    [Header("Ground Scale")]
    [SerializeField] private float _randGroundScaleMin = 0.5f;        // �ּ� �� ũ��
    [SerializeField] private float _randGroundScaleMax = 1.5f;       // �ִ� �� ũ��

    [Header("Ground Interval")]
    [SerializeField] private float _randGroundIntervalMin = 1f;       // �ּ� �� ����
    [SerializeField] private float _randGroundIntervalMax = 2f;       // �ִ� �� ����

    private List<Rect> _existingGrounds = new List<Rect>();           // ������ ������ ���� ���� (�˻��)
    private List<PolygonCollider2D> _colliderGroundList = new List<PolygonCollider2D>();       // �ݶ��̴� ����Ʈ ����
    #endregion

    private eMapType _mapType = eMapType.None;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Init();
    }

    public void Init()
    {
        _colliderGroundList.Clear();
        _existingGrounds.Clear();

        RandomMapGeneration();
        //RandomGroundGeneration();
    }

    public void RandomMapGeneration()
    {
        // ���� ����
        _mapType =(eMapType)Random.Range((int)eMapType.Bridge, (int)eMapType.Max);

        // ��� ����
        GameObject map = Instantiate(_mapPrefabList[(int)_mapType]);
        map.transform.parent = this.transform;
    }

    #region Random Ground Generation
    private void RandomGroundGeneration()
    {
        float height = Camera.main.orthographicSize;
        float width = height * Camera.main.aspect * _mapSizeWidthRatio;

        int orderInLayerInc = 0;
        int maxAttempts = 3; // �ִ� �õ� Ƚ��

        // ȭ���� �Ѿ�� ���� �������� �ʴ´�
        for (int i = (int)-width; i < (int)width;)
        {
            // �� ����
            Ground ground = Instantiate(_mapGroundPrefab);
            ground.transform.parent = this.transform;
            ground.Init(_mapGroundTextureList[(int)_mapType]);

            // ���̾� ���� ����
            SpriteRenderer sr = ground.GetComponent<SpriteRenderer>();
            sr.sortingOrder += orderInLayerInc;
            orderInLayerInc++;

            // ���� ũ�� ����
            float randScale = Random.Range(_randGroundScaleMin, _randGroundScaleMax);
            ground.transform.localScale = new Vector2(randScale, randScale);

            // ���� ��ġ ����
            int minHeight = -(int)height;
            int maxHeight = minHeight - (minHeight / 2);

            bool isValidPosition = false;
            Vector3 newPosition = Vector3.zero;
            Rect newGroundBounds = new Rect();

            // ���� ��Ĩ Ȯ��
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // û
                float groundPosX = i == -(int)width ? i : i + Random.Range(_randGroundIntervalMin, _randGroundIntervalMax) * randScale;
                float groundPosY = Random.Range(minHeight, maxHeight);

                newGroundBounds = new Rect(
                    groundPosX - (ground.transform.localScale.x * 0.5f),
                    groundPosY - (ground.transform.localScale.y * 0.5f),
                    ground.transform.localScale.x,
                    ground.transform.localScale.y
                );

                bool isOverlapping = false;

                foreach (Rect existing in _existingGrounds)
                {
                    // ���� ��ġ���� Ȯ��
                    if (existing.Overlaps(newGroundBounds))
                    {
                        // ���� ���� �� ū �� �ȿ� ������ ���ԵǴ��� Ȯ��
                        if (newGroundBounds.width <= existing.width && newGroundBounds.height <= existing.height &&
                            existing.Contains(new Vector2(newGroundBounds.xMin, newGroundBounds.yMin)) &&
                            existing.Contains(new Vector2(newGroundBounds.xMax, newGroundBounds.yMax)))
                        {
                            isOverlapping = true;
                            break; // ���ο� ���ԵǸ� �ٽ� ��ġ ã��
                        }
                    }
                }

                if (!isOverlapping)
                {
                    newPosition = new Vector3(groundPosX, groundPosY, 0);
                    _existingGrounds.Add(newGroundBounds);
                    isValidPosition = true;
                    break;
                }
            }

            if (isValidPosition)
            {
                ground.transform.position = newPosition;
                _colliderGroundList.Add(ground.GetComponent<PolygonCollider2D>());
            }
            else
            {              
                Destroy(ground); // ������ ��ġ�� ã�� ���� ��� ����
            }

            // ���� ��ġ ��� (���� ����)
            i += (int)(Random.Range(_randGroundIntervalMin, _randGroundIntervalMax) * randScale);
        }
    }

    public List<PolygonCollider2D> GetGroundCollider2DList()
    {
        return _colliderGroundList;
    }
    #endregion
}
