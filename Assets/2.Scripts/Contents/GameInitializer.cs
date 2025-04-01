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

        // 맵 생성
        _mapGenerator.Init();

        _polygonCollider2DList = _mapGenerator.GetGroundCollider2DList();

        // 최상단 부분 찾기
        _groundTopPos = FindGroundTopPos();

        // 중복 x좌표 중 y값이 더 큰 곳만 살리기
        _groundTopPos = FilterMultiplyPositionX(_groundTopPos);

        // 캐릭터 랜덤 좌표에 생성
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

            // 모든 경로를 순회하며 최상단 좌표를 탐색
            for (int pathIndex = 0; pathIndex < colider.pathCount; pathIndex++)
            {
                Vector2[] points = colider.GetPath(pathIndex);

                foreach (Vector2 point in points)
                {
                    Vector2 worldPoint = transform.TransformPoint(point);

                    // 최상단 y값 갱신
                    if (worldPoint.y > maxY)
                    {
                        maxY = worldPoint.y;
                        topPosList.Clear(); // 새로운 최대값이 등장하면 초기화
                        topPosList.Add(worldPoint);
                    }
                    else if (Mathf.Approximately(worldPoint.y, maxY))
                    {
                        topPosList.Add(worldPoint); // 동일한 최대값인 경우 추가
                    }
                }
            }
        }

        return topPosList;
    }

    // x좌표 중복되는곳 제외
    List<Vector2> FilterMultiplyPositionX(List<Vector2> positions)
    {
        // x좌표가 key값 Vector2 (x,y) 좌표가 Value
        Dictionary<float, Vector2> uniquePositions = new Dictionary<float, Vector2>();

        foreach (Vector2 pos in positions)
        {
            // x좌표가 중복인지 확인
            if (uniquePositions.ContainsKey(pos.x))
            {
                // 동일한 x좌표의 경우 y값이 더 큰 좌표만 저장
                if (pos.y > uniquePositions[pos.x].y)
                {
                    uniquePositions[pos.x] = pos;
                }
            }
            else
            {
                // 새로운 x좌표 추가
                uniquePositions[pos.x] = pos;
            }
        }

        return uniquePositions.Values.ToList(); // 결과를 리스트로 반환
    }
    #endregion

    #region Player Random Pos Spawn Method
    private void PlayerRandomSpawn()
    {
        int randPosIndex = UnityEngine.Random.Range(0, _groundTopPos.Count);

        // TODO : Player가 선택한 탱크에 맞는 키값을 이용해서 알맞는 탱크 생성하기
        //          : 현재는 첫번째 탱크만 소환

        GameObject go = Instantiate(_tankPrefabList[0], _groundTopPos[randPosIndex], Quaternion.identity);

        // 중앙에서 오른쪽에 있다면 좌측을 바라보도록 뒤집기
        if(_groundTopPos[randPosIndex].x > 0)
        {
            go.transform.localScale = new Vector3(-1,1,1);
        }

        // 중복 좌표에 생성 안되도록 제거
        _groundTopPos.RemoveAt(randPosIndex);
    }
    #endregion
}
