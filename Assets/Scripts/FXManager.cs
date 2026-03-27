using UnityEngine;

public class FXManager : MonoBehaviour
{
    public static FXManager instance;

    public GameObject healTextPrefab; // Сюди перетягни свій синій префаб з Assets
    public Transform playerTransform; // Твій Player (об'єкт зі сцени)

    void Awake()
    {
        if (instance == null) instance = this;
    }

    public void SpawnHealText(int amount)
    {
        if (healTextPrefab == null || playerTransform == null) return;

        // Позиція над головою гравця
        Vector3 spawnPos = playerTransform.position + new Vector3(0, 1.5f, 0);

        // Створюємо копію
        GameObject newText = Instantiate(healTextPrefab, spawnPos, Quaternion.identity);

        // Передаємо значення
        FloatingText ft = newText.GetComponent<FloatingText>();
        if (ft != null)
        {
            ft.SetText("+" + amount, spawnPos);
        }
    }
}