using UnityEngine;

public class FloatingIcon : MonoBehaviour
{
    [Header("Налаштування левітації")]
    [Tooltip("Швидкість руху вгору-вниз")]
    public float speed = 3f;

    [Tooltip("Наскільки високо підіймається іконка")]
    public float height = 0.15f;

    // Зберігаємо стартову позицію, щоб іконка не "полетіла" в космос
    private Vector3 startPos;

    void Start()
    {
        // Беремо localPosition, бо іконка рухається разом з NPC
        startPos = transform.localPosition;
    }

    void Update()
    {
        // Mathf.Sin генерує плавну хвилю від -1 до 1 на основі часу
        float newY = startPos.y + Mathf.Sin(Time.time * speed) * height;

        // Застосовуємо нові координати, не чіпаючи X та Z
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
    }
}