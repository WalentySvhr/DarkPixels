using UnityEngine;
using TMPro; // Не забудь namespace для TMP

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 2f; // Швидкість підйому
    public float fadeSpeed = 1f; // Швидкість зникнення (alpha)
    public float lifetime = 1.5f; // Час життя тексту

    private TextMeshProUGUI textMesh;
    private Color textColor;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        textColor = textMesh.color;
    }

    // Метод, який ми викликаємо при створенні, щоб встановити текст і позицію
    public void SetText(string textValue, Vector2 spawnPosition)
    {
        textMesh.text = textValue;
        transform.position = spawnPosition;
        Destroy(gameObject, lifetime); // Самовидалення через lifetime
    }

    void Update()
    {
        // 1. Рухаємо текст вгору
        transform.Translate(Vector2.up * moveSpeed * Time.deltaTime);

        // 2. Плавно робимо його прозорим
        textColor.a -= fadeSpeed * Time.deltaTime;
        textMesh.color = textColor;
    }
}