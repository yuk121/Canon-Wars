using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

public class IngameManager : NetworkBehaviour
{
    public static IngameManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject loadingUI;
    [SerializeField] private Text timerText;

    [Header("���� ����")]
    [SerializeField] private float totalGameTime = 180f;
    [SerializeField] private float turnTime = 10f;

    private NetworkVariable<float> gameTimer = new NetworkVariable<float>();
    private NetworkVariable<ulong> currentTurnClientId = new NetworkVariable<ulong>();

    private float turnTimer;
    private bool isGameRunning = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(GameStartRoutine());
        }

        if (IsClient)
        {
            loadingUI.SetActive(true);
            StartCoroutine(HideLoadingAfterDelay(2f));
        }
    }

    IEnumerator GameStartRoutine()
    {
        yield return new WaitForSeconds(2f); // ���� ��� ��

        gameTimer.Value = totalGameTime;
        currentTurnClientId.Value = NetworkManager.ConnectedClientsIds[0]; // ù ���� ȣ��Ʈ
        isGameRunning = true;
    }

    IEnumerator HideLoadingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        loadingUI.SetActive(false);
    }

    void Update()
    {
        if (!IsServer || !isGameRunning) return;

        gameTimer.Value -= Time.deltaTime;
        turnTimer -= Time.deltaTime;

        if (gameTimer.Value <= 0f)
        {
            EndGame();
        }

        if (turnTimer <= 0f)
        {
            ChangeTurn();
        }
    }

    void ChangeTurn()
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId != currentTurnClientId.Value)
            {
                currentTurnClientId.Value = clientId;
                break;
            }
        }

        turnTimer = turnTime;
        SendTurnChangedClientRpc(currentTurnClientId.Value);
    }

    [ClientRpc]
    void SendTurnChangedClientRpc(ulong newTurnClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == newTurnClientId)
        {
            Debug.Log("�� ���Դϴ�!");
            // �Է� Ȱ��ȭ
        }
        else
        {
            Debug.Log("��� ���Դϴ�.");
            // �Է� ��Ȱ��ȭ
        }
    }

    void EndGame()
    {
        isGameRunning = false;
        Debug.Log("���� ����!");
        // ���� ���� �� ��� ���� ��
    }

    public bool IsMyTurn()
    {
        return currentTurnClientId.Value == NetworkManager.Singleton.LocalClientId;
    }
}
