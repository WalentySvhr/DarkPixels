using UnityEngine;

public class PetSpawner : MonoBehaviour
{
    public static PetSpawner Instance;

    private GameObject currentActivePetObject; // Посилання на живого пета на сцені

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Метод для створення пета на сцені
    public void SpawnPet(PetData petData)
    {
        // Якщо на сцені вже літає пет — спочатку видаляємо його
        DespawnPet();

        if (petData == null)
        {
            Debug.LogError("<color=red>[PetSpawner]</color> Спроба заспавнити пета, але petData рівний null!");
            return;
        }

        if (petData.petPrefab == null)
        {
            Debug.LogError($"<color=red>[PetSpawner]</color> У ScriptableObject '{petData.itemName}' НЕ ЗАДАНИЙ префаб пета в Інспекторі!");
            return;
        }

        // Спавнимо префаб пета трохи збоку від гравця
        Vector3 spawnPosition = transform.position + new Vector3(-1f, 1f, 0f);
        currentActivePetObject = Instantiate(petData.petPrefab, spawnPosition, Quaternion.identity);

        // --- ДИНАМІЧНЕ ПРИЗНАЧЕННЯ ЦІЛІ ---
        // Отримуємо скрипт руху, який висить на заспавненому вовку
        PetFollower follower = currentActivePetObject.GetComponent<PetFollower>();
        if (follower != null)
        {
            follower.playerTarget = this.transform; // Передаємо трансформ гравця
            Debug.Log($"<color=cyan>[PetSpawner]</color> Скрипт PetFollower знайдено. Ціль (Player) успішно передана помічнику!");
        }
        else
        {
            // Якщо вовк заспавнився, але стоїть на місці — консоль одразу підкаже, що на префабі немає потрібного скрипта
            Debug.LogWarning($"<color=yellow>[PetSpawner]</color> Попередження: На префабі '{petData.petPrefab.name}' не знайдено компонент PetFollower! Пет не знає за чим бігти.");
        }

        Debug.Log($"<color=lime>[PetSpawner]</color> {petData.itemName} успішно спавнився на арені!");
    }

    // Метод для видалення пета зі сцени
    public void DespawnPet()
    {
        if (currentActivePetObject != null)
        {
            Destroy(currentActivePetObject);
            currentActivePetObject = null;
            Debug.Log("<color=orange>[PetSpawner]</color> Попереднього помічника видалено зі сцени.");
        }
    }
}