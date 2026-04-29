using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;
    private long _currentUnixTime;
    private bool _isTimeReady = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    void Start() => StartCoroutine(GetRealTime());

    IEnumerator GetRealTime()
    {
        // Використовуємо безкоштовне API для отримання часу
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://worldtimeapi.org/api/timezone/Etc/UTC"))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Парсимо JSON (можна спростити, витягнувши unixtime через string.IndexOf)
                string json = webRequest.downloadHandler.text;
                int startIndex = json.IndexOf("unixtime\":") + 10;
                int endIndex = json.IndexOf(",", startIndex);
                string unixStr = json.Substring(startIndex, endIndex - startIndex);

                _currentUnixTime = long.Parse(unixStr);
                _isTimeReady = true;
                Debug.Log("<color=cyan>[TimeManager]</color> Час синхронізовано.");
            }
            else
            {
                // Якщо немає інтернету, беремо час телефону (але це вразливо до читів)
                _currentUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                _isTimeReady = true;
                Debug.LogWarning("[TimeManager] Немає інтернету, використано час пристрою.");
            }
        }
    }

    public long GetCurrentUnixTime()
    {
        // Додаємо час, що пройшов з моменту запуску гри
        return _currentUnixTime + (long)Time.realtimeSinceStartup;
    }

    public bool IsReady() => _isTimeReady;
}