using System.Collections;
using System.Collections.Generic;
using BNG;
using TMPro;
using UnityEngine;

public class IndividualShelfManager : MonoBehaviour
{
    [SerializeField] private bool bottomShelf = false;
    [SerializeField] private GameObject shelfFillPoint;
    [SerializeField] private ShelfManager shelfManager;
    private Grabber handGrabber;
    private ControllerHand detectedHand;
    
    private int maxProductsPerRow = 13;
    private int maxRows = 3;
    
    private float smallProductSpacing = 0.12f;
    private float mediumProductSpacing = 0.30f;
    private float largeProductSpacing = 0.50f;
    private float rowDepthOffset = 0.14f;
    
    private List<Vector2Int> availableSpots = new List<Vector2Int>();
    private Dictionary<GameObject, Vector2Int> productPositions = new Dictionary<GameObject, Vector2Int>();
    private HashSet<GameObject> cooldownProducts = new HashSet<GameObject>();
    private string shelfProductType = null;
    
    private float reEntryCooldown = 1.0f;
    
    private bool isBeingGrabbed = false;
    
    private Coroutine waitForHandOpenCoroutine;

    private bool isInitialized = false;

    private int maxSpots;
    
    private void Start()
    {
        if (isInitialized) return;

        if (bottomShelf) maxRows++; 

        for (int row = 0; row < maxRows; row++)
        {
            for (int col = 0; col < maxProductsPerRow; col++)
            {
                availableSpots.Add(new Vector2Int(col, row));
                maxSpots = availableSpots.Count;
            }
        }

        shelfManager.RegisterShelf(this);
        isInitialized = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Product")) return;
        if (cooldownProducts.Contains(other.gameObject)) return;
        
        VRUtils.Instance.Log("Entered");

        GameObject removableProduct = other.gameObject;

        Grabbable removableGrabbable = removableProduct.GetComponent<Grabbable>();

        if (removableGrabbable != null && removableGrabbable.BeingHeld && productPositions.ContainsKey(removableProduct))
        {
            RemoveProduct(removableProduct);
        }

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb.isKinematic) return;

        ProductHolder product = other.GetComponent<ProductHolder>();
        if (product == null) return;

        if (shelfProductType != null && product.productData.productName != shelfProductType)
        {
            Debug.LogWarning($"This shelf only accepts: {shelfProductType}. {product.productData.productName} cannot be placed here!");
            return;
        }
        
        Grabbable grabbable = other.GetComponent<Grabbable>();
        if (grabbable != null && grabbable.BeingHeld)
        {
            waitForHandOpenCoroutine = StartCoroutine(WaitForHandOpen(grabbable, other.gameObject, product.productData.size, product.productData.productName));
        }
        else
        {
            PlaceProduct(other.gameObject, product.productData.size, product.productData.productName);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerHand") || other.CompareTag("Product"))
        {
            if (waitForHandOpenCoroutine != null)
            {
                StopCoroutine(waitForHandOpenCoroutine);
                waitForHandOpenCoroutine = null;
            }
            isBeingGrabbed = false;
        }
    }
    
    private IEnumerator WaitForHandOpen(Grabbable grabbable, GameObject product, int size, string productType)
    {
        while (grabbable.BeingHeld)
        {
            VRUtils.Instance.Log("hand closed");
            yield return null;
        }
        VRUtils.Instance.Log("hand open");
        PlaceProduct(product, size, productType);
        isBeingGrabbed = false;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void PlaceProduct(GameObject product, int size, string productType)
    {
        if (shelfFillPoint == null)
        {
            Debug.LogError("ShelfFillPoint is null");
            return;
        }

        if (shelfProductType == null)
        {
            shelfProductType = productType;
            ProductHolder productData = product.GetComponent<ProductHolder>();
            SetPriceTagPrice(productData.productData.price);
        }

        if (availableSpots.Count == 0)
        {
            Debug.LogWarning("Shelf is full, can't place more products.");
            return;
        }
        
        Vector2Int spot = availableSpots[0];
        availableSpots.RemoveAt(0);

        VRUtils.Instance.Log($"Available spots count after placing: {availableSpots.Count}");
    
        float spacing = GetSpacing(size);
        Vector3 newPosition = shelfFillPoint.transform.position + new Vector3(spot.x * spacing, 0.1f, spot.y * rowDepthOffset);
    
        product.transform.position = newPosition;
        product.transform.rotation = Quaternion.identity;

        product.GetComponent<Rigidbody>().isKinematic = true;
    
        productPositions[product] = spot;
        shelfManager.UpdateShelfProduct(this, productType);

        Debug.Log($"Placed {productType} at row {spot.y}, column {spot.x}");
    }

    private float GetSpacing(int size)
    {
        return size switch
        {
            1 => smallProductSpacing,
            2 => mediumProductSpacing,
            3 => largeProductSpacing,
            _ => smallProductSpacing,
        };
    }
    
    private IEnumerator RemoveProductWithDelay(GameObject product)
    {
        yield return new WaitForEndOfFrame();

        if (productPositions.TryGetValue(product, out Vector2Int spot))
        {
            Debug.Log($"Removing product from row {spot.y}, column {spot.x}");

            availableSpots.Add(spot);
            availableSpots.Sort((a, b) => a.y == b.y ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));

            productPositions.Remove(product);

            cooldownProducts.Add(product);
            StartCoroutine(RemoveFromCooldown(product));
            
            if (availableSpots.Count == maxSpots)
            {
                shelfProductType = null;
                SetPriceTagPrice(100f);
            }

            Debug.Log($"Removed product from row {spot.y}, column {spot.x}");
        }
        else
        {
            VRUtils.Instance.Log("No shelf product to remove.");
        }
    }

    private void SetPriceTagPrice(float price)
    {
        TMP_Text priceTag = GetComponentInChildren<TMP_Text>();
        if (Mathf.Approximately(price, 100f))
        {
            priceTag.text = "100%";
            return;
        }
        priceTag.text = $"€{price}";
    }

    public void RemoveProduct(GameObject product)
    {
        StartCoroutine(RemoveProductWithDelay(product));
    }
    
    private IEnumerator RemoveFromCooldown(GameObject product)
    {
        yield return new WaitForSeconds(reEntryCooldown);
        cooldownProducts.Remove(product);
    }
    
    public bool HasProduct(GameObject product)
    {
        bool exists = productPositions.ContainsKey(product);
        Debug.Log($"Checking if {product.name} exists on shelf: {exists}");
        return exists;
    }
    
    public bool HasAnyProducts()
    {
        return productPositions.Count > 0;
    }
}