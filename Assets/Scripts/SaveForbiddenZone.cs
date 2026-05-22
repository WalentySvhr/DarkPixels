using UnityEngine;

public class SaveForbiddenZone : MonoBehaviour
{
    public static bool CanSave = true;

    // Запобіжник від "фантомних" виходів з трігера при видаленні сцени
    private bool isDestroying = false;

    private void Start()
    {
        // Завжди скидаємо стан на true при завантаженні нормальної сцени
        CanSave = true;
        isDestroying = false;
    }

    private void OnDisable()
    {
        // Спрацьовує, коли Unity починає руйнувати сцену при переході в меню
        isDestroying = true;
    }

    private void OnApplicationQuit()
    {
        // Спрацьовує, якщо гравець просто згорнув/закрив гру на телефоні
        isDestroying = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CanSave = false;
            Debug.Log("<color=red>[SaveZone]</color> Гравець у башті. Збереження заблоковано.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // ГОЛОВНИЙ ФІКС: Якщо гра закривається або сцена вивантажується - ігноруємо!
        if (isDestroying) return;

        if (other.CompareTag("Player"))
        {
            CanSave = true;
            Debug.Log("<color=green>[SaveZone]</color> Гравець вийшов з башти. Збереження дозволено.");
        }
    }
}