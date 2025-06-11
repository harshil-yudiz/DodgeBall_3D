using UnityEngine;
using Mirror;
using Mirror.Discovery;
using Telepathy;

public class NetworkManagerController : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private NetworkDiscovery networkDiscovery;
    [SerializeField] private Transport transport;
    private bool isSearching = false;

    private void Awake()
    {
        // Ensure we have references
        if (networkManager == null)
            networkManager = GetComponent<NetworkManager>();

        if (networkDiscovery == null)
            networkDiscovery = GetComponent<NetworkDiscovery>();

        // Add transport if missing
        if (networkManager != null && networkManager.transport == null)
        {
            // First try to find existing transport
            transport = GetComponent<Transport>();
            
            // If no transport exists, add Telepathy
            if (transport == null)
            {
                transport = gameObject.AddComponent<TelepathyTransport>();
                Debug.Log("Added TelepathyTransport to NetworkManager");
            }
            
            networkManager.transport = transport;
        }
    }

    private void Start()
    {
        // Additional validation
        if (networkManager == null || networkManager.transport == null)
        {
            Debug.LogError("NetworkManager or Transport not properly initialized!");
            enabled = false;
            return;
        }
    }

    public void OnPlayButtonClick()
    {
        if (NetworkClient.active || NetworkServer.active)
            return;

        StartSearchingForServer();
    }

    private void StartSearchingForServer()
    {
        isSearching = true;
        networkDiscovery.OnServerFound.AddListener(OnServerFound);
        networkDiscovery.StartDiscovery();
        
        // Set a timeout to host if no server is found
        Invoke(nameof(HostIfNoServer), 1f);
    }

    private void OnServerFound(ServerResponse info)
    {
        if (!isSearching) return;
        
        // Cancel the timeout since we found a server
        CancelInvoke(nameof(HostIfNoServer));
        isSearching = false;
        
        // Connect to the found server
        networkManager.networkAddress = info.uri.ToString();
        networkManager.StartClient();
    }

    private void HostIfNoServer()
    {
        if (!isSearching) return;
        
        isSearching = false;
        networkDiscovery.OnServerFound.RemoveListener(OnServerFound);
        
        // Start hosting
        networkManager.StartHost();
        networkDiscovery.AdvertiseServer();
    }

    private void OnDisable()
    {
        if (networkDiscovery != null)
            networkDiscovery.OnServerFound.RemoveListener(OnServerFound);
    }
}
