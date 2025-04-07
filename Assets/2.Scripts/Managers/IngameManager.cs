using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Firebase.Database;
using Newtonsoft.Json;

public class IngameManager : MonoBehaviour
{
    public static IngameManager Instance;

    [Header("UI")]
    [SerializeField] private LoadingUIPlayerSlot playerSlotUI;
    [SerializeField] private LoadingUIPlayerSlot enemySlotUI;
    [SerializeField] private GameObject loadingUI;
    [SerializeField] private Text timerText;

    [Header("게임 설정")]
    [SerializeField] private float gameTime = 180f;
    [SerializeField] private float turnTime = 10f;

    private float remainingTime;
    private float remainingTurnTime;

    private bool gameStarted = false;
    private bool isMyTurn = true;

    private int playerHP;
    private int enemyHP;

    private int playerATK;
    private int enemyATK;

    private string enemyNowTankKey;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        SetupPlayerSlot();

        string enemyUID = MatchData.EnemyUID; // 서버 또는 매칭 시스템에서 받아야 함
        StartCoroutine(LoadEnemyDataAndStart(enemyUID));
    }

    void SetupPlayerSlot()
    {
        var userData = FirebaseManager._instance.userVO;
        Sprite tankSprite = TankUtil.GetTankSprite(userData.NowTank);
        var (wins, losses) = TankUtil.GetWinLoss(userData.BattleInfos);

        playerSlotUI.Setup(userData.NickName, tankSprite, wins, losses);
    }

    IEnumerator LoadEnemyDataAndStart(string enemyUID)
    {
        var dbRef = FirebaseDatabase.DefaultInstance
            .GetReference("UserDataSeat")
            .OrderByChild("UID")
            .EqualTo(enemyUID)
            .GetValueAsync();

        yield return new WaitUntil(() => dbRef.IsCompleted);

        if (dbRef.IsCompleted && dbRef.Result.Exists)
        {
            foreach (var snapshot in dbRef.Result.Children)
            {
                string json = snapshot.GetRawJsonValue();
                UserData enemyData = JsonConvert.DeserializeObject<UserData>(json);

                enemyNowTankKey = enemyData.NowTank;

                Sprite tankSprite = TankUtil.GetTankSprite(enemyNowTankKey);
                var (wins, losses) = TankUtil.GetWinLoss(enemyData.BattleInfos);

                enemySlotUI.Setup(enemyData.NickName, tankSprite, wins, losses);
                break;
            }
        }

        yield return new WaitForSeconds(2f);

        playerSlotUI.SetLoaded();
        enemySlotUI.SetLoaded();

        yield return new WaitForSeconds(0.5f);
        loadingUI.SetActive(false);

        StartGameplay();
    }

    void StartGameplay()
    {
        Debug.Log("게임 시작!");
        gameStarted = true;
        remainingTime = gameTime;
        remainingTurnTime = turnTime;

        // 탱크 스탯 반영
        var myTank = TankUtil.GetTankData(FirebaseManager._instance.userVO.NowTank);
        var enemyTank = TankUtil.GetTankData(enemyNowTankKey);

        if (myTank != null)
        {
            playerHP = myTank._hp;
            playerATK = myTank._atk;
        }

        if (enemyTank != null)
        {
            enemyHP = enemyTank._hp;
            enemyATK = enemyTank._atk;
        }

        StartTurn();
    }

    void Update()
    {
        if (!gameStarted) return;

        remainingTime -= Time.deltaTime;
        timerText.text = FormatTime(remainingTime);

        if (remainingTime <= 0f)
        {
            EndGame();
        }

        HandleTurnTimer();
    }

    void HandleTurnTimer()
    {
        if (!gameStarted) return;

        remainingTurnTime -= Time.deltaTime;

        if (remainingTurnTime <= 0f)
        {
            EndTurn();
        }
    }

    void StartTurn()
    {
        remainingTurnTime = turnTime;
        Debug.Log(isMyTurn ? "내 턴 시작" : "상대 턴 시작");
        // TODO: 턴에 따라 입력 활성화, AI 등 처리
    }

    void EndTurn()
    {
        isMyTurn = !isMyTurn;
        StartTurn();
    }

    public void TakeDamage(bool isEnemy, int damage)
    {
        if (isEnemy)
            enemyHP -= damage;
        else
            playerHP -= damage;

        Debug.Log($"{(isEnemy ? "적" : "플레이어")} 체력 감소: {damage}");

        if (playerHP <= 0 || enemyHP <= 0)
        {
            EndGame();
        }
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    void EndGame()
    {
        Debug.Log("게임 종료!");
        gameStarted = false;
        // TODO: 결과 화면 처리, 데이터 저장 등
    }
}

public static class MatchData
{
    public static string EnemyUID;
}
