using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class DisplayManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private RectTransform textContainer;
    [SerializeField] private ScrollRect scrollRect;
    private List<ProductData> scannedProducts = new List<ProductData>();

    private const int MaxDisplayedProducts = 5;

    public void UpdateDisplay(List<ProductData> products)
    {
        scannedProducts = products;
        float totalAmount = 0f;
        string productList = "";

        // Calculate total amount
        foreach (ProductData product in scannedProducts)
        {
            totalAmount += product.price;
        }

        // Get the last few items only (if list is longer than MaxDisplayedProducts)
        int startIdx = Mathf.Max(0, scannedProducts.Count - MaxDisplayedProducts);
        for (int i = startIdx; i < scannedProducts.Count; i++)
        {
            productList += $"{scannedProducts[i].productName} - €{scannedProducts[i].price:F2}\n";
        }

        // Display total first, then the last few products
        displayText.text = $"Total: €{totalAmount:F2}\n\n{productList}";

        AdjustScrollView();
    }

    public void PaymentDisplay(bool success)
    {
        CreditCard creditCard = GameObject.FindWithTag("CreditCard")?.GetComponent<CreditCard>();

        if (creditCard == null)
        {
            displayText.text = "<color=red>Error: No credit card detected!</color>";
            return;
        }

        if (success)
        {
            displayText.text = $"<color=green>Payment successful!</color>\nNew balance: €{creditCard.balance:F2}";
            StartCoroutine(ResetDisplay());
        }
        else
        {
            displayText.text = $"<color=red>Insufficient balance!</color>\nCurrent balance: €{creditCard.balance:F2}";
            StartCoroutine(ResetDisplay());
        }
    }

    private IEnumerator ResetDisplay()
    {
        yield return new WaitForSeconds(3f);
        displayText.text = "Scan ";
    }

    private void AdjustScrollView()
    {
        if (textContainer != null)
        {
            float textHeight = displayText.preferredHeight;
            textContainer.sizeDelta = new Vector2(textContainer.sizeDelta.x, textHeight);
        }
    }
}