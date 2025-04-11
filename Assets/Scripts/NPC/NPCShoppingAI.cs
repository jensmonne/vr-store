using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCShoppingAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private NPCShoppingList shoppingList;
    private NPCState state;

    [SerializeField] private List<Transform> shelfLocations;
    private Transform enterExitPoint;
    private Transform chosenCheckout;
    private int currentTargetIndex = 0;
    [SerializeField] private EnterExitPoint enterExit;

    [SerializeField] private NPCShoppingManager shoppingManager;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        shoppingList = GetComponent<NPCShoppingList>();
        shoppingManager = NPCShoppingManager.Instance;
        enterExitPoint = GameObject.FindGameObjectWithTag("EnterExitPoint").transform;
        enterExit = enterExitPoint.GetComponent<EnterExitPoint>();

        if (shoppingList == null || shoppingList.GetShoppingList().Count == 0)
        {
            Debug.LogError($"{name} has no shopping list!");
            return;
        }

        shelfLocations = new List<Transform>();
        for (int i = 0; i < shoppingList.GetShoppingList().Count; i++)
        {
            var shelf = shoppingManager.GetNextShelf(i);
            if (shelf != null) shelfLocations.Add(shelf);
        }

        enterExitPoint = shoppingManager.enterExitPoint;

        state = NPCState.Entering;
        Debug.Log($"{name} is entering the store.");
        agent.SetDestination(enterExitPoint.position);
    }

    private void Update()
    {
        if (agent.pathPending) return;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            switch (state)
            {
                case NPCState.Entering:
                    Debug.Log($"{name} entered the store.");
                    state = NPCState.GettingBasket;
                    Invoke(nameof(GetBasketOrCart), 1f);
                    break;

                case NPCState.WalkingToShelf:
                    state = NPCState.PickingProduct;
                    Invoke(nameof(PickProduct), 1f);
                    break;

                case NPCState.PickingProduct:
                    MoveToNextShelf();
                    break;

                case NPCState.CheckoutQueueing:
                    state = NPCState.ScanningProducts;
                    Debug.Log($"{name} reached checkout and is scanning products.");
                    Invoke(nameof(ProcessCheckout), 2f);
                    break;

                case NPCState.Paying:
                    Invoke(nameof(GoToExit), 1.5f);
                    break;

                case NPCState.Exiting:
                    Debug.Log($"{name} is exiting.");
                    enterExit.currentDestroyedNpcAmount--;
                    Destroy(gameObject);
                    break;
            }
        }
    }

    private void GetBasketOrCart()
    {
        Debug.Log($"{name} picked up a basket.");
        MoveToNextShelf();
    }

    private void MoveToNextShelf()
    {
        if (currentTargetIndex < shelfLocations.Count)
        {
            state = NPCState.WalkingToShelf;
            Transform nextShelf = shelfLocations[currentTargetIndex];
            agent.SetDestination(nextShelf.position);
            Debug.Log($"{name} moving to shelf {currentTargetIndex}.");
            currentTargetIndex++;
        }
        else
        {
            JoinCheckoutQueue();
        }
    }

    private void PickProduct()
    {
        // Get the product the NPC is supposed to pick
        if (currentTargetIndex < shelfLocations.Count)
        {
            Transform nextShelf = shelfLocations[currentTargetIndex];
            if (nextShelf != null)
            {
                Debug.Log($"{name} picked an item from shelf {currentTargetIndex}.");
                // If product is missing or unavailable at the shelf, log a warning and move on
                if (!IsProductAvailable(nextShelf))
                {
                    Debug.LogWarning($"{name} couldn't find the item on shelf {currentTargetIndex}. Moving to the next product.");
                    MoveToNextShelf();  // Move to the next shelf for the next product
                    return;
                }
            }
            else
            {
                Debug.LogWarning($"{name} found a null shelf at index {currentTargetIndex}. Moving on.");
                MoveToNextShelf();  // Move to the next shelf
                return;
            }
        }
        else
        {
            JoinCheckoutQueue();  // If all products are picked, proceed to checkout
        }
    }

    private bool IsProductAvailable(Transform shelf)
    {
        // Add logic to check if the product is available on the shelf
        // This could be based on whether there's any product object at the shelf, or other conditions
        // For example, checking if the shelf has products tagged as "Product" or some specific component
    
        var productObjects = shelf.GetComponentsInChildren<Transform>(); // Example check, you can modify it for your scenario

        if (productObjects.Length == 0)  // If no products found, consider the item as unavailable
        {
            return false;
        }

        return true;  // If there are product objects, consider the item as available
    }


    private void JoinCheckoutQueue()
    {
        chosenCheckout = shoppingManager.GetLeastBusyCheckout();
        if (chosenCheckout != null)
        {
            shoppingManager.JoinCheckoutQueue(chosenCheckout);
            agent.SetDestination(chosenCheckout.position);
            Debug.Log($"{name} joining queue at {chosenCheckout.name}.");
            state = NPCState.CheckoutQueueing;
        }
        else
        {
            Debug.LogWarning($"{name} couldn't find a checkout to join!");
        }
    }

    private void ProcessCheckout()
    {
        Debug.Log($"{name} is processing checkout...");
        state = NPCState.Paying;
        Invoke(nameof(PayForItems), 2f);
    }

    private void PayForItems()
    {
        Debug.Log($"{name} paid for their items.");
        shoppingManager.LeaveCheckoutQueue(chosenCheckout);
        state = NPCState.Paying;
    }

    private void GoToExit()
    {
        Debug.Log($"{name} is leaving the store.");
        state = NPCState.Exiting;
        agent.SetDestination(enterExitPoint.position);
    }
}
