using UnityEngine;
using System.Collections.Generic;

public class ShelfManager : MonoBehaviour
{
    private Dictionary<IndividualShelfManager, string> shelfProducts = new Dictionary<IndividualShelfManager, string>();

    public void RegisterShelf(IndividualShelfManager shelf)
    {
        if (!shelfProducts.ContainsKey(shelf))
        {
            shelfProducts[shelf] = "Empty";
        }
    }
    
    public void UpdateShelfProduct(IndividualShelfManager shelf, string productType)
    {
        if (shelfProducts.ContainsKey(shelf))
        {
            shelfProducts[shelf] = productType;
        }
        else
        {
            shelfProducts.Add(shelf, productType);
        }

        //Debug.Log($"Shelf {shelf.name} now contains {productType}");
        //PrintShelfContents();
    }
    
    public string GetProductTypeOnShelf(IndividualShelfManager shelf)
    {
        return shelfProducts.ContainsKey(shelf) ? shelfProducts[shelf] : "Empty";
    }

    public void PrintShelfContents()
    {
        Debug.Log("Shelf Contents:");
        foreach (var shelf in shelfProducts)
        {
            Debug.Log($"{shelf.Key.name}: {shelf.Value}");
        }
    }
    
    public void RemoveProductFromShelf(GameObject product, IndividualShelfManager shelf)
    {
        if (shelf == null || product == null) return;

        shelf.RemoveProduct(product);

        if (!shelf.HasAnyProducts())
        {
            shelfProducts[shelf] = "Empty";
        }

        Debug.Log($"Product removed from shelf: {shelf.name}");
    }
}
