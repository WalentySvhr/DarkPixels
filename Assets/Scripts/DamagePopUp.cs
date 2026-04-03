using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float disappearTime = 1f;
    private TextMeshProUGUI textMesh;
    private Color textColor;

    void Awake()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        textColor = textMesh.color;
    }

    public void Setup(int damageAmount)
    {
        // ДОДАНО: Знак мінуса перед числом
        textMesh.text = "-" + damageAmount.ToString();

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