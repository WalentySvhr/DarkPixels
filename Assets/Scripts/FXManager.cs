using UnityEngine;

public class FXManager : MonoBehaviour
{
    public static FXManager instance;

    [Header("UI Text Prefab")]
    public GameObject healTextPrefab;

    [Header("Player Tracking")]
    public Transform playerTransform;
    public Vector3 popupOffset = new Vector3(0, 1.5f, 0);

    [Header("Розкид тексту (Random Spread)")]
    [Tooltip("Максимальне зміщення по горизонталі (вліво/вправо)")]
    public float spreadX = 0.5f;
    [Tooltip("Максимальне зміщення по вертикалі (вгору/вниз)")]
    public float spreadY = 0.3f;

    [Header("Colors")]
    public Color healthColor = Color.green;
    public Color manaColor = Color.cyan;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    // Спавн тексту лікування ХП (Зелений)
    public void SpawnHealText(int amount)
    {
        if (healTextPrefab == null || playerTransform == null) return;

        // Рахуємо базову позицію + додаємо випадковий розкид
        Vector3 randomOffset = new Vector3(
            Random.Range(-spreadX, spreadX),
            Random.Range(-spreadY, spreadY),
            0
        );
        Vector3 spawnPos = playerTransform.position + popupOffset + randomOffset;

        GameObject newText = Instantiate(healTextPrefab, spawnPos, Quaternion.identity);

        FloatingText ft = newText.GetComponent<FloatingText>();
        if (ft != null)
        {
            ft.SetText("+" + amount, spawnPos);
        }
    }

    // Спавн тексту регену манни (Бірюзовий)
    public void SpawnManaText(int amount)
    {
        if (healTextPrefab == null || playerTransform == null) return;

        // Для манни теж робимо випадковий розкид
        Vector3 randomOffset = new Vector3(
            Random.Range(-spreadX, spreadX),
            Random.Range(-spreadY, spreadY),
            0
        );
        // Можна залишити базове зміщення +0.3f по X, щоб манна в середньому була трохи правіше, + додаємо рандом
        Vector3 spawnPos = playerTransform.position + popupOffset + new Vector3(0.3f, 0.2f, 0) + randomOffset;

        GameObject newText = Instantiate(healTextPrefab, spawnPos, Quaternion.identity);

        FloatingText ft = newText.GetComponent<FloatingText>();
        if (ft != null)
        {
            ft.SetText("+" + amount, spawnPos);

            var textMesh = newText.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (textMesh != null)
            {
                textMesh.color = manaColor;
            }
        }
    }
}