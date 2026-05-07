using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float disappearTime = 1f;
    private TextMeshProUGUI textMesh;
    private Color textColor;

    [Header("Звичайний урон")]
    public Color normalColor = Color.white;
    public float normalFontSize = 36f; // Підбери розмір під свій Canvas

    [Header("Критичний урон")]
    public Color critColor = Color.red;
    public float critFontSize = 55f; // Більший розмір для крита
    public string critPrefix = "КРІТ! ";

    void Awake()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
    }

    // ОНОВЛЕНО: Тепер метод приймає параметр isCrit (за замовчуванням false, щоб не зламати старий код)
    public void Setup(int damageAmount, bool isCrit = false)
    {
        if (isCrit)
        {
            // Налаштування для критичного удару
            textMesh.text = $"{critPrefix}-{damageAmount}";
            textMesh.color = critColor;
            textMesh.fontSize = critFontSize;

            // Робимо так, щоб критичний урон висів трохи довше і летів трохи швидше (за бажанням)
            moveSpeed *= 1.2f;
            disappearTime += 0.2f;
        }
        else
        {
            // Налаштування для звичайного удару
            textMesh.text = $"-{damageAmount}";
            textMesh.color = normalColor;
            textMesh.fontSize = normalFontSize;
        }

        // Запам'ятовуємо поточний колір для логіки зникнення (альфа-каналу)
        textColor = textMesh.color;

        // Випадкове зміщення
        transform.position += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.2f, 0.2f), 0);
        Destroy(gameObject, disappearTime);
    }

    void Update()
    {
        // Рух вгору
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

        // Плавне зникнення
        if (disappearTime > 0)
        {
            disappearTime -= Time.deltaTime;

            if (disappearTime < 0.5f)
            {
                textColor.a -= 2f * Time.deltaTime;
                textMesh.color = textColor;
            }
        }
    }
}