using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Utp;
using Task = System.Threading.Tasks.Task;

public class ConnectUI : MonoBehaviour
{
    [SerializeField] private RelayNetworkManager networkManager;
    private bool isAuthenticated = false;
    private bool isAuthenticating = false;
    
    private void Start()
    {
        if (networkManager == null)
        {
            networkManager = FindObjectOfType<RelayNetworkManager>();
        }
    }

    private async Task EnsureAuthentication()
    {
        if (isAuthenticated || isAuthenticating) return;
        
        isAuthenticating = true;
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Logged into Unity, player ID is: " + AuthenticationService.Instance.PlayerId);

            isAuthenticated = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to authenticate: {e.Message}");
        }
        finally
        {
            isAuthenticating = false;
        }
    }

    public async void Host()
    {
        await EnsureAuthentication();
        if (!isAuthenticated)
        {
            Debug.LogError("Authentication failed. Cannot host.");
            return;
        }
        
        networkManager.StartRelayHost(2);
    }
    
    public async void Client()
    {
        await EnsureAuthentication();
        if (!isAuthenticated)
        {
            Debug.LogError("Authentication failed. Cannot connect.");
            return;
        }
        
        //networkManager.JoinRelayServer();
    }
}
