using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using Newtonsoft;
using Newtonsoft.Json;
using UnityEngine.Events;
using System.Linq;


public enum ERROR_State
{
    NONE,
    DonSearch_ID,
    DonSearch_PW
}


public class FirebaseManager : MonoBehaviour
{
    public UserData userVO;
    private DatabaseReference token;

    [Space(5)]
    public ERROR_State eState;

    [Space(10)]
    public InputField if_testID; //테스트용 inputField
    public InputField if_testPW; //테스트용 inputField

    [Space(10)]
    public UnityEvent loginCallback;

    async void Start()
    {
        await InitializeFirebase();
    }

    async Task InitializeFirebase()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;

            // 🔥 Database URL을 여기에 추가해야 함!
            string databaseUrl = "https://canon-wars-41ce5-default-rtdb.firebaseio.com/";
            token = FirebaseDatabase.GetInstance(app, databaseUrl).RootReference;

            Debug.Log("🔥 Firebase Initialized Successfully!");
        }
        else
        {
            Debug.LogError($"❌ Firebase Initialization Failed: {dependencyStatus}");
        }
    }


    //계정 생성 버튼
    public void Create_UserAccount()
    {
        Create_UserAccount(if_testID.text, if_testPW.text);
    }

    //계정 생성 기능
    void Create_UserAccount(string a_UserID, string a_UserPW)
    {

        UserData userData = new UserData();
        userData.UserID = a_UserID;
        userData.UserPW = a_UserPW;
        userData.UID = DateTime.Now.ToString("yyyyMMddHHmmss");

        ///---- User 테이블에 user 정보 저장.
        {
            string userTable_path = "UserDataSeat/" + userData.UserID;  // 데이터베이스에서 저장할 위치 설정

            // Firebase에 데이터 업로드 (UserID, UserPW, UID)
            token.Child(userTable_path).SetRawJsonValueAsync(JsonConvert.SerializeObject(userData)).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("User data uploaded successfully.");
                }
                else
                {
                    Debug.LogError("Error uploading user data.");
                }
            });
        }
        ///-----------------------------------------

        ///---- User의 보유 Cannon Table에 User의 정보 생성.
        {
            string cannonTable_path = "UserCannonSeat/" + userData.UID;
            
            ///firebase에는 null 값으로 data를 저장할 수 없음. 따라서 임시 0번째의 값을 빈 값으로 객체를 직렬화하여 seat를 초기화함.
            UserCannon userCannon = new UserCannon();
            userCannon.CannonKeys = new List<string>();
            userCannon.CannonKeys.Add("");

            token.Child(cannonTable_path).SetRawJsonValueAsync(JsonConvert.SerializeObject(userCannon)).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("User data uploaded successfully.");
                }
                else
                {
                    Debug.LogError("Error uploading user data.");
                }
            });
        }
        ///-------------------------------------------

        ///---- User의 전적 관리 Table에 User 정보 생성.
        {
            string battleInfoTable_path = "UserBattleInfoSeat/" + userData.UID;

            ///firebase에는 null 값으로 data를 저장할 수 없음. 따라서 임시 0번째의 값을 빈 값으로 객체를 직렬화하여 seat를 초기화함.
            Wrapper info = new Wrapper();
            info.BattleDatas = new List<UserBattleInfo>
            {
                new UserBattleInfo()
                {
                    date = "",
                    result = ""
                }
            };

            string json = JsonConvert.SerializeObject(info);

            token.Child(battleInfoTable_path).SetRawJsonValueAsync(json).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("User data uploaded successfully.");
                }
                else
                {
                    Debug.LogError("Error uploading user data.");
                }
            });
        }

        ///------------------------------------------

    }

    //로그인 버튼
    public void Login_Function()
    {
        Login(if_testID.text, if_testPW.text, delegate { loginCallback.Invoke();  });
    }

    //로그인 기능
    async void Login(string a_UserID, string a_UserPW, Action callback)
    {
        //USER SEAT에서 유저 정보 찾기.
        {
            UserData tempUserData = new UserData();
            string path = "UserDataSeat";

            await token.Child(path).OrderByKey().EqualTo(a_UserID).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;

                    if (snapshot.Exists)
                    {
                        string value = snapshot.Child(a_UserID).GetRawJsonValue();

                        if (value == null)
                        {
                            eState = ERROR_State.DonSearch_ID; //ID 없음.
                            return;
                        }

                        tempUserData = JsonConvert.DeserializeObject<UserData>(value);
                    }
                }
             });



            if (tempUserData.UserPW != a_UserPW)
            {
                eState = ERROR_State.DonSearch_PW; //PW 불일치.
                return;
            }

            //voSet
            userVO.UID = tempUserData.UID;
            userVO.UserID = tempUserData.UserID;
            userVO.UserPW = tempUserData.UserPW;
         }

        //user battleinfoseat에서 유저 정보 찾기.
        {
            string path = "UserBattleInfoSeat";
            // 특정 uid가 있는 데이터를 찾음
            var query = token.Child(path).OrderByKey().EqualTo(userVO.UID);

            await query.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;

                    if (snapshot.Exists)
                    {
                        // uid의 BattleDatas를 가져옴
                        foreach (var child in snapshot.Children)
                        {
                            string battleDataJson = child.Child("BattleDatas").GetRawJsonValue();
                            userVO.BattleInfos = JsonConvert.DeserializeObject<List<UserBattleInfo>>(battleDataJson);
                        }
                    }
                }
            });
        }

        //user CannonSeat에서 유저 정보 찾기.
        {
            string path = "UserCannonSeat";
            // 특정 uid가 있는 데이터를 찾음
            var query = token.Child(path).OrderByKey().EqualTo(userVO.UID);

            await query.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;

                    if (snapshot.Exists)
                    {
                        // uid의 BattleDatas를 가져옴
                        foreach (var child in snapshot.Children)
                        {
                            string CannonDataJson = child.Child("CannonKeys").GetRawJsonValue();
                            userVO.CannonInfos.CannonKeys = JsonConvert.DeserializeObject<List<string>>(CannonDataJson);
                        }
                    }
                }
            });
        }

        eState = ERROR_State.NONE;
        if(callback != null)
            callback.Invoke();
    }


    //비밀번호 찾기 버튼
    public void SearchPW_Function()
    {
        Get_UserPW(if_testID.text, delegate { Debug.Log(userVO.UserPW); });
    }

    //비밀번호 찾기 기능
    async void Get_UserPW(string a_UserID, Action callback)
    {
        //USER SEAT에서 유저 정보 찾기.
        {
            UserData tempUserData = new UserData();
            string path = "UserDataSeat";

            await token.Child(path).OrderByKey().EqualTo(a_UserID).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;

                    if (snapshot.Exists)
                    {
                        string value = snapshot.Child(a_UserID).GetRawJsonValue();

                        if (value == null)
                        {
                            eState = ERROR_State.DonSearch_ID; //ID 없음.
                            return;
                        }

                        tempUserData = JsonConvert.DeserializeObject<UserData>(value);
                    }
                }
            });

            userVO.UID = tempUserData.UID;
            userVO.UserID = tempUserData.UserID;
            userVO.UserPW = tempUserData.UserPW;

        }

        if (callback != null)
            callback.Invoke();
    }

    //비밀번호 변경 버튼
    public void UpdatePW_Function()
    {
        Update_UserPW(if_testID.text, if_testPW.text, delegate { Debug.Log(userVO.UserPW); });
    }
    
    //비밀번호 변경 기능
    async void Update_UserPW(string a_UserID, string a_newUserPW, Action callback)
    {
        //USER SEAT에서 유저 정보 찾기.
        {
            UserData tempUserData = new UserData();
            string path = "UserDataSeat";

            await token.Child(path).OrderByKey().EqualTo(a_UserID).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;

                    if (snapshot.Exists)
                    {
                        string value = snapshot.Child(a_UserID).GetRawJsonValue();

                        if (value == null)
                        {
                            eState = ERROR_State.DonSearch_ID; //ID 없음.
                            return;
                        }

                        tempUserData = JsonConvert.DeserializeObject<UserData>(value);
                    }
                }
            });

            userVO.UID = tempUserData.UID;
            userVO.UserID = tempUserData.UserID;
            userVO.UserPW = a_newUserPW;

        }


        {
            string userTable_path = "UserDataSeat/" + userVO.UserID;  // 데이터베이스에서 저장할 위치 설정

            // Firebase에 데이터 업로드 (UserID, UserPW, UID)
            await token.Child(userTable_path).SetRawJsonValueAsync(JsonConvert.SerializeObject(userVO)).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("User data uploaded successfully.");
                }
                else
                {
                    Debug.LogError("Error uploading user data.");
                }
            });
        }

        if (callback != null)
            callback.Invoke();
    }


    //유저 정보- Cannon List 를 Database에 저장.
    void Update_UserCannon()
    {
        {
            string cannonTable_path = "UserCannonSeat/" + userVO.UID;

            UserCannon userCannon = new UserCannon();

            userCannon.CannonKeys = userVO.CannonInfos.CannonKeys.ToList();

            for (int i = 0; i < userCannon.CannonKeys.Count;)
            {
                if (userCannon.CannonKeys[i] == string.Empty)
                {
                    userCannon.CannonKeys.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            token.Child(cannonTable_path).SetRawJsonValueAsync(JsonConvert.SerializeObject(userCannon)).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("User data uploaded successfully.");
                }
                else
                {
                    Debug.LogError("Error uploading user data.");
                }
            });
        }
    }

    //유저 정보- Battle List 를 Database에 저장.
    void Update_UserBattleInfo()
    {
        string battleInfoTable_path = "UserBattleInfoSeat/" + userVO.UID;

        ///firebase에는 null 값으로 data를 저장할 수 없음. 따라서 임시 0번째의 값을 빈 값으로 객체를 직렬화하여 seat를 초기화함.
        Wrapper info = new Wrapper();
        info.BattleDatas = userVO.BattleInfos.ToList();

        for (int i = 0; i < info.BattleDatas.Count;)
        {
            if (info.BattleDatas[i].date == string.Empty)
            {
                info.BattleDatas.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }


        string json = JsonConvert.SerializeObject(info);

        token.Child(battleInfoTable_path).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("User data uploaded successfully.");
            }
            else
            {
                Debug.LogError("Error uploading user data.");
            }
        });
    }





//--------------------------임시 test용 기능
public void addCannonnKeys(string value)
    {
        userVO.CannonInfos.CannonKeys.Add(value);
    }

    public void addBattleInnfo(string value)
    {
        userVO.BattleInfos.Add(new UserBattleInfo() { date = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), result = value });
    }

    public void UpdateCannonList()
    {
        Update_UserCannon();
    }

    public void UpdateBattleInfo()
    {
        Update_UserBattleInfo();
    }


}


// UserBattleInfo 배열을 래핑할 클래스
[System.Serializable]
public class Wrapper
{
    public List<UserBattleInfo> BattleDatas;
}
