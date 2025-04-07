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

    [Header("게임 설정")]
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
        yield return new WaitForSeconds(2f); // 연출 대기 등

        gameTimer.Value = totalGameTime;
        currentTurnClientId.Value = NetworkManager.ConnectedClientsIds[0]; // 첫 턴은 호스트
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
            Debug.Log("내 턴입니다!");
            // 입력 활성화
        }
        else
        {
            Debug.Log("상대 턴입니다.");
            // 입력 비활성화
        }
    }

    void EndGame()
    {
        isGameRunning = false;
        Debug.Log("게임 종료!");
        // 승패 판정 및 결과 전송 등
    }

    public bool IsMyTurn()
    {
        return currentTurnClientId.Value == NetworkManager.Singleton.LocalClientId;
    }
}
