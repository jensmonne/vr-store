using TMPro;
using UnityEngine;

public class CrateSpawner : MonoBehaviour
{
    [SerializeField] private GameObject cratePrefab;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int cost;
    
    private void Start()
    {
        if (priceText != null)
        {
            priceText.text = $"€ {cost.ToString()}";
        }
        else
        {
            Debug.LogError("PriceText is not assigned.");
        }

        if (cratePrefab == null)
        {
            Debug.LogError("Crate prefab is not assigned.");
        }
    }

    public void OnButtonPress()
    {
        if (!GameManager.CheckMoney(cost)) return;
        GameManager.RemoveMoney(cost);
        
        GameObject crate = Instantiate(cratePrefab, spawnPoint.position, Quaternion.identity);
        Debug.Log("Crate spawned at: " + spawnPoint.position);
    }
}
