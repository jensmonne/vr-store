using System;
using System.Collections;
using UnityEngine;

public class SlidingDoors : MonoBehaviour
{
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;
    private float slideDistance = 2f;
    private float slideSpeed = 2f;
    
    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;
    private Vector3 leftOpenPos;
    private Vector3 rightOpenPos;
    private Coroutine moveCoroutine;

    private void Start()
    {
        leftClosedPos = leftDoor.localPosition;
        rightClosedPos = rightDoor.localPosition;

        leftOpenPos = leftClosedPos + Vector3.forward * slideDistance;
        rightOpenPos = rightClosedPos + Vector3.back * slideDistance;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Uncomment if only the player/npc's is supposed to open the doors
        // if (!other.CompareTag("Player") || !other.CompareTag("NPC")) return;
        
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveDoors(leftOpenPos, rightOpenPos));
    }

    private void OnTriggerExit(Collider other)
    {
        // Uncomment if only the player/npc's is supposed to open the doors
        // if (!other.CompareTag("Player") || !other.CompareTag("NPC")) return;
        
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveDoors(leftClosedPos, rightClosedPos));
    }
    
    private IEnumerator MoveDoors(Vector3 leftTarget, Vector3 rightTarget)
    {
        while (Vector3.Distance(leftDoor.localPosition, leftTarget) > 0.01f)
        {
            leftDoor.localPosition = Vector3.MoveTowards(leftDoor.localPosition, leftTarget, slideSpeed * Time.deltaTime);
            rightDoor.localPosition = Vector3.MoveTowards(rightDoor.localPosition, rightTarget, slideSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
