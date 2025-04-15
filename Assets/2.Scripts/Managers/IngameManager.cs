using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class IngameManager : NetworkBehaviour
{
    public static IngameManager Instance;

    [Header("�ε� UI")]
    [SerializeField] GameObject _loadingUI;
    [SerializeField] Text _loadingText;

    [Header("�� ����")]
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
            _loadingUI.SetActive(true); // Ŭ���̾�Ʈ�� �ε� UI ǥ��
        }

        if (IsServer)
        {
            _isGameStarted = false; // ������ ���� �ȵ� ���·� �ʱ�ȭ
        }
    }

    // �� ���� �Ϸ�� ȣ��
    public void InitMapDone()
    {
        _isMapSpawned = true;
        InitAllDOne();
    }

    // ��ũ ��ġ �Ϸ�� ȣ��
    public void InitTankDone()
    {
        _isTankSpawned = true;
        InitAllDOne();
    }

    // �� �� �Ϸ�Ǿ����� Ȯ�� �� ������ �غ� �Ϸ� �˸�
    void InitAllDOne()
    {
        if (_isMapSpawned && _isTankSpawned)
        {
            ReportClientReadyServerRpc();
        }
    }

    // Ŭ���̾�Ʈ�� �غ� �Ϸ���� ������ �˸�
    [ServerRpc(RequireOwnership = false)]
    void ReportClientReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        Debug.Log($"[����] Ŭ���̾�Ʈ {senderId} �غ� �Ϸ�");
        _clientReadyDict[senderId] = true;

        if (AllClientsReady())
        {
            StartCoroutine(StartGameRoutine());
        }
    }

    // ��� Ŭ���̾�Ʈ�� �غ� �Ϸ� �������� Ȯ��
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

    // ���� ���� ��ƾ - �������� ù �� Ŭ���̾�Ʈ ���� �� ���� ���� �˸�
    IEnumerator StartGameRoutine()
    {
        yield return new WaitForSeconds(1f);

        ulong randomId = NetworkManager.Singleton.ConnectedClientsIds[Random.Range(0, NetworkManager.Singleton.ConnectedClientsIds.Count)];
        _currentTurnClientId.Value = randomId;
        _turnTimer.Value = _turnTime;

        NotifyGameStartClientRpc(randomId);
        _isGameStarted = true;
        OnTurnStarted(randomId); // ù �� ���� �˸�
    }

    // Ŭ���̾�Ʈ���� ���� ���� �˸� + �ε� UI ����
    [ClientRpc]
    void NotifyGameStartClientRpc(ulong firstTurnClientId)
    {
        _loadingUI.SetActive(false);
        Debug.Log($"[Ŭ���̾�Ʈ] ���� ����, ù ��: {firstTurnClientId}");
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

    // ������ �Ϸ�Ǿ��� �� ȣ��� (Ŭ���̾�Ʈ����)
    public void NotifyAttackCompleted()
    {
        if (!IsMyTurn() || _isAttackResolving)
        {
            return;
        }
        StartCoroutine(DelayedEndTurn());
    }

    // ���� �� �����̸� �ΰ� ���� �ѱ�
    IEnumerator DelayedEndTurn()
    {
        _isAttackResolving = true;
        yield return new WaitForSeconds(_postAttackDelay);
        ChangeTurn();
        _isAttackResolving = false;
    }

    // ���� ���� Ŭ���̾�Ʈ�� �����ϰ� �˸�
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

    // Ŭ���̾�Ʈ���� �� ���� �˸�
    [ClientRpc]
    void SendTurnChangedClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log("[�� ��] ���� Ȱ��ȭ");
        }
        else
        {
            Debug.Log("[��� ��] ��� ��");
        }
    }

    // �� ���� �� Ŭ���̾�Ʈ���� ȣ��� (ī�޶� �̵��̳� UI ǥ�� � ��� ����)
    void OnTurnStarted(ulong turnClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == turnClientId)
        {
            Debug.Log("[OnTurnStarted] �� �� ����!");
        }
    }

    // �÷��̾� ��� �� ������ �˸�
    [ServerRpc(RequireOwnership = false)]
    public void NotifyDeadServerRpc(ulong clientId)
    {
        Debug.Log($"���� ����: Ŭ���̾�Ʈ {clientId} ���");
        _isGameStarted = false;
    }

    // ���� ���� Ŭ���̾�Ʈ�� ���� ������ �ִ��� Ȯ��
    public bool IsMyTurn()
    {
        return _currentTurnClientId.Value == NetworkManager.Singleton.LocalClientId;
    }
}

public static class MatchData
{
    public static string EnemyUID;
}
