using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DemoSpawnManager : MonoBehaviour
{
    public NetworkObject playerPrefab;

    public static DemoSpawnManager Instance { get; private set; }

    NetworkObject _player;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // ���� ī�޶� ��Ȱ��ȭ
        Camera.main.gameObject.SetActive(false);

        // ������ Ŭ���̾�Ʈ �÷��̾� ��ȯ
        NetworkManager singleton = NetworkManager.Singleton;
        singleton.OnClientConnectedCallback += (clientId) =>
        {
            SpawnPlayerRpc(clientId);
        };

        //ȣ��Ʈ �÷��̾� ��ȯ
        if (NetworkManager.Singleton.IsServer)
        {
            SpawnPlayerRpc(singleton.ConnectedClientsIds[0]);
        }
    }

    [Rpc(SendTo.Server)]
    void SpawnPlayerRpc(ulong clientId)
    {
        // �÷��̾� ��ȯ
        _player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        _player.SpawnAsPlayerObject(clientId);
        Debug.Log($"Player spawned: {clientId}");
    }
}
