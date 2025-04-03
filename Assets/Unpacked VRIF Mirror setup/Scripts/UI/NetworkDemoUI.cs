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
            ShowDisconnectButton();
        }

        public override void OnStartClient()
        {
            Debug.Log("OnStartClient");
            ClientConnected = true;
            DisplayText.text += "Client started.\n";
            ShowDisconnectButton();
        }

        public async void OnHostButton()
        {
            Debug.Log("Host Button clicked");
            try
            {
                await EnsureAuthentication();
                if (!isAuthenticated)
                {
                    Debug.Log("Authentication failed");
                    return;
                }

                Debug.Log("Authentication successful");

                networkManager.StartRelayHost(4);

                // SaveLocalPlayersData();
                ShowDisconnectButton();
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

            networkManager.JoinRelayServer(joinCode);

            // SaveLocalPlayersData();
            ShowDisconnectButton();
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

            HideDisconnectButton();
        }

        public void OnOfflineButton()
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(1));
            sceneLoader.LoadScene(sceneName);
        }

        public override void OnStopClient()
        {
            Debug.Log("OnStopClient");
            DisplayText.text += "Client disconnected.\n";
            ClientConnected = false;
            HideDisconnectButton();
        }

        public void ShowDisconnectButton()
        {
            if (DisconnectButton) DisconnectButton.SetActive(true);
            if (ConnectButton) ConnectButton.SetActive(false);
            if (HostButton) HostButton.SetActive(false);

            foreach (GameObject go in DisableGameObjects)
            {
                go.SetActive(false);
            }
        }

        public void HideDisconnectButton()
        {
            if (DisconnectButton) DisconnectButton.SetActive(false);
            if (ConnectButton) ConnectButton.SetActive(true);
            if (HostButton) HostButton.SetActive(true);
        }

        public void SaveLocalPlayersData()
        {
            SaveLoadData saveLoadData = SaveLoadData.Instance;
            LocalPlayerData localPlayerData = LocalPlayerData.Instance;
            localPlayerData.SetPlayerName(PlayerNameInput.text);
            saveLoadData.SavePlayerPref("PlayerName", localPlayerData.PlayerName);
            saveLoadData.SavePlayerPref("PrefabIndex", localPlayerData.playerPrefabIndex);
            saveLoadData.SavePlayerPref("BodyTextureIndex", localPlayerData.bodyTextureIndex);
            saveLoadData.SavePlayerPref("PropTextureIndex", localPlayerData.propTextureIndex);
        }
    }
}
