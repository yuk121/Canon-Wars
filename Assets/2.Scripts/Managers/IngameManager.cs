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

    [Header("���� ����")]
    [SerializeField] private float gameTime = 180f;

    private float remainingTime;
    private bool gameStarted = false;

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

        string enemyUID = MatchData.EnemyUID; // ���� �Ǵ� ��Ī �ý��ۿ��� �޾ƾ� ��
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

                Sprite tankSprite = TankUtil.GetTankSprite(enemyData.NowTank);
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
        Debug.Log("���� ����!");
        gameStarted = true;
        remainingTime = gameTime;
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
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    void EndGame()
    {
        Debug.Log("���� ����!");
        gameStarted = false;
        // ��� ó�� �� �߰� ����
    }
}

public static class MatchData
{
    public static string EnemyUID; // ���� ��Ī �� �������� ���� UID ����
}
