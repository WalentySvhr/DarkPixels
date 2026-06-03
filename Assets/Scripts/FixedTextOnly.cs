using UnityEngine;

public class FixedTextOnly : MonoBehaviour
{
    private Quaternion _initialRotation;
    private Vector3 _initialLocalScale;

    void Start()
    {
        // Запам'ятовуємо початковий світовий поворот і локальний масштаб тексту
        _initialRotation = transform.rotation;
        _initialLocalScale = transform.localScale;
    }

    void LateUpdate()
    {
        // 1. Примусово тримаємо світовий поворот тексту рівно
        transform.rotation = _initialRotation;

        // 2. Рятуємо текст від віддзеркалення (якщо NPC розвернувся вліво)
        if (transform.parent != null)
        {
            // lossyScale - це фінальний глобальний масштаб об'єкта у світі з урахуванням усіх батьків
            Vector3 parentGlobalScale = transform.parent.lossyScale;
            Vector3 newScale = _initialLocalScale;

            // Якщо батьківський об'єкт розвернувся в мінус по X
            if (parentGlobalScale.x < 0)
            {
                // Робимо свій масштаб теж негативним. Мінус на мінус дасть плюс, і текст стане читатися зліва направо!
                newScale.x = -Mathf.Abs(_initialLocalScale.x);
            }
            else
            {
                // Якщо NPC дивиться вправо, тримаємо масштаб позитивним
                newScale.x = Mathf.Abs(_initialLocalScale.x);
            }

            transform.localScale = newScale;
        }
    }
}