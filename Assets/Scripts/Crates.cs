using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Crates : MonoBehaviour
{
    [SerializeField] private List<Rigidbody> productList = new List<Rigidbody>();

    private void Start()
    {
        productList = GetChildrenRigidbodies();
    }

    private void Update()
    {
        for (int i = productList.Count - 1; i >= 0; i--)
        {
            Rigidbody rb = productList[i];
            if (!rb.isKinematic)
            {
                // Unparent
                rb.transform.SetParent(null, true);

                GameObject detached = Instantiate(rb.gameObject);
                detached.transform.position = rb.transform.position;
                detached.transform.rotation = rb.transform.rotation;
                Destroy(rb.gameObject); // Or disable


                productList.RemoveAt(i);
            }

        }
    }

    private List<Rigidbody> GetChildrenRigidbodies()
    {
        List<Rigidbody> children = new List<Rigidbody>();
        foreach (Transform child in transform)
        {
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb != null)
            {
                children.Add(rb);
            }
        }
        return children;
    }
}