using Mirror;
using TMPro;
using UnityEngine;
using Utp;

public class RoomOptions : MonoBehaviour
{
    [SerializeField] private TMP_Text roomCodeText;
    private RelayNetworkManager networkManager;
    
    private void Start()
    {
        networkManager = FindObjectOfType<RelayNetworkManager>();
        if (networkManager == null)
        {
            Debug.LogError("RelayNetworkManager not found in the scene.");
            return;
        }
        
        string RoomCode = PlayerPrefs.GetString("RoomCode", "");
        if (string.IsNullOrEmpty(RoomCode))
        {
            Debug.LogError("Room code is not set in PlayerPrefs.");
            return;
        }
        
        roomCodeText.text = $"Room Code:\n {RoomCode}\n\n Press ≡ to Disable Menu";
    }

    public void OnDisconnectButton()
    {
        if (NetworkServer.active)
        {
            networkManager.StopHost();
        }
        else if (NetworkClient.active)
        {
            networkManager.StopClient();
        }
        else 
        {
            Debug.LogWarning("Neither server nor client is active.");
        }
    }
}
