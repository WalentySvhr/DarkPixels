using UnityEngine;

public class LevelInitializer : MonoBehaviour
{
    public string spawnPointName = "SpawnPoint_FromCave";

    void Start()
    {
        // Шукаємо гравця за тегом
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        // Шукаємо точку появи за назвою
        GameObject spawnPoint = GameObject.Find(spawnPointName);

        if (player != null && spawnPoint != null)
        {
            player.transform.position = spawnPoint.transform.position;
            Debug.Log("Гравець перенесений на точку появи!");
        }
    }
}