using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("UI Елементи Мапи")]
    [SerializeField] private GameObject fullMapWindow;
    [SerializeField] private GameObject miniMapWindow;

    [Header("Налаштування геймплею")]
    [SerializeField] private bool pauseGameOnOpen = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (fullMapWindow != null)
        {
            fullMapWindow.SetActive(false);
        }
    }

    public void OpenFullMap()
    {
        // --- ОСЬ ЦЯ ПЕРЕВІРКА ЗАБЛОКУЄ МАПУ В БАШТІ ---
        if (TowerManager.Instance != null && TowerManager.Instance.IsPlayerInTower)
        {
            Debug.Log("Відкриття мапи заблоковано: гравець у башті.");
            return; // Вихід з методу, код далі не виконується
        }

        if (fullMapWindow == null) return;

        fullMapWindow.SetActive(true);

        if (miniMapWindow != null)
        {
            miniMapWindow.SetActive(false);
        }

        if (pauseGameOnOpen)
        {
            Time.timeScale = 0f;
        }
    }

    public void CloseFullMap()
    {
        if (fullMapWindow == null) return;

        fullMapWindow.SetActive(false);

        if (miniMapWindow != null)
        {
            miniMapWindow.SetActive(true);
        }

        if (pauseGameOnOpen)
        {
            Time.timeScale = 1f;
        }
    }
}