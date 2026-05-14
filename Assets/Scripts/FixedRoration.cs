using UnityEngine;

public class FixedRotation : MonoBehaviour
{
    // Початковий поворот, який ми хочемо зберегти
    private Quaternion _initialRotation;

    void Start()
    {
        // Запам'ятовуємо поворот об'єкта при старті (світовий)
        _initialRotation = transform.rotation;
    }

    // Використовуємо LateUpdate, щоб спрацювати після того, 
    // як NPC вже розвернувся в звичайному Update
    void LateUpdate()
    {
        // Примусово повертаємо текст у початковий стан
        transform.rotation = _initialRotation;
    }
}