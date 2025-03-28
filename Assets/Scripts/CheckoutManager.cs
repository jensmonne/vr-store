using System.Collections.Generic;
using UnityEngine;

public class CheckoutManager : MonoBehaviour
{
    public List<ProductData> scannedProducts;
    
    public DisplayManager displayManager;

    private void OnTriggerEnter(Collider other)
    {
        ProductHolder product = other.GetComponent<ProductHolder>();
        if (product != null)
        {
            scannedProducts.Add(product.productData);
            Debug.Log($"Added {product.productData.name}");
            displayManager.UpdateDisplay(scannedProducts);
        }
    }
    
    public void TryPayment(CreditCard creditCard)
    {
        float total = BerekenTotaalBedrag();
        if (creditCard.balance >= total)
        {
            creditCard.balance -= total;
            scannedProducts.Clear();
            displayManager.PaymentDisplay(true);
        }
        else
        {
            displayManager.PaymentDisplay(false);
        }
    }
    
    public float BerekenTotaalBedrag()
    {
        float totaal = 0;
        foreach (ProductData product in scannedProducts)
        {
            totaal += product.price;
        }
        return totaal;
    }
}