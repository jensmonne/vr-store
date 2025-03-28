using UnityEngine;

public class Payment : MonoBehaviour
{
    public CheckoutManager checkoutManager;
    
    private void OnTriggerEnter(Collider other)
    {
        CreditCard creditCard = other.GetComponent<CreditCard>();
        if (creditCard != null)
        {
            checkoutManager.TryPayment(creditCard);
        }
    }
}
