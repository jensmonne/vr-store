using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCShoppingAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private NPCShoppingList shoppingList;
    private NPCState state;
    
    private List<Transform> shelfLocations;
    private List<Transform> checkoutPoints; // Need to make this more of a queue thing so npc's auto pick the least used checkout
    private Transform enterPoint;
    private Transform exitPoint;

    private Transform chosenCheckout;
    private int currentTargetIndex = 0;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        shoppingList = GetComponent<NPCShoppingList>();

        if (shoppingList == null || shoppingList.GetShoppingList().Count == 0)
        {
            Debug.LogError("NPC has no shopping list!");
            return;
        }

        state = NPCState.Entering;
        agent.SetDestination(enterPoint.position);
    }

    private void Update()
    {
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            switch (state)
            {
                case NPCState.Entering:
                    MoveToNextShelf();
                    break;
                
                case NPCState.GettingBasket:
                    GetBasketOrCart();
                    break;
                
                case NPCState.WalkingToShelf:
                    PickProduct();
                    break;
                
                case NPCState.PickingProduct:
                    MoveToNextShelf();
                    break;
                
                case NPCState.CheckoutQueueing:
                    ProcessCheckout();
                    break;
                
                case NPCState.ScanningProducts:
                    PayForItems();
                    break;

                case NPCState.Paying:
                    GoToExit();
                    break;

                case NPCState.Exiting:
                    Destroy(gameObject);
                    break;
            }
        }
    }

    private void GetBasketOrCart()
    {
        
    }

    private void MoveToNextShelf()
    {
        if (currentTargetIndex < shelfLocations.Count)
        {
            state = NPCState.WalkingToShelf;
            agent.SetDestination(shelfLocations[currentTargetIndex].position);
            currentTargetIndex++;
        }
        else
        {
            state = NPCState.WalkingToCheckout;
            JoinCheckoutQueue();
        }
    }

    private void PickProduct()
    {
        Debug.Log($"{gameObject.name} picked an item.");
        state = NPCState.PickingProduct;
    }
    
    private void JoinCheckoutQueue()
    {
        chosenCheckout = GetLeastBusyCheckout();
        state = NPCState.CheckoutQueueing;
        agent.SetDestination(chosenCheckout.position);
    }
    
    private Transform GetLeastBusyCheckout()
    {
        if (checkoutPoints.Count == 0) return null;
        return checkoutPoints[Random.Range(0, checkoutPoints.Count)]; // TODO: Implement a real queue system
    }
    
    private void ProcessCheckout()
    {
        Debug.Log($"{gameObject.name} is scanning products.");
        state = NPCState.ScanningProducts;
        Invoke(nameof(PayForItems), 2f);
    }

    private void PayForItems()
    {
        Debug.Log($"{gameObject.name} is paying.");
        state = NPCState.Paying;
        Invoke(nameof(GoToExit), 2f);
    }

    private void GoToExit()
    {
        state = NPCState.Exiting;
        agent.SetDestination(exitPoint.position);
    }
}