using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    private const float SPAWN_OFFEST_X = 1.5f;
    private const float SPAWN_OFFSET_Y = 1.5f;
    private const int SPAWN_POS_REMOVE_RANGE = 2;      // 스폰 위치 주위 삭제 범위 (플레이어간 너무 가까운곳에 생성 방지)
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
    private List<Vector2Int> FindGroundTopPos()
    {
        List<Vector2Int> topPosListAll = new List<Vector2Int>(); // 모든 그라운드 top pos 리스트

        foreach (var colider in _polygonCollider2DList)
        {
            if (colider == null)
                continue;

            int maxY = int.MinValue;
            HashSet<Vector2Int> coliderTopPosSet = new HashSet<Vector2Int>(); // 중복 제거를 위한 HashSet

            // 모든 경로 순회하며 최상단 좌표 탐색
            for (int pathIndex = 0; pathIndex < colider.pathCount; pathIndex++)
            {
                Vector2[] points = colider.GetPath(pathIndex);

                foreach (Vector2 point in points)
                {
                    Vector2 worldPoint = colider.transform.TransformPoint(point);

                    // x 좌표: SPAWN_OFFSET_X 보정 후 변환
                    int posIntX = Mathf.RoundToInt(worldPoint.x - Mathf.Sign(worldPoint.x) * SPAWN_OFFEST_X);
                    // y 좌표: SPAWN_OFFSET_Y 보정 후 변환
                    int posIntY = Mathf.CeilToInt(worldPoint.y + SPAWN_OFFSET_Y);

                    Vector2Int worldPointInt = new Vector2Int(posIntX, posIntY);

                    // 최상단 y값 갱신
                    if (posIntY > maxY)
                    {
                        maxY = posIntY;
                        coliderTopPosSet.Clear(); // 새로운 최상단 발견 시 초기화
                        coliderTopPosSet.Add(worldPointInt);
                    }
                    else if (posIntY == maxY)
                    {
                        coliderTopPosSet.Add(worldPointInt); // 같은 높이의 좌표 추가
                    }
                }
            }

            // 최상단 좌표 리스트 정렬 (x 값 기준으로 정렬)
            List<Vector2Int> coliderTopPosList = coliderTopPosSet.ToList();

            // 최대 ~ 최소까지 1 단위 간격으로 좌표 생성
            List<Vector2Int> topLinePos = GenerateTopLine(coliderTopPosList);

            // 결과 리스트에 추가
            topPosListAll.AddRange(topLinePos);
        }

        return topPosListAll;
    }

    // 최상단 좌표를 기반으로 연속적인 x 좌표 리스트 생성
    private List<Vector2Int> GenerateTopLine(List<Vector2Int> topPositions)
    {
        List<Vector2Int> topNewPos = new List<Vector2Int>();

        if (topPositions.Count == 0) return topNewPos;

        int startX = topPositions[0].x;
        int endX = topPositions[topPositions.Count - 1].x;
        int y = topPositions[0].y; // 모든 y 값이 같으므로 하나만 가져옴

        for (int x = startX; x >= endX; x--)
        {
            topNewPos.Add(new Vector2Int(x, y));
        }

        return topNewPos;
    }

    // x좌표 중복되는곳 제외
    List<Vector2Int> FilterMultiplyPositionX(List<Vector2Int> positions)
    {
        // x좌표가 key값 Vector2Int (x,y) 좌표가 Value
        Dictionary<float, Vector2Int> uniquePositions = new Dictionary<float, Vector2Int>();

        foreach (Vector2Int pos in positions)
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
        // 랜덤 좌표 생성
        int randPosIndex = UnityEngine.Random.Range(0, _groundTopPos.Count);
        float posX = _groundTopPos[randPosIndex].x;
        float posY = _groundTopPos[randPosIndex].y;
        Vector3 randSpawnPos = new Vector3(posX, posY);

        // TODO : Player가 선택한 탱크에 맞는 키값을 이용해서 알맞는 탱크 생성하기
        //          : 현재는 첫번째 탱크만 소환
        GameObject go = Instantiate(_tankPrefabList[0], randSpawnPos, Quaternion.identity);

        // 중앙에서 오른쪽에 있다면 좌측을 바라보도록 뒤집기
        if(_groundTopPos[randPosIndex].x > 0)
        {
            go.transform.localScale = new Vector3(-1,1,1);
        }

        // 중복 좌표에 생성 안되도록 제거
        int minRemoveIndex = Mathf.Max(0,randPosIndex - SPAWN_POS_REMOVE_RANGE);
        int maxRemoveIndex = Mathf.Min(randPosIndex + SPAWN_POS_REMOVE_RANGE, _groundTopPos.Count-1);
        
        for (int i = maxRemoveIndex; i >= minRemoveIndex; i--)
        {
            _groundTopPos.RemoveAt(i);
        }
    }
    #endregion
}
