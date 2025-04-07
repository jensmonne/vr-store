using System;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Utp;

namespace BNG {
    public class NetworkDemoUI : NetworkBehaviour
    {

        [SerializeField] private RelayNetworkManager networkManager;

        [SerializeField] private InputField PlayerNameInput;
        [SerializeField] private InputField RoomCodeInput;

        [SerializeField] private GameObject ConnectButton;
        [SerializeField] private GameObject HostButton;
        [SerializeField] private GameObject DisconnectButton;
        [SerializeField] private TMP_Text DisplayText;
        [SerializeField] private List<GameObject> DisableGameObjects;
        
        [SerializeField] private SceneLoader sceneLoader;

        private bool ClientConnected;
        private string relayJoinCode;
        private bool isAuthenticated = false;
        private bool isAuthenticating = false;
        
        private async void Start()
        {
            await EnsureAuthentication();
        }

        private async Task EnsureAuthentication()
        {
            if (isAuthenticated || isAuthenticating) return;

            isAuthenticating = true;
            try
            {
                if (!UnityServices.State.Equals(ServicesInitializationState.Initialized))
                {
                    await UnityServices.InitializeAsync();
                    Debug.Log("Unity Services Initialized");
                }

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    Debug.Log("Signed into Unity Services! Player ID: " + AuthenticationService.Instance.PlayerId);
                }

                isAuthenticated = true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Unity Services authentication failed: {e.Message}");
            }
            finally
            {
                isAuthenticating = false;
            }
        }

        public override void OnStartServer()
        {
            Debug.Log("OnStartServer");
            DisplayText.text = "Server started.\n";
            HideConnectionUI();
        }

        public override void OnStartClient()
        {
            Debug.Log("OnStartClient");
            ClientConnected = true;
            DisplayText.text += "Client started.\n";
            HideConnectionUI();
        }

        public async void OnHostButton()
        {
            Debug.Log("Host Button clicked");
            try
            {
                DisplayText.text = "Attempting to host server... \n";
                
                string playerName = PlayerNameInput.text;
                if (string.IsNullOrEmpty(playerName))
                {
                    DisplayText.text = "Please enter your player name.\n";
                    return;
                }
                
                PlayerPrefs.SetString("PlayerName", playerName);
                
                await EnsureAuthentication();
                if (!isAuthenticated)
                {
                    Debug.Log("Authentication failed");
                    return;
                }

                Debug.Log("Authentication successful");

                HideConnectionUI();
                
                networkManager.StartRelayHost(4);
            }
            catch (Exception e)
            {
                Debug.LogError($"Host creation failed: {e.Message}");
            }
        }

        public async void OnConnectButton()
        {
            await EnsureAuthentication();
            if (!isAuthenticated) return;

            DisplayText.text = "Connecting to Relay...\n";

            string joinCode = RoomCodeInput.text.Trim();
            
            if (string.IsNullOrEmpty(joinCode) || joinCode.Length != 6)
            {
                DisplayText.text = "Please enter a valid join code.\n";
                return;
            }
            
            PlayerPrefs.SetString("RoomCode", joinCode);
            
            string playerName = PlayerNameInput.text;
            
            if (string.IsNullOrEmpty(playerName))
            {
                DisplayText.text = "Please enter your player name.\n";
                return;
            }
            
            PlayerPrefs.SetString("PlayerName", playerName);
            
            HideConnectionUI();

            networkManager.JoinRelayServer(joinCode);
        }

        public void OnDisconnectButton()
        {
            if (ClientConnected)
            {
                networkManager.StopClient();
            }
            else
            {
                networkManager.StopHost();
            }

            if (isServer)
            {
                networkManager.StopHost();
            }

            ShowConnectionUI();
        }

        public void OnOfflineButton()
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(2));
            sceneLoader.LoadScene(sceneName);
        }

        public override void OnStopClient()
        {
            Debug.Log("OnStopClient");
            DisplayText.text += "Client disconnected.\n";
            ClientConnected = false;
            HideConnectionUI();
        }

        private void ShowConnectionUI()
        {
            if (DisconnectButton) DisconnectButton.SetActive(false);
            foreach (GameObject go in DisableGameObjects)
            {
                go.SetActive(false);
            }
        }

        private void HideConnectionUI()
        {
            if (DisconnectButton) DisconnectButton.SetActive(true);
            foreach (GameObject go in DisableGameObjects)
            {
                go.SetActive(true);
            }
        }
    }
}
