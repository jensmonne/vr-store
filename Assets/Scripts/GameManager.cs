using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] EnterExitPoint enterExitPoint;
    [SerializeField] bool isGameStarted = false;
    private static int money = 100;

    public void StartGame()
    {
        enterExitPoint.StartNpcSpawn();
        isGameStarted = true;
    }
    
    public void EndGame()
    {
        isGameStarted = false;
    }
    
    public static void AddMoney(int amount)
    {
        money += amount;
    }

    public static bool CheckMoney(int amount)
    {
        if (money < amount)
        {
            Debug.Log("Not enough money!");
            return false;
        }
        else return true;
    }

    public static void RemoveMoney(int amount)
    {
        money -= amount;
    }
}
