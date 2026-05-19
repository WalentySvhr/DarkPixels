using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement; // ОБОВ'ЯЗКОВО: для відстеження завантаження сцен
using System.Collections.Generic;
using System.Collections; // ОБОВ'ЯЗКОВО: для роботи Корутин (IEnumerator)

public class ContinuousPlaylist : MonoBehaviour
{
    public static ContinuousPlaylist instance;

    [Header("Налаштування мікшера")]
    public AudioMixerGroup musicOutput; // Канал Music (MainMixer)

    [Header("Плейлист за замовчуванням")]
    public List<AudioClip> playlist = new List<AudioClip>(); // Твої треки
    public bool shuffleMode = false; // Чи перемішувати музику?

    [Header("Налаштування плавного переходу")]
    [Range(0.1f, 3f)] public float fadeDuration = 1.0f; // Час затухання/наростання в секундах

    private AudioSource audioSource;
    private int currentTrackIndex = 0;
    private List<int> playOrder = new List<int>(); // Черга програвання
    private float silenceTimer = 0f;

    private float sceneTransitionCooldown = 0f; // Таймер блокування перемикання
    private Coroutine fadeCoroutine; // Посилання на поточну активну корутину переходу
    private float maxVolume = 1f; // Цільова максимальна гучність AudioSource

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
            maxVolume = audioSource.volume; // Запам'ятовуємо дефолтну гучність, яку ти виставив в інспекторі
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

    // Цей метод викликається АВТОМАТИЧНО, як тільки будь-яка场景 завантажилась
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

        // Автоматично переходимо на наступний трек лише тоді, коли зараз НЕ йде плавна зміна зони
        if (playlist.Count > 0 && fadeCoroutine == null)
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
        audioSource.volume = maxVolume; // Повертаємо гучність на початковий максимум
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

    // =======================================================
    // ОНОВЛЕНИЙ МЕТОД ДЛЯ ПОВНОЇ ЗАМІНИ ПЛЕЙЛИСТА З ЕФЕКТОМ FADE
    // =======================================================
    public void ChangeZonePlaylist(List<AudioClip> newTracks, bool shuffle)
    {
        if (newTracks == null || newTracks.Count == 0 || audioSource == null) return;

        // ПЕРЕВІРКА: Чи цей список такий самий, як поточний?
        if (playlist.Count == newTracks.Count && playlist.Count > 0 && playlist[0] == newTracks[0])
        {
            if (!audioSource.isPlaying) audioSource.Play();
            return;
        }

        // Якщо попередня зміна зони ще активна — зупиняємо її корутину, щоб не було конфліктів
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        // Запускаємо новий плавний перехід
        fadeCoroutine = StartCoroutine(FadeZonePlaylistRoutine(newTracks, shuffle));
    }

    // Корутина для плавного затухання та наростання звуку локацій
    private IEnumerator FadeZonePlaylistRoutine(List<AudioClip> newTracks, bool shuffle)
    {
        // 1. FADE OUT: Плавно глушимо попередній трек до 0
        if (audioSource.isPlaying)
        {
            float startVolume = audioSource.volume;
            while (audioSource.volume > 0)
            {
                audioSource.volume -= startVolume * (Time.deltaTime / fadeDuration);
                yield return null; // Чекаємо наступного кадру
            }
            audioSource.Stop();
        }

        // 2. ОНОВЛЕННЯ ДАНИХ ПЛЕЙЛИСТА
        playlist = new List<AudioClip>(newTracks);
        shuffleMode = shuffle;
        currentTrackIndex = 0;
        GeneratePlayOrder();

        // Підкидаємо новий перший трек із нульовою гучністю
        int trackToPlay = playOrder[currentTrackIndex];
        audioSource.clip = playlist[trackToPlay];
        audioSource.volume = 0f;
        audioSource.Play();

        Debug.Log($"[PlaylistManager] Плавне завантаження нової зони. Зараз грає: {playlist[trackToPlay].name}");

        // 3. FADE IN: Плавно виводимо гучність нового треку на максимум
        while (audioSource.volume < maxVolume)
        {
            audioSource.volume += maxVolume * (Time.deltaTime / fadeDuration);
            yield return null;
        }

        // Жорстко фіксуємо фінальну гучність та очищаємо посилання на корутину
        audioSource.volume = maxVolume;
        fadeCoroutine = null;
    }
}