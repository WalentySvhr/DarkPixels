using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class ContinuousPlaylist : MonoBehaviour
{
    public static ContinuousPlaylist instance;

    [Header("Налаштування мікшера")]
    public AudioMixerGroup musicOutput;

    [Header("Плейлист за замовчуванням")]
    public List<AudioClip> playlist = new List<AudioClip>();
    public bool shuffleMode = false;

    [Header("Налаштування плавного переходу")]
    [Range(0.1f, 3f)] public float fadeDuration = 1.0f;

    private AudioSource audioSource;
    private int currentTrackIndex = 0;
    private List<int> playOrder = new List<int>(); // Черга програвання
    private float silenceTimer = 0f;

    private float sceneTransitionCooldown = 0f;
    private Coroutine fadeCoroutine;
    private float maxVolume = 1f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

            if (musicOutput != null)
                audioSource.outputAudioMixerGroup = musicOutput;

            audioSource.loop = false;
            maxVolume = audioSource.volume;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (instance != this) return;
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
        // ЗАХИСТ: Якщо списки порожні або індекс вилетів за межі черги — перестраховуємось
        if (playlist.Count == 0 || playOrder.Count == 0) return;

        if (currentTrackIndex >= playOrder.Count) currentTrackIndex = 0;

        int trackToPlay = playOrder[currentTrackIndex];

        // Додаткова перевірка на випадок мікро-лагів при заміні списків
        if (trackToPlay >= playlist.Count) return;

        audioSource.clip = playlist[trackToPlay];
        audioSource.volume = maxVolume;
        audioSource.Play();

        Debug.Log($"<color=cyan>[Playlist] Зараз грає: {playlist[trackToPlay].name} (Трек {currentTrackIndex + 1} із {playlist.Count})</color>");
    }

    public void PlayNextTrack()
    {
        currentTrackIndex++;

        // ІСПРАВЛЕНО: Перевіряємо межі саме по playOrder.Count, а не по playlist.Count
        if (currentTrackIndex >= playOrder.Count)
        {
            currentTrackIndex = 0;
            if (shuffleMode) GeneratePlayOrder();
        }
        PlayCurrentTrack();
    }

    public void ChangeZonePlaylist(List<AudioClip> newTracks, bool shuffle)
    {
        if (newTracks == null || newTracks.Count == 0 || audioSource == null) return;

        // Перевірка: чи не намагаємося ми увімкнути той самий плейлист, який вже грає?
        if (playlist.Count == newTracks.Count && playlist.Count > 0 && playlist[0] == newTracks[0])
        {
            if (!audioSource.isPlaying) audioSource.Play();
            return;
        }

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeZonePlaylistRoutine(newTracks, shuffle));
    }

    private IEnumerator FadeZonePlaylistRoutine(List<AudioClip> newTracks, bool shuffle)
    {
        // 1. FADE OUT
        if (audioSource.isPlaying)
        {
            float startVolume = audioSource.volume;
            while (audioSource.volume > 0)
            {
                audioSource.volume -= startVolume * (Time.deltaTime / fadeDuration);
                yield return null;
            }
            audioSource.Stop();
        }

        // 2. ОНОВЛЕННЯ ДАНИХ
        playlist = new List<AudioClip>(newTracks);
        shuffleMode = shuffle;
        currentTrackIndex = 0;
        GeneratePlayOrder();

        // Перевіряємо, чи успішно згенерована черга
        if (playOrder.Count > 0)
        {
            int trackToPlay = playOrder[currentTrackIndex];
            audioSource.clip = playlist[trackToPlay];
            audioSource.volume = 0f;
            audioSource.Play();

            Debug.Log($"<color=green>[PlaylistManager] Нова зона активована. Початковий трек: {playlist[trackToPlay].name}</color>");
        }

        // 3. FADE IN
        while (audioSource.volume < maxVolume)
        {
            audioSource.volume += maxVolume * (Time.deltaTime / fadeDuration);
            yield return null;
        }

        audioSource.volume = maxVolume;
        fadeCoroutine = null;
    }
}