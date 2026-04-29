using UnityEngine;

public class NextFloorDoor : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Якщо в двері зайшов гравець
        if (other.CompareTag("Player"))
        {
            // ОПЦІОНАЛЬНО: Тут можна додати анімацію затемнення екрана (Fade Out)
            Debug.Log("<color=magenta>Гравець зайшов у двері!</color>");
            // Кажемо менеджеру башти перекинути нас далі
            if (TowerManager.Instance != null)
            {
                TowerManager.Instance.GoToNextFloor();
            }
        }
    }
}