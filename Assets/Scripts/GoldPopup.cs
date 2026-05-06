using UnityEngine;
using TMPro; // Для тексту

public class GoldPopup : MonoBehaviour
{
    [Header("Налаштування")]
    public float moveSpeed = 2f;
    public float lifetime = 1.5f;

    [Header("Посилання")]
    public TextMeshPro textMesh;           // Текст (використовуємо звичайний 3D текст для відкритого світу)
    public SpriteRenderer coinSprite;      // Картинка монетки

    private Color textColor;
    private Color spriteColor;

    void Awake()
    {
        // Запам'ятовуємо початкові кольори
        if (textMesh != null) textColor = textMesh.color;
        if (coinSprite != null) spriteColor = coinSprite.color;
    }

    public void Setup(int amount)
    {
        if (textMesh != null)
        {
            textMesh.text = "+" + amount.ToString() + " Gold";
        }

        // Легке випадкове зміщення, як у твоєму DamagePopup
        transform.position += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.2f, 0.2f), 0);
    }

    void Update()
    {
        // Рух вгору
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

        lifetime -= Time.deltaTime;

        // Починаємо плавно розчиняти об'єкт в останні 0.5 секунд його життя
        if (lifetime < 0.5f)
        {
            float fadeAmount = 2f * Time.deltaTime;

            // Розчиняємо текст
            if (textMesh != null)
            {
                textColor.a -= fadeAmount;
                textMesh.color = textColor;
            }

            // Розчиняємо картинку монетки одночасно з текстом!
            if (coinSprite != null)
            {
                spriteColor.a -= fadeAmount;
                coinSprite.color = spriteColor;
            }
        }

        // Видаляємо об'єкт, коли час вийшов
        if (lifetime <= 0)
        {
            Destroy(gameObject);
        }
    }
}