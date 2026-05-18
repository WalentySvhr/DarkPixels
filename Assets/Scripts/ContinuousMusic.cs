using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement; // ОБОВ'ЯЗКОВО: для відстеження завантаження сцен
using System.Collections.Generic;

public class ContinuousPlaylist : MonoBehaviour
{
    public static ContinuousPlaylist instance;

    [Header("Налаштування мікшера")]
    public AudioMixerGroup musicOutput; // Канал Music (MainMixer)

    [Header("Плейлист")]
    public List<AudioClip> playlist = new List<AudioClip>(); // Твої треки
    public bool shuffleMode = false; // Чи перемішувати музику?

    private AudioSource audioSource;
    private int currentTrackIndex = 0;
    private List<int> playOrder = new List<int>(); // Черга програвання
    private float silenceTimer = 0f;

    private float sceneTransitionCooldown = 0f; // Таймер блокування перемикання

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Об'єкт живе між сценами

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

            if (musicOutput != null)
                audioSource.outputAudioMixerGroup = musicOutput;

            audioSource.loop = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Підписуємося на подію завантаження сцени в Unity
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Відписуємося при знищенні, щоб уникати витоку пам'яті
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Цей метод викликається АВТОМАТИЧНО, як тільки будь-яка сцена завантажилась
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (instance != this) return;

        // Блокуємо перевірку закінчення треку на 1.2 секунди.
        // Цього з головою вистачить, щоб Unity переімпортувала камери та AudioListener.
        sceneTransitionCooldown = 1.2f;
        silenceTimer = 0f;
    }

    void Start()
    {
        if (instance != this) return;

        if (playlist.Count > 0)
        {
            GeneratePlayOrder();
            PlayCurrentTrack();
        }
        else
        {
            Debug.LogWarning("Плейлист порожній! Додай треки в Інспекторі.");
        }
    }

    void Update()
    {
        if (instance != this) return;

        // Якщо діє захисний таймер після зміни сцени — просто зменшуємо його і виходимо
        if (sceneTransitionCooldown > 0)
        {
            sceneTransitionCooldown -= Time.deltaTime;
            return;
        }

        if (playlist.Count > 0)
            HandleTrackTransition();
    }

    void HandleTrackTransition()
    {
        if (!audioSource.isPlaying)
        {
            // Додатковий захист від мікро-лагів кадрів
            if (Time.deltaTime < 0.15f)
            {
                silenceTimer += Time.deltaTime;
            }

            if (silenceTimer >= 0.5f)
            {
                PlayNextTrack();
                silenceTimer = 0f;
            }
        }
        else
        {
            silenceTimer = 0f;
        }
    }

    void GeneratePlayOrder()
    {
        playOrder.Clear();
        for (int i = 0; i < playlist.Count; i++) playOrder.Add(i);

        if (shuffleMode)
        {
            for (int i = 0; i < playOrder.Count; i++)
            {
                int temp = playOrder[i];
                int randomIndex = Random.Range(i, playOrder.Count);
                playOrder[i] = playOrder[randomIndex];
                playOrder[randomIndex] = temp;
            }
        }
    }

    void PlayCurrentTrack()
    {
        if (playlist.Count == 0) return;

        int trackToPlay = playOrder[currentTrackIndex];
        audioSource.clip = playlist[trackToPlay];
        audioSource.Play();

        Debug.Log($"Зараз грає: {playlist[trackToPlay].name}");
    }

    public void PlayNextTrack()
    {
        currentTrackIndex++;
        if (currentTrackIndex >= playlist.Count)
        {
            currentTrackIndex = 0;
            if (shuffleMode) GeneratePlayOrder();
        }
        PlayCurrentTrack();
    }
}