using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    private static PersistentObject instance;

    void Awake()
    {
        // Перевіряємо, чи такий об'єкт уже існує (наприклад, з минулого поверху)
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Забороняємо видалення
        }
        else
        {
            // Якщо ми завантажили нову сцену і там з'явився новий дублікат - видаляємо його
            Destroy(gameObject);
        }
    }
}