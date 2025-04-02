using System;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;
using Utp;

namespace BNG {
    public class NetworkDemoUI : NetworkBehaviour {

        [SerializeField] private RelayNetworkManager networkManager;

        public InputField PlayerNameInput;
        public InputField RoomCodeInput;

        public GameObject ConnectButton;
        public GameObject HostButton;
        public GameObject DisconnectButton;
        public TMP_Text DisplayText;
        public List<GameObject> DisableGameObjects;

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

        public override void OnStartServer() {
            Debug.Log("OnStartServer");
            DisplayText.text = "Server started.\n";
            ShowDisconnectButton();
        }

        public override void OnStartClient() {
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

        public async void OnConnectButton() {
            await EnsureAuthentication();
            if (!isAuthenticated) return;
            
            DisplayText.text = "Connecting to Relay...\n";
            
            string joinCode = RoomCodeInput.text;

            networkManager.JoinRelayServer(joinCode);

            // SaveLocalPlayersData();
            ShowDisconnectButton();
        }

        public void OnDisconnectButton() {
            if (ClientConnected) {
                networkManager.StopClient();
            } else {
                networkManager.StopHost();
            }

            if (isServer) {
                networkManager.StopHost();
            }

            HideDisconnectButton();
        }

        public override void OnStopClient() {
            Debug.Log("OnStopClient");
            DisplayText.text += "Client disconnected.\n";
            ClientConnected = false;
            HideDisconnectButton();
        }

        public void ShowDisconnectButton() {
            if (DisconnectButton) DisconnectButton.SetActive(true);
            if (ConnectButton) ConnectButton.SetActive(false);
            if (HostButton) HostButton.SetActive(false);

            foreach (GameObject go in DisableGameObjects) {
                go.SetActive(false);
            }
        }

        public void HideDisconnectButton() {
            if (DisconnectButton) DisconnectButton.SetActive(false);
            if (ConnectButton) ConnectButton.SetActive(true);
            if (HostButton) HostButton.SetActive(true);
        }

        public void SaveLocalPlayersData() {
            SaveLoadData saveLoadData = SaveLoadData.Instance;
            LocalPlayerData localPlayerData = LocalPlayerData.Instance;
            localPlayerData.SetPlayerName(PlayerNameInput.text);
            saveLoadData.SavePlayerPref("PlayerName", localPlayerData.PlayerName);
            saveLoadData.SavePlayerPref("PrefabIndex", localPlayerData.playerPrefabIndex);
            saveLoadData.SavePlayerPref("BodyTextureIndex", localPlayerData.bodyTextureIndex);
            saveLoadData.SavePlayerPref("PropTextureIndex", localPlayerData.propTextureIndex);
        }

        // ---------------- RELAY INTEGRATION ----------------

        private async Task<string> CreateRelay() {
            try {
                Debug.Log("Creating Relay allocation...");
                
                if (!UnityServices.State.Equals(ServicesInitializationState.Initialized)) {
                    Debug.LogWarning("Unity Services not initialized! Initializing now...");
                    await UnityServices.InitializeAsync();
                }
                
                await EnsureAuthentication();
                if (!isAuthenticated) {
                    Debug.LogError("Authentication failed. Cannot create Relay.");
                    return null;
                }
                
                if (RelayService.Instance == null)
                {
                    Debug.LogError("RelayService.Instance is null. Make sure Unity Relay is correctly set up in the dashboard.");
                    return null;
                }
                else
                {
                    Debug.Log("AHHHH");
                }
                

                var relayService = RelayService.Instance;
                if (relayService == null) {
                    Debug.LogError("RelayService.Instance is null. Make sure Unity Relay is correctly set up in the dashboard.");
                    return null;
                }
                

                Allocation allocation = await relayService.CreateAllocationAsync(4);
                if (allocation == null) {
                    Debug.LogError("Failed to create Relay allocation.");
                    return null;
                }
                else {
                    Debug.Log($"Relay allocation created: {allocation.AllocationId}");
                }
                
                string joinCode = await relayService.GetJoinCodeAsync(allocation.AllocationId);

                if (string.IsNullOrEmpty(joinCode)) {
                    Debug.LogError("Failed to retrieve Join Code.");
                    return null;
                }
                else {
                    Debug.Log($"Join Code: {joinCode}");
                }
                
                if (networkManager == null) {
                    Debug.LogError("❌ NetworkManager.singleton is NULL! Make sure there is a NetworkManager in the scene.");
                    return null;
                } else {
                    Debug.Log("✅ NetworkManager found!");
                }

                networkManager.StartStandardHost();

                return joinCode;
            } catch (System.Exception e) {
                Debug.LogError($"Relay creation failed: {e.Message}");
                return null;
            }
        }

        private async Task JoinRelay(string joinCode) {
            try {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                networkManager.JoinStandardServer();
                Debug.Log("Connected to relay server!");
            } catch (System.Exception e) {
                Debug.LogError($"Relay join failed: {e.Message}");
            }
        }
    }
}
