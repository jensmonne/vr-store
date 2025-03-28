using UnityEngine;

[CreateAssetMenu(fileName = "New Product", menuName = "Product Data")]
public class ProductData : ScriptableObject
{
    public string productName;
    public float price;
    public int size;
    public int maxQuantity;
}