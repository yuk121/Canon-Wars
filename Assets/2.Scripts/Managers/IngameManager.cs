using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class IngameManager : NetworkBehaviour
{
    public static IngameManager Instance;

    [Header("로딩 UI")]
    [SerializeField] GameObject _loadingUI;
    [SerializeField] Text _loadingText;

    [Header("턴 설정")]
    [SerializeField] float _turnTime = 40f;
    [SerializeField] float _postAttackDelay = 10f;
    [SerializeField] Text _turnTimerText;

    bool _isMapSpawned = false;
    bool _isTankSpawned = false;
    bool _isGameStarted = false;
    bool _isAttackResolving = false;

    NetworkVariable<ulong> _currentTurnClientId = new();
    NetworkVariable<float> _turnTimer = new();

    Dictionary<ulong, bool> _clientReadyDict = new();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient && !IsServer)
        {
            _loadingUI.SetActive(true); // 클라이언트는 로딩 UI 표시
        }

        if (IsServer)
        {
            _isGameStarted = false; // 서버는 시작 안된 상태로 초기화
        }
    }

    // 맵 생성 완료시 호출
    public void InitMapDone()
    {
        _isMapSpawned = true;
        InitAllDOne();
    }

    // 탱크 배치 완료시 호출
    public void InitTankDone()
    {
        _isTankSpawned = true;
        InitAllDOne();
    }

    // 둘 다 완료되었는지 확인 후 서버에 준비 완료 알림
    void InitAllDOne()
    {
        if (_isMapSpawned && _isTankSpawned)
        {
            ReportClientReadyServerRpc();
        }
    }

    // 클라이언트가 준비 완료됨을 서버에 알림
    [ServerRpc(RequireOwnership = false)]
    void ReportClientReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        Debug.Log($"[서버] 클라이언트 {senderId} 준비 완료");
        _clientReadyDict[senderId] = true;

        if (AllClientsReady())
        {
            StartCoroutine(StartGameRoutine());
        }
    }

    // 모든 클라이언트가 준비 완료 상태인지 확인
    bool AllClientsReady()
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!_clientReadyDict.ContainsKey(clientId) || !_clientReadyDict[clientId])
            {
                return false;
            }
        }
        return true;
    }

    // 게임 시작 루틴 - 랜덤으로 첫 턴 클라이언트 선택 후 게임 시작 알림
    IEnumerator StartGameRoutine()
    {
        yield return new WaitForSeconds(1f);

        ulong randomId = NetworkManager.Singleton.ConnectedClientsIds[Random.Range(0, NetworkManager.Singleton.ConnectedClientsIds.Count)];
        _currentTurnClientId.Value = randomId;
        _turnTimer.Value = _turnTime;

        NotifyGameStartClientRpc(randomId);
        _isGameStarted = true;
        OnTurnStarted(randomId); // 첫 턴 시작 알림
    }

    // 클라이언트에게 게임 시작 알림 + 로딩 UI 제거
    [ClientRpc]
    void NotifyGameStartClientRpc(ulong firstTurnClientId)
    {
        _loadingUI.SetActive(false);
        Debug.Log($"[클라이언트] 게임 시작, 첫 턴: {firstTurnClientId}");
    }

    void Update()
    {
        if (!IsServer || !_isGameStarted || _isAttackResolving) return;

        _turnTimer.Value -= Time.deltaTime;
        if (_turnTimerText != null)
        {
            _turnTimerText.text = Mathf.CeilToInt(_turnTimer.Value).ToString();
        }

        if (_turnTimer.Value <= 0f)
        {
            ChangeTurn();
        }
    }

    // 공격이 완료되었을 때 호출됨 (클라이언트에서)
    public void NotifyAttackCompleted()
    {
        if (!IsMyTurn() || _isAttackResolving)
        {
            return;
        }
        StartCoroutine(DelayedEndTurn());
    }

    // 공격 후 딜레이를 두고 턴을 넘김
    IEnumerator DelayedEndTurn()
    {
        _isAttackResolving = true;
        yield return new WaitForSeconds(_postAttackDelay);
        ChangeTurn();
        _isAttackResolving = false;
    }

    // 다음 턴의 클라이언트를 설정하고 알림
    void ChangeTurn()
    {
        foreach (var id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (id != _currentTurnClientId.Value)
            {
                _currentTurnClientId.Value = id;
                _turnTimer.Value = _turnTime;
                SendTurnChangedClientRpc(id);
                OnTurnStarted(id);
                break;
            }
        }
    }

    // 클라이언트에게 턴 변경 알림
    [ClientRpc]
    void SendTurnChangedClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log("[내 턴] 조작 활성화");
        }
        else
        {
            Debug.Log("[상대 턴] 대기 중");
        }
    }

    // 턴 시작 시 클라이언트에서 호출됨 (카메라 이동이나 UI 표시 등에 사용 가능)
    void OnTurnStarted(ulong turnClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == turnClientId)
        {
            Debug.Log("[OnTurnStarted] 내 턴 시작!");
        }
    }

    // 플레이어 사망 시 서버에 알림
    [ServerRpc(RequireOwnership = false)]
    public void NotifyDeadServerRpc(ulong clientId)
    {
        Debug.Log($"게임 종료: 클라이언트 {clientId} 사망");
        _isGameStarted = false;
    }

    // 현재 로컬 클라이언트가 턴을 가지고 있는지 확인
    public bool IsMyTurn()
    {
        return _currentTurnClientId.Value == NetworkManager.Singleton.LocalClientId;
    }
}

public static class MatchData
{
    public static string EnemyUID;
}
