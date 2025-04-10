using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    #region �ӽ� �̱���
    public static GameInitializer Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    #endregion

    private const float TURN_END_TERM = 3f;         // �� ���� �ð�

    [Header("Info")]
    [SerializeField] private MapSpawner _mapSpawner;
    [SerializeField] private List<GameObject> _tankPrefabList;
    [SerializeField] private eMapType _selectedMapType = eMapType.Random;
    [SerializeField] private int _playerCount = 0;

    [Header("Environment")]
    [Range(0f,10f)]
    [SerializeField] private float _windForceMax = 0f;
    [SerializeField] private float _curWindForce = 0f;

    private CameraController _camController = null;
    private List<PlayerController> _playerList = new List<PlayerController>();

    private int _curTurnPlayerIndex = 0;
    public PlayerController CurTurnPlayer { get => _playerList[_curTurnPlayerIndex]; }
    public Transform CurShellTrans { get; set; }

    private bool _isTurnWait = false;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();
    }

    public void Init(Action pCallback = null)
    {
        // ������ �� ����
        _mapSpawner.SpawnSelectMap(_selectedMapType);

        if (_playerCount == 0)
        {
            Debug.Log("No Player");
            return;
        }

        // ĳ���� ���� ���� ��ǥ ��ġ
        PlayerRandomPosSpawn();

        // ī�޶� �ʱ�ȭ
        _camController = Camera.main.GetComponent<CameraController>();

        if (_camController != null)
            _camController.Init();

        // ���� ������ �� ����
        _curTurnPlayerIndex = UnityEngine.Random.Range(0, _playerCount);

        Debug.Log($"First Turn Player : {CurTurnPlayer.name}");

        _camController.PlayerFocusing(CurTurnPlayer);
        CurTurnPlayer.IsMyTurn();

        // �ٶ� ����
        RandomWindForce();

        if (pCallback != null)
            pCallback.Invoke();
    }

    // ���� ��ǥ ����߿� �������� �����ؼ� �÷��̾� ĳ���� ��ġ�ϴ� �޼ҵ�
    private void PlayerRandomPosSpawn()
    {
        List<Vector3> posList = _mapSpawner.GetSpawnPosPList();
        int randPosIndex = 0;

        // �÷��̾��� ����ŭ ���� ��ǥ�� ����
        for (int i = 0; i < _playerCount; i++)
        {
            randPosIndex = UnityEngine.Random.Range(0, posList.Count);

            // �ӽ÷� 0��° ������ ����
            Vector2 randPos = (Vector2)posList[randPosIndex];
            GameObject go = Instantiate(_tankPrefabList[0], randPos, Quaternion.identity);
            go.name = $"Player {i}";

            // �÷��̾� �ʱ�ȭ
            PlayerController player = go.GetComponent<PlayerController>();
            player.Init();

            // x��ǥ�� 0���� ū ��� ������ �ٶ󺸵���
            if (player.transform.position.x > 0)
            {
                player.Flip(-1);
            }

            _playerList.Add(player);

            // ���� �ڸ��� ���� ��ǥ ��Ͽ��� ����
            posList.RemoveAt(randPosIndex);
        }
    }

    public void PlayerTurnEnd()
    {
        _isTurnWait = true;

        StartCoroutine(StartNextPlayerTurn());
    }

    private IEnumerator StartNextPlayerTurn()
    {
        // �� �ε��� ����
        int nextIndex = (_curTurnPlayerIndex + 1) % _playerCount;
        _curTurnPlayerIndex = nextIndex;

        Debug.Log($"Now Turn Player : {CurTurnPlayer.name}");

        // ���� ź�� �������ٸ� ���
        while (CurShellTrans != null)
            yield return null;

        // �������� ���ð�
        yield return new WaitForSeconds(TURN_END_TERM);

        // �ٶ� ����
        RandomWindForce();

        // ���� �÷��̾� �� ����
        _camController.PlayerFocusing(CurTurnPlayer);
        CurTurnPlayer.IsMyTurn();

        _isTurnWait = false;
    }

    private void RandomWindForce()
    {
        // �ݿø� �� �Ҽ��� ��°�ڸ�������
        _curWindForce = Mathf.Round(UnityEngine.Random.Range(-_windForceMax, _windForceMax) * 100f) / 100f;
    }

    public float GetWindForce()
    {
        return _curWindForce;
    }


    public bool IsCurPlayerTurnWait()
    {
        return _isTurnWait;
    }

    public Vector2 GetMapSize()
    {
        if (_mapSpawner == null)
            return Vector2.zero;

        return _mapSpawner.GetMapSize();
    }

    //#region Ground Top Pos Find Method
    //private List<Vector2Int> FindGroundTopPos()
    //{
    //    List<Vector2Int> topPosListAll = new List<Vector2Int>(); // ��� �׶��� top pos ����Ʈ

    //    foreach (var colider in _polygonCollider2DList)
    //    {
    //        if (colider == null)
    //            continue;

    //        int maxY = int.MinValue;
    //        HashSet<Vector2Int> coliderTopPosSet = new HashSet<Vector2Int>(); // �ߺ� ���Ÿ� ���� HashSet

    //        // ��� ��� ��ȸ�ϸ� �ֻ�� ��ǥ Ž��
    //        for (int pathIndex = 0; pathIndex < colider.pathCount; pathIndex++)
    //        {
    //            Vector2[] points = colider.GetPath(pathIndex);

    //            foreach (Vector2 point in points)
    //            {
    //                Vector2 worldPoint = colider.transform.TransformPoint(point);

    //                // x ��ǥ: SPAWN_OFFSET_X ���� �� ��ȯ
    //                int posIntX = Mathf.RoundToInt(worldPoint.x - Mathf.Sign(worldPoint.x) * SPAWN_OFFEST_X);
    //                // y ��ǥ: SPAWN_OFFSET_Y ���� �� ��ȯ
    //                int posIntY = Mathf.CeilToInt(worldPoint.y + SPAWN_OFFSET_Y);

    //                Vector2Int worldPointInt = new Vector2Int(posIntX, posIntY);

    //                // �ֻ�� y�� ����
    //                if (posIntY > maxY)
    //                {
    //                    maxY = posIntY;
    //                    coliderTopPosSet.Clear(); // ���ο� �ֻ�� �߰� �� �ʱ�ȭ
    //                    coliderTopPosSet.Add(worldPointInt);
    //                }
    //                else if (posIntY == maxY)
    //                {
    //                    coliderTopPosSet.Add(worldPointInt); // ���� ������ ��ǥ �߰�
    //                }
    //            }
    //        }

    //        // �ֻ�� ��ǥ ����Ʈ ���� (x �� �������� ����)
    //        List<Vector2Int> coliderTopPosList = coliderTopPosSet.ToList();

    //        // �ִ� ~ �ּұ��� 1 ���� �������� ��ǥ ����
    //        List<Vector2Int> topLinePos = GenerateTopLine(coliderTopPosList);

    //        // ��� ����Ʈ�� �߰�
    //        topPosListAll.AddRange(topLinePos);
    //    }

    //    return topPosListAll;
    //}

    //// �ֻ�� ��ǥ�� ������� �������� x ��ǥ ����Ʈ ����
    //private List<Vector2Int> GenerateTopLine(List<Vector2Int> topPositions)
    //{
    //    List<Vector2Int> topNewPos = new List<Vector2Int>();

    //    if (topPositions.Count == 0) return topNewPos;

    //    int startX = topPositions[0].x;
    //    int endX = topPositions[topPositions.Count - 1].x;
    //    int y = topPositions[0].y; // ��� y ���� �����Ƿ� �ϳ��� ������

    //    for (int x = startX; x >= endX; x--)
    //    {
    //        topNewPos.Add(new Vector2Int(x, y));
    //    }

    //    return topNewPos;
    //}

    //// x��ǥ �ߺ��Ǵ°� ����
    //List<Vector2Int> FilterMultiplyPositionX(List<Vector2Int> positions)
    //{
    //    // x��ǥ�� key�� Vector2Int (x,y) ��ǥ�� Value
    //    Dictionary<float, Vector2Int> uniquePositions = new Dictionary<float, Vector2Int>();

    //    foreach (Vector2Int pos in positions)
    //    {
    //        // x��ǥ�� �ߺ����� Ȯ��
    //        if (uniquePositions.ContainsKey(pos.x))
    //        {
    //            // ������ x��ǥ�� ��� y���� �� ū ��ǥ�� ����
    //            if (pos.y > uniquePositions[pos.x].y)
    //            {
    //                uniquePositions[pos.x] = pos;
    //            }
    //        }
    //        else
    //        {
    //            // ���ο� x��ǥ �߰�
    //            uniquePositions[pos.x] = pos;
    //        }
    //    }

    //    return uniquePositions.Values.ToList(); // ����� ����Ʈ�� ��ȯ
    //}
    //#endregion

    //#region Player Random Pos Spawn Method
    //private void PlayerRandomSpawn()
    //{
    //    // ���� ��ǥ ����
    //    int randPosIndex = UnityEngine.Random.Range(0, _groundTopPos.Count);
    //    float posX = _groundTopPos[randPosIndex].x;
    //    float posY = _groundTopPos[randPosIndex].y;
    //    Vector3 randSpawnPos = new Vector3(posX, posY);

    //    // TODO : Player�� ������ ��ũ�� �´� Ű���� �̿��ؼ� �˸´� ��ũ �����ϱ�
    //    //          : ����� ù��° ��ũ�� ��ȯ
    //    GameObject go = Instantiate(_tankPrefabList[0], randSpawnPos, Quaternion.identity);

    //    // �߾ӿ��� �����ʿ� �ִٸ� ������ �ٶ󺸵��� ������
    //    if(_groundTopPos[randPosIndex].x > 0)
    //    {
    //        go.transform.localScale = new Vector3(-1,1,1);
    //    }

    //    // �ߺ� ��ǥ�� ���� �ȵǵ��� ����
    //    int minRemoveIndex = Mathf.Max(0,randPosIndex - SPAWN_POS_REMOVE_RANGE);
    //    int maxRemoveIndex = Mathf.Min(randPosIndex + SPAWN_POS_REMOVE_RANGE, _groundTopPos.Count-1);

    //    for (int i = maxRemoveIndex; i >= minRemoveIndex; i--)
    //    {
    //        _groundTopPos.RemoveAt(i);
    //    }
    //}
    //#endregion
}
