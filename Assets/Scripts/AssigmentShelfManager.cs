using System.Collections.Generic;
using UnityEngine;

public class AssigmentShelfManager : MonoBehaviour
{
    [SerializeField] private GameObject shelfProduct;
    public GameObject[] shelfSlots;

    private void Start()
    {
        FindShelfSlots();
        SetupShelves();
    }

    private void FindShelfSlots()
    {
        GameObject parentObject = GameObject.FindWithTag("Placements");

        if (parentObject != null)
        {
            List<GameObject> slotsList  = new List<GameObject>();

            foreach (Transform child in parentObject.transform)
            {
                slotsList.Add(child.gameObject);
            }
            shelfSlots = slotsList.ToArray();
        }
        else
        {
            Debug.LogError("No object with tag 'Placements' found in the scene.");
        }
    }

    private void SetupShelves()
    {
        if (shelfSlots == null || shelfProduct == null) return;

        foreach (GameObject slot in shelfSlots)
        {
            GameObject product = Instantiate(shelfProduct, slot.transform.position, Quaternion.identity);
            product.transform.localScale = Vector3.one;
        }
    }
}
