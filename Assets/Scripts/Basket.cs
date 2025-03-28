using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;

public class Basket : MonoBehaviour
{
    [SerializeField] private float reEnterTime = 0.5f;
    private List<GameObject> recentlyExitedProducts = new List<GameObject>();
    
    IEnumerator AddProductToBasket(GameObject product, Grabbable grabbable)
    {
        while (grabbable.BeingHeld)
        {
            yield return null;
        }
        
        product.GetComponent<Rigidbody>().isKinematic = true;
        product.transform.SetParent(transform);
    }

    IEnumerator RemoveFromRecentlyExitedProducts(GameObject product)
    {
        yield return new WaitForSeconds(reEnterTime); 
        recentlyExitedProducts.Remove(product);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Product")) return;
        if (recentlyExitedProducts.Contains(other.gameObject)) return;
        
        Grabbable grabbable = other.GetComponent<Grabbable>();
        if (!grabbable.BeingHeld) return;
        
        StartCoroutine(AddProductToBasket(other.gameObject, grabbable));
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Product")) return;
        Grabbable grabbable = other.GetComponent<Grabbable>();
        if (!grabbable.BeingHeld) return;
        
        StopCoroutine(AddProductToBasket(other.gameObject, grabbable));
        
        other.transform.SetParent(null);
        
        recentlyExitedProducts.Add(other.gameObject);
        StartCoroutine(RemoveFromRecentlyExitedProducts(other.gameObject));
    }
}