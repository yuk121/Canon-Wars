using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum EGameMode
{
    Mode1vs1,
    Mode2vs2
}

public class LobbyManager : MonoBehaviour
{
    [Header("�޴�")]
    public GameObject startPanel;
    public GameObject infoPanel;
    public GameObject optionPanel;
    public Button startButton;
    public Button infoButton;
    public Button optionButton;
    public Button exitButton;

    [Header("�κ�")]
    public StartLobbyUI startLobbyUI;
    public CreateLobbyUI createLobbyUI;
    public SortLobbyUI sortLobbyUI;
    public MainLobbyUI mainLobbyUI;
    public GameObject loadingUI;
    public string gameSceneName;

    public static LobbyManager Instance { get; private set; }

    public List<LobbyData> PublicLobbyDatas { get; private set; } = new();
    public List<PlayerData> LobbyPlayerDatas { get; private set; } = new();

    public bool IsLobbyHost => _joinedLobby.HostId == AuthenticationService.Instance.PlayerId;

    Lobby _joinedLobby;

    string _playerName;
    string _playerNameDataKey = "PlayerName";
    string _playerReadyDataKey = "PlayerReady";
    string _gameModeDataKey = "GameMode";
    string _gameStartDataKey = "GameStart";
    string _lobbyNotFoundError = "lobby not found";
    float _maintainLobbyTime = 20;
    float _checkLobbyTime = 1.1f;
    int _maxPlayers = 4;

    void Awake()
    {
        Instance = this;
    }

    async void Start()
    {
        _joinedLobby = null;

        // �͸����� ����Ƽ �α���
        try
        {
            await UnityServices.InitializeAsync();
            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log($"Signed in: {AuthenticationService.Instance.PlayerId}.");
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError(ex.Message);
        }

        // �÷��̾� ���̵� �ο�
        if (FirebaseManager._instance != null)
        {
            _playerName = FirebaseManager._instance.userVO.UserID;
        }
        else
        {
            _playerName = $"player {Random.Range(0, 100)}";
        }

        // �����ϱ�
        startButton.onClick.AddListener(() =>
        {
            startPanel.SetActive(true);
            infoPanel.SetActive(false);
            optionPanel.SetActive(false);
        });

        // �� ����
        infoButton.onClick.AddListener(() =>
        {
            startPanel.SetActive(false);
            infoPanel.SetActive(true);
            optionPanel.SetActive(false);
        });

        // �ɼ�
        optionButton.onClick.AddListener(() =>
        {
            startPanel.SetActive(false);
            infoPanel.SetActive(false);
            optionPanel.SetActive(true);
        });

        // ����
        exitButton.onClick.AddListener(() =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });
    }

    public async void CreateLobby(EGameMode gameMode, string lobbyName, bool isPrivate)
    {
        try
        {
            // �κ� ����
            loadingUI.SetActive(true);
            _joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, _maxPlayers, new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Player = GetPlayer(true),
                Data = new Dictionary<string, DataObject>
                {
                    // ���� ���
                    { _gameModeDataKey, new DataObject(DataObject.VisibilityOptions.Public, gameMode.ToString(), DataObject.IndexOptions.S1) },

                    // �κ� ������Ը� ���� ���� ���� ����
                    { _gameStartDataKey, new DataObject(DataObject.VisibilityOptions.Member, "0") }
                }
            });

            mainLobbyUI.EnterMainLobbyUI(lobbyName, _joinedLobby.LobbyCode);
            ReadyPlayer(true);
            InvokeRepeating(nameof(MaintainLobby), _maintainLobbyTime, _maintainLobbyTime);
            InvokeRepeating(nameof(RefreshPlayers), _checkLobbyTime, _checkLobbyTime);
            Debug.Log($"Created Lobby: {lobbyName}, Code: {_joinedLobby.LobbyCode}.");
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public async void RefreshPublicLobbies(EGameMode gameMode)
    {
        try
        {
            // ã�� �κ� ǥ��
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                {
                    // �� �ڸ��� 0 ���� ū �κ� ǥ��
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),

                    // ������ ���� ��常 ǥ��
                    new QueryFilter(QueryFilter.FieldOptions.S1, gameMode.ToString(), QueryFilter.OpOptions.EQ)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Name)
                },
            });

            PublicLobbyDatas.Clear();
            for (int i = 0; i < queryResponse.Results.Count; i++)
            {
                // �κ� ���� ����
                Lobby lobby = queryResponse.Results[i];
                PublicLobbyDatas.Add(new LobbyData
                {
                    id = lobby.Id,
                    name = lobby.Name,
                    gameMode = (EGameMode)System.Enum.Parse(typeof(EGameMode), lobby.Data[_gameModeDataKey].Value),
                });

                Debug.Log($"Lobbies found:{lobby.Name}, Mode: {lobby.Data[_gameModeDataKey].Value}.");
            }

            sortLobbyUI.RefreshLobbiesUI();
        }
        catch(LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public async void JoinLobby(string lobbyCode, int index)
    {
        try
        {
            loadingUI.SetActive(true);
            if (lobbyCode == null)
            {
                // ���� �κ� ����
                _joinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(PublicLobbyDatas[index].id, new JoinLobbyByIdOptions
                {
                    Player = GetPlayer(false),
                });
            }
            else
            {
                // ����� �κ� ����
                _joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, new JoinLobbyByCodeOptions
                {
                    Player = GetPlayer(false),
                });
            }

            PublicLobbyDatas.Clear();
            mainLobbyUI.EnterMainLobbyUI(_joinedLobby.Name, _joinedLobby.LobbyCode);
            InvokeRepeating(nameof(RefreshPlayers), _checkLobbyTime, _checkLobbyTime);
            Debug.Log($"Joined lobby: {_joinedLobby.Name}.");
        }
        catch (LobbyServiceException ex)
        {
            if (ex.Message.Equals(_lobbyNotFoundError))
            {
                Debug.Log(ex.Message);
            }
            else
            {
                Debug.LogError(ex.Message);
            }

            sortLobbyUI.ShowJoinFailedUI();
            _joinedLobby = null;
        }
    }

    public async void LeaveLobby()
    {
        loadingUI.SetActive(true);  
        if (IsLobbyHost)
        {
            // ȣ��Ʈ�� ���� ��� �κ� ����
            DeleteLobby();
            Debug.Log("Host left the lobby.");
        }
        else
        {
            // �ƴϸ� �κ񿡼� ���� ����
            try
            {
                CancelInvoke(nameof(RefreshPlayers));
                await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                Debug.Log($"{_playerName} left the lobby.");
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        _joinedLobby = null;
        loadingUI.SetActive(false);
        mainLobbyUI.LeaveMainLobbyUI();
        CancelInvoke(nameof(MaintainLobby));
    }

    public async void MaintainLobby()
    {
        // 30�ʰ� ������ �κ� ������Ƿ� ���������� ����
        if (_joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(_joinedLobby.Id);
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogError(ex.Message);
            }
        }
    }

    public async void KickPlayer(int index)
    {
        // �÷��̾� �߹�
        try
        {
            Unity.Services.Lobbies.Models.Player player = _joinedLobby.Players[index];
            if (player.Id != _joinedLobby.HostId)
            {
                loadingUI.SetActive(true);
                await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, player.Id);
                Debug.Log($"{player.Data[_playerNameDataKey].Value} kicked.");
            }
            else
            {
                // ȣ��Ʈ�� �߹� �Ұ�
                Debug.Log($"{player.Data[_playerNameDataKey].Value} is host and can't be kicked.");
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public async void ReadyPlayer(bool value)
    {
        // �غ�
        try
        {
            _joinedLobby = await LobbyService.Instance.UpdatePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    // �κ� ������Ը� �غ� ���� ����
                    { _playerReadyDataKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, value ? "1" : "0") }
                }
            });

            Debug.Log($"Ready: {value}");
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public async void StartGameAsHost()
    {
        // ȣ��Ʈ�� ���� ����
        try
        {
            loadingUI.SetActive(true);
            string relayCode = await CreateRelay();
            _joinedLobby = await Lobbies.Instance.UpdateLobbyAsync(_joinedLobby.Id, new UpdateLobbyOptions
            {
                // ���� ���� �˸�
                Data = new Dictionary<string, DataObject>
                {
                    // �κ� ������Ը� ����
                    { _gameStartDataKey, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                }
            });

            CancelInvoke(nameof(MaintainLobby));
            CancelInvoke(nameof(RefreshPlayers));
            Invoke(nameof(LoadGameScene), _checkLobbyTime);
            _joinedLobby = null;
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public bool IsPlayer(int index)
    {
        // �ش� �÷��̾ �������� Ȯ��
        return _joinedLobby.Players[index].Id == AuthenticationService.Instance.PlayerId;
    }

    async void RefreshPlayers()
    {
        // �κ� ����
        try
        {
            _joinedLobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
        }
        catch (LobbyServiceException ex)
        {
            if (ex.Message.Equals(_lobbyNotFoundError))
            {
                // �κ� ���� ���
                CancelInvoke(nameof(RefreshPlayers));
                mainLobbyUI.ShowSessionEndedUI();
                _joinedLobby = null;
                Debug.Log(ex.Message);
                return;
            }
            else
            {
                Debug.LogError(ex.Message);
            }
        }

        // �߹�� ���
        if (_joinedLobby.Players[0].Data == null)
        {
            CancelInvoke(nameof(RefreshPlayers));
            mainLobbyUI.ShowKickedUI();
            _joinedLobby = null;
            Debug.Log("You are kicked from the lobby.");
            return;
        }

        // �÷��̾� ������ ����
        LobbyPlayerDatas.Clear();
        int playerIndex = 0;
        bool gameReady = true;
        for (int i = 0; i < _joinedLobby.Players.Count; i++)
        {
            // �̸��� �غ� ���� ����
            bool playerReady = _joinedLobby.Players[i].Data[_playerReadyDataKey].Value.Equals("1");
            LobbyPlayerDatas.Add(new PlayerData
            {
                name = _joinedLobby.Players[i].Data[_playerNameDataKey].Value,
                ready = playerReady
            });

            // ��� �÷��̾ �غ�Ǿ����� Ȯ��
            if (!playerReady || _joinedLobby.Players.Count < 2)
            {
                gameReady = false;
            }

            if (IsPlayer(i))
            {
                playerIndex = i;
            }
        }

        // ȣ��Ʈ�� �ƴ� ���
        if (!IsLobbyHost)
        {
            //������ ���۵Ǿ����� Ȯ��
            string joinCode = _joinedLobby.Data[_gameStartDataKey].Value;
            if (!joinCode.Equals("0"))
            {
                StartGameAsClient(joinCode);
            }
        }

        mainLobbyUI.RefreshPlayersUI(playerIndex, gameReady);
        loadingUI.SetActive(false);
    }

    async void DeleteLobby()
    {
        // �κ� ����
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(_joinedLobby.Id);
            Debug.Log("Lobby deleted.");
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    async Task<string> CreateRelay()
    {
        // ������ ����
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(_maxPlayers - 1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            RelayServerData relayServerData = new(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();
            Debug.Log("Game started as host.");
            return joinCode;
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex.Message);
            return null;
        }
    }

    async void JoinRelay(string joinCode)
    {
        // ������ ����
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = new(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
            Debug.Log("Game started as client.");
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    void StartGameAsClient(string joinCode)
    {
        // Ŭ���̾�Ʈ�� ���� ����
        loadingUI.SetActive(true);
        JoinRelay(joinCode);
        CancelInvoke(nameof(RefreshPlayers));
        _joinedLobby = null;
    }

    void LoadGameScene()
    {
        // ���� �� �ε�
        NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }

    Unity.Services.Lobbies.Models.Player GetPlayer(bool isHost)
    {
        return new Unity.Services.Lobbies.Models.Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                // �κ� ������Ը� �÷��̾� �̸� ����
                { _playerNameDataKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName) },

                // �κ� ������Ը� �غ� ���� ����
                { _playerReadyDataKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, isHost? "1" : "0") }
            },
        };
    }

    public class LobbyData
    {
        public string id;
        public string name;
        public EGameMode gameMode;
    }

    public class PlayerData
    {
        public string name;
        public bool ready;
    }
}
