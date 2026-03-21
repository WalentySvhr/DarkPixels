using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public float moveSpeed = 2f;      // Швидкість польоту вгору
    public float disappearTime = 1f;  // Через скільки секунд зникне
    private TextMeshProUGUI textMesh;
    private Color textColor;

    void Awake()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        textColor = textMesh.color;
    }

    public void Setup(int damageAmount)
    {
        textMesh.text = damageAmount.ToString();
        // Випадкове зміщення, щоб цифри не накладалися одна на одну
        transform.position += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.2f, 0.2f), 0);
        Destroy(gameObject, disappearTime);
    }

    void Update()
    {
        // Рух вгору
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

        // Плавне зникнення (альфа-канал)
        disappearTime -= Time.deltaTime;
        if (disappearTime < 0.5f) // Починаємо зникати в другій половині життя
        {
            textColor.a -= 2f * Time.deltaTime;
            textMesh.color = textColor;
        }
    }
}