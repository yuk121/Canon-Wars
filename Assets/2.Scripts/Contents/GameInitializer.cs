using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private MapGenerator _mapGenerator;
    [SerializeField] private List<GameObject> _tankPrefabList;
    [SerializeField] private float _playerCount = 0;

    private List<PolygonCollider2D> _polygonCollider2DList;
    private List<Vector2> _groundTopPos = new List<Vector2>();

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
    private List<Vector2> FindGroundTopPos()
    {
        List<Vector2> topPosList = new List<Vector2>();

        foreach (var colider in _polygonCollider2DList)
        {
            if (colider == null)
                continue;

            float maxY = float.MinValue;

            // ��� ��θ� ��ȸ�ϸ� �ֻ�� ��ǥ�� Ž��
            for (int pathIndex = 0; pathIndex < colider.pathCount; pathIndex++)
            {
                Vector2[] points = colider.GetPath(pathIndex);

                foreach (Vector2 point in points)
                {
                    Vector2 worldPoint = transform.TransformPoint(point);

                    // �ֻ�� y�� ����
                    if (worldPoint.y > maxY)
                    {
                        maxY = worldPoint.y;
                        topPosList.Clear(); // ���ο� �ִ밪�� �����ϸ� �ʱ�ȭ
                        topPosList.Add(worldPoint);
                    }
                    else if (Mathf.Approximately(worldPoint.y, maxY))
                    {
                        topPosList.Add(worldPoint); // ������ �ִ밪�� ��� �߰�
                    }
                }
            }
        }

        return topPosList;
    }

    // x��ǥ �ߺ��Ǵ°� ����
    List<Vector2> FilterMultiplyPositionX(List<Vector2> positions)
    {
        // x��ǥ�� key�� Vector2 (x,y) ��ǥ�� Value
        Dictionary<float, Vector2> uniquePositions = new Dictionary<float, Vector2>();

        foreach (Vector2 pos in positions)
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
        int randPosIndex = UnityEngine.Random.Range(0, _groundTopPos.Count);

        // TODO : Player�� ������ ��ũ�� �´� Ű���� �̿��ؼ� �˸´� ��ũ �����ϱ�
        //          : ����� ù��° ��ũ�� ��ȯ

        GameObject go = Instantiate(_tankPrefabList[0], _groundTopPos[randPosIndex], Quaternion.identity);

        // �߾ӿ��� �����ʿ� �ִٸ� ������ �ٶ󺸵��� ������
        if(_groundTopPos[randPosIndex].x > 0)
        {
            go.transform.localScale = new Vector3(-1,1,1);
        }

        // �ߺ� ��ǥ�� ���� �ȵǵ��� ����
        _groundTopPos.RemoveAt(randPosIndex);
    }
    #endregion
}
