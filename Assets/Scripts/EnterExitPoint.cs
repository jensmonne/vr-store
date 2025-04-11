using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterExitPoint : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    [SerializeField] private List<GameObject> npcs = new List<GameObject>();
    [SerializeField] private int dailyNpcAmount = 10;
    [SerializeField] private int currentNpcAmount = 0;
    [SerializeField] public int currentDestroyedNpcAmount = 0;

    private void Start()
    {
        if (npcs.Count == 0)
        {
            Debug.LogError("No NPCs assigned to the EnterExitPoint.");
        }
    }
    
    public void StartNpcSpawn()
    {
        for (int i = 0; i < dailyNpcAmount; i++)
        {
            SpawnNpc();
        }

        StartCoroutine(SpawnNpcs());
    }

    private void SpawnNpc()
    {
        int randomIndex = Random.Range(0, npcs.Count);
        GameObject npc = Instantiate(npcs[randomIndex], transform.position, Quaternion.identity);
        npc.transform.SetParent(transform);
        currentNpcAmount += 1;
    }

    private IEnumerator SpawnNpcs()
    {
        while (true)
        {
            if (Random.Range(0, 4) == 0)
            {
                if (currentNpcAmount >= dailyNpcAmount)
                {
                    yield return new WaitUntil(() => currentNpcAmount == currentDestroyedNpcAmount);
                    gameManager.EndGame();
                }
                SpawnNpc();
            }

            yield return new WaitForSeconds(7f);
        }
    }
}