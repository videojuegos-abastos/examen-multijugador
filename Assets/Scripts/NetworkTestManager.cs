using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class NetworkTestManager : MonoBehaviour
{
    [SerializeField] GameObject meteorSpawner;

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    void Start()
    {
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
    }

    void OnServerStarted()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            var instance = Instantiate(meteorSpawner);
            instance.GetComponent<NetworkObject>().Spawn();
        }
    }
}
