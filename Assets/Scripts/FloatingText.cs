using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float fadeSpeed = 1f;
    public float lifetime = 1.5f;

    public TextMeshProUGUI textMesh; // Сюди перетягнемо текст в інспекторі
    private Color textColor;

    void Start()
    {
        if (textMesh != null) textColor = textMesh.color;
        Destroy(gameObject, lifetime);
    }

    public void SetText(string textValue, Vector2 spawnPosition)
    {
        if (textMesh == null) return;
        textMesh.text = textValue;
        transform.position = spawnPosition;
    }

    void Update()
    {
        if (textMesh == null) return;
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        textColor.a -= fadeSpeed * Time.deltaTime;
        textMesh.color = textColor;
    }
}