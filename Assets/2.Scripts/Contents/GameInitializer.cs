using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    private const float SPAWN_OFFEST_X = 1.5f;
    private const float SPAWN_OFFSET_Y = 1.5f;
    private const int SPAWN_POS_REMOVE_RANGE = 2;      // ���� ��ġ ���� ���� ���� (�÷��̾ �ʹ� �������� ���� ����)
    [SerializeField] private MapGenerator _mapGenerator;
    [SerializeField] private List<GameObject> _tankPrefabList;
    [SerializeField] private float _playerCount = 0;

    private List<PolygonCollider2D> _polygonCollider2DList;
    private List<Vector2Int> _groundTopPos = new List<Vector2Int>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();
    }

    public void Init(Action pCallback = null)
    {
        _groundTopPos.Clear();

        // �� ����
        _mapGenerator.Init();

        _polygonCollider2DList = _mapGenerator.GetGroundCollider2DList();

        // �ֻ�� �κ� ã��
        _groundTopPos = FindGroundTopPos();

        // �ߺ� x��ǥ �� y���� �� ū ���� �츮��
        _groundTopPos = FilterMultiplyPositionX(_groundTopPos);

        // ĳ���� ���� ��ǥ�� ����
        for(int i =0; i < _playerCount; i++)
        {
            PlayerRandomSpawn();
        }

        if (pCallback != null)
            pCallback.Invoke();
    }
    #region Ground Top Pos Find Method
    private List<Vector2Int> FindGroundTopPos()
    {
        List<Vector2Int> topPosListAll = new List<Vector2Int>(); // ��� �׶��� top pos ����Ʈ

        foreach (var colider in _polygonCollider2DList)
        {
            if (colider == null)
                continue;

            int maxY = int.MinValue;
            HashSet<Vector2Int> coliderTopPosSet = new HashSet<Vector2Int>(); // �ߺ� ���Ÿ� ���� HashSet

            // ��� ��� ��ȸ�ϸ� �ֻ�� ��ǥ Ž��
            for (int pathIndex = 0; pathIndex < colider.pathCount; pathIndex++)
            {
                Vector2[] points = colider.GetPath(pathIndex);

                foreach (Vector2 point in points)
                {
                    Vector2 worldPoint = colider.transform.TransformPoint(point);

                    // x ��ǥ: SPAWN_OFFSET_X ���� �� ��ȯ
                    int posIntX = Mathf.RoundToInt(worldPoint.x - Mathf.Sign(worldPoint.x) * SPAWN_OFFEST_X);
                    // y ��ǥ: SPAWN_OFFSET_Y ���� �� ��ȯ
                    int posIntY = Mathf.CeilToInt(worldPoint.y + SPAWN_OFFSET_Y);

                    Vector2Int worldPointInt = new Vector2Int(posIntX, posIntY);

                    // �ֻ�� y�� ����
                    if (posIntY > maxY)
                    {
                        maxY = posIntY;
                        coliderTopPosSet.Clear(); // ���ο� �ֻ�� �߰� �� �ʱ�ȭ
                        coliderTopPosSet.Add(worldPointInt);
                    }
                    else if (posIntY == maxY)
                    {
                        coliderTopPosSet.Add(worldPointInt); // ���� ������ ��ǥ �߰�
                    }
                }
            }

            // �ֻ�� ��ǥ ����Ʈ ���� (x �� �������� ����)
            List<Vector2Int> coliderTopPosList = coliderTopPosSet.ToList();

            // �ִ� ~ �ּұ��� 1 ���� �������� ��ǥ ����
            List<Vector2Int> topLinePos = GenerateTopLine(coliderTopPosList);

            // ��� ����Ʈ�� �߰�
            topPosListAll.AddRange(topLinePos);
        }

        return topPosListAll;
    }

    // �ֻ�� ��ǥ�� ������� �������� x ��ǥ ����Ʈ ����
    private List<Vector2Int> GenerateTopLine(List<Vector2Int> topPositions)
    {
        List<Vector2Int> topNewPos = new List<Vector2Int>();

        if (topPositions.Count == 0) return topNewPos;

        int startX = topPositions[0].x;
        int endX = topPositions[topPositions.Count - 1].x;
        int y = topPositions[0].y; // ��� y ���� �����Ƿ� �ϳ��� ������

        for (int x = startX; x >= endX; x--)
        {
            topNewPos.Add(new Vector2Int(x, y));
        }

        return topNewPos;
    }

    // x��ǥ �ߺ��Ǵ°� ����
    List<Vector2Int> FilterMultiplyPositionX(List<Vector2Int> positions)
    {
        // x��ǥ�� key�� Vector2Int (x,y) ��ǥ�� Value
        Dictionary<float, Vector2Int> uniquePositions = new Dictionary<float, Vector2Int>();

        foreach (Vector2Int pos in positions)
        {
            // x��ǥ�� �ߺ����� Ȯ��
            if (uniquePositions.ContainsKey(pos.x))
            {
                // ������ x��ǥ�� ��� y���� �� ū ��ǥ�� ����
                if (pos.y > uniquePositions[pos.x].y)
                {
                    uniquePositions[pos.x] = pos;
                }
            }
            else
            {
                // ���ο� x��ǥ �߰�
                uniquePositions[pos.x] = pos;
            }
        }

        return uniquePositions.Values.ToList(); // ����� ����Ʈ�� ��ȯ
    }
    #endregion

    #region Player Random Pos Spawn Method
    private void PlayerRandomSpawn()
    {
        // ���� ��ǥ ����
        int randPosIndex = UnityEngine.Random.Range(0, _groundTopPos.Count);
        float posX = _groundTopPos[randPosIndex].x;
        float posY = _groundTopPos[randPosIndex].y;
        Vector3 randSpawnPos = new Vector3(posX, posY);

        // TODO : Player�� ������ ��ũ�� �´� Ű���� �̿��ؼ� �˸´� ��ũ �����ϱ�
        //          : ����� ù��° ��ũ�� ��ȯ
        GameObject go = Instantiate(_tankPrefabList[0], randSpawnPos, Quaternion.identity);

        // �߾ӿ��� �����ʿ� �ִٸ� ������ �ٶ󺸵��� ������
        if(_groundTopPos[randPosIndex].x > 0)
        {
            go.transform.localScale = new Vector3(-1,1,1);
        }

        // �ߺ� ��ǥ�� ���� �ȵǵ��� ����
        int minRemoveIndex = Mathf.Max(0,randPosIndex - SPAWN_POS_REMOVE_RANGE);
        int maxRemoveIndex = Mathf.Min(randPosIndex + SPAWN_POS_REMOVE_RANGE, _groundTopPos.Count-1);
        
        for (int i = maxRemoveIndex; i >= minRemoveIndex; i--)
        {
            _groundTopPos.RemoveAt(i);
        }
    }
    #endregion
}
