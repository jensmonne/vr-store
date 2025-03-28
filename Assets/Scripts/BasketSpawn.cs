using System.Collections;
using UnityEngine;
using BNG;

public class BasketSpawn : MonoBehaviour
{
    [SerializeField] private GameObject basketPrefab;
    [SerializeField] private Transform spawnPoint;
    
    private GameObject currentBasket;
    private Grabber handGrabber;
    private ControllerHand detectedHand;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PlayerHand")) return;
        
        handGrabber = other.GetComponentInParent<Grabber>();

        if (handGrabber == null) return;
        
        detectedHand = handGrabber.HandSide;
        
        StartCoroutine(WaitForHandClose());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("PlayerHand")) return;
        
        StopCoroutine(WaitForHandClose());
        handGrabber = null;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator WaitForHandClose()
    {
        while (handGrabber && !handGrabber.HoldingItem)
        {
            float grip = detectedHand == ControllerHand.Left ? InputBridge.Instance.LeftGrip : InputBridge.Instance.RightGrip;
            if (grip > 0.8f)
            {
                SpawnBasket();
                yield break;
            }
            yield return null;
        }
    }

    private void SpawnBasket()
    {
        if (currentBasket != null) return; 
        
        currentBasket = Instantiate(basketPrefab, spawnPoint.position, spawnPoint.rotation);
        Grabbable basketGrabbable = currentBasket.GetComponent<Grabbable>();

        if (basketGrabbable == null) return;
        
        handGrabber.GrabGrabbable(basketGrabbable);
        StartCoroutine(WaitForBasketRelease(basketGrabbable));
    }
    
    private IEnumerator WaitForBasketRelease(Grabbable basketGrabbable)
    {
        while (basketGrabbable.BeingHeld)
        {
            VRUtils.Instance.Log("hand closed");
            yield return null;
        }
        currentBasket = null;
    }
}