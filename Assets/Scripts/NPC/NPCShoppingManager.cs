using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NPCShoppingManager : MonoBehaviour
{
    public static NPCShoppingManager Instance { get; private set; }
    
    [SerializeField] private List<ProductData> availableProducts = new List<ProductData>();
    private List<Transform> shelfLocations = new List<Transform>();
    private List<Transform> checkoutPoints = new List<Transform>();
    private Dictionary<Transform, int> checkoutQueue = new Dictionary<Transform, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }
    
    private void Start()
    {
        FindShoppingLocations();
    }

    private void FindShoppingLocations()
    {
        shelfLocations = GameObject.FindGameObjectsWithTag("NPCShelfPoint").Select(obj => obj.transform).ToList();
        checkoutPoints = GameObject.FindGameObjectsWithTag("NPCQueuePoint").Select(obj => obj.transform).ToList();

        foreach (var checkout in checkoutPoints)
        {
            if (!checkoutQueue.ContainsKey(checkout)) checkoutQueue[checkout] = 0;
        }
    }
    
    public Transform GetNextShelf(int index)
    {
        return (index < shelfLocations.Count) ? shelfLocations[index] : null;
    }

    public Transform GetLeastBusyCheckout()
    {
        if (checkoutPoints.Count == 0) return null;

        return checkoutPoints.OrderBy(c => checkoutQueue.ContainsKey(c) ? checkoutQueue[c] : 0).FirstOrDefault();
    }

    public void JoinCheckoutQueue(Transform checkout)
    {
        if (!checkoutQueue.ContainsKey(checkout))
            checkoutQueue[checkout] = 0;

        checkoutQueue[checkout]++;
    }

    public void LeaveCheckoutQueue(Transform checkout)
    {
        if (checkoutQueue.ContainsKey(checkout) && checkoutQueue[checkout] > 0)
            checkoutQueue[checkout]--;
    }

    public List<ProductData> GetRandomShoppingList()
    {
        List<ProductData> ShoppingList = new List<ProductData>();
        int TotalProducts = Random.Range(1, availableProducts.Count);
        
        List<ProductData> shuffledProducts = new List<ProductData>(availableProducts);
        shuffledProducts.Shuffle();

        for (int i = 0; i < TotalProducts; i++)
        {
            ProductData selectedProduct = shuffledProducts[i];
            int quantity = Random.Range(1, selectedProduct.maxQuantity + 1);
            
            for (int j = 0; j < quantity; j++)
            {
                ShoppingList.Add(selectedProduct);
            }
        }
        return ShoppingList;
    }
}

public static class ListExtensions
{
    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}