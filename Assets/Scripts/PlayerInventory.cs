using UnityEngine;
using TMPro; // Для відображення кількості монет на екрані

public class PlayerInventory : MonoBehaviour
{
    public int currentCoins = 0;
    public TextMeshProUGUI coinText; // Посилання на текст UI з монетами

    void Start()
    {
        UpdateCoinUI();
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        if (currentCoins < 0) currentCoins = 0;
        UpdateCoinUI();
    }
        
    void UpdateCoinUI()
    {
        if (coinText != null)
        {
            coinText.text = "Монети: " + currentCoins;
        }
    }
}