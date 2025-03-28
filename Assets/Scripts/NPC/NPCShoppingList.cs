using UnityEngine;
using System.Collections.Generic;

public class NPCShoppingList : MonoBehaviour
{
    private List<ProductData> shoppingList;

    private void Start()
    {
        if (NPCShoppingManager.Instance == null)
        {
            Debug.LogError("ShoppingManager instance is missing!");
            return;
        }

        shoppingList = NPCShoppingManager.Instance.GetRandomShoppingList();
        Debug.Log("NPC Shopping List: " + string.Join(", ", shoppingList.ConvertAll(p => p.productName)));
    }

    public List<ProductData> GetShoppingList()
    {
        return shoppingList;
    }
    
    public bool NeedsProduct(ProductData product)
    {
        return shoppingList.Contains(product);
    }
}