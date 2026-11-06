using System.Collections.Generic;
using UnityEngine;

// All musik och alla ljudeffekter. Skapas automatiskt från Resources/GameAudio
// och lever genom alla scener, så musiken inte börjar om vid scenbyte.
// Byt ljud och volym på prefaben Assets/Resources/GameAudio.
public class GameAudio : MonoBehaviour
{
    [Header("Bakgrund (spelas i loop)")]
    public AudioClip music;
    [Range(0f, 1f)] public float musicVolume = 0.3f;
    public AudioClip ocean;
    [Range(0f, 1f)] public float oceanVolume = 1f;

    [Header("Effekter")]
    public AudioClip cannonShoot;
    [Range(0f, 1f)] public float cannonVolume = 0.5f;
    public AudioClip explosion;
    [Range(0f, 1f)] public float explosionVolume = 0.3f;
    public AudioClip playerDeath;
    [Range(0f, 1f)] public float playerDeathVolume = 1f;
    public AudioClip mermaidShoot;
    [Range(0f, 5f)] public float mermaidShootVolume = 5f;
    public AudioClip bossIntro;
    [Range(0f, 1f)] public float bossIntroVolume = 0.25f;
    public AudioClip bossAttack;
    [Range(0f, 1f)] public float bossAttackVolume = 1f;

    [Header("Överlapp")]
    [Tooltip("Samma effekt spelas inte oftare än så här (sekunder)")]
    public float minRepeatDelay = 0.08f;
    [Tooltip("Sjöjungfruljudet är långt, så det får vänta längre mellan gångerna")]
    public float mermaidRepeatDelay = 0.6f;

    static GameAudio instance;

    readonly Dictionary<AudioClip, float> nextAllowed = new Dictionary<AudioClip, float>();
    AudioSource effects;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Create()
    {
        GameAudio prefab = Resources.Load<GameAudio>("GameAudio");
        if (prefab == null) return;

        instance = Instantiate(prefab);
        instance.name = "GameAudio";
        DontDestroyOnLoad(instance.gameObject);
    }

    void Awake()
    {
        effects = gameObject.AddComponent<AudioSource>();
        effects.playOnAwake = false;
    }

    void Start()
    {
        StartLoop(music, musicVolume);
        StartLoop(ocean, oceanVolume);
    }

    void StartLoop(AudioClip clip, float volume)
    {
        if (clip == null) return;

        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.loop = true;
        source.Play();
    }

    void Play(AudioClip clip, float volume, float repeatDelay)
    {
        if (clip == null) return;

        // Oskalad tid, så att ljuden fungerar även när spelet är pausat
        float now = Time.unscaledTime;
        if (nextAllowed.TryGetValue(clip, out float allowedAt) && now < allowedAt) return;

        nextAllowed[clip] = now + repeatDelay;
        effects.PlayOneShot(clip, volume);
    }

    public static void PlayCannon()
    {
        if (instance != null) instance.Play(instance.cannonShoot, instance.cannonVolume, instance.minRepeatDelay);
    }

    public static void PlayExplosion()
    {
        if (instance != null) instance.Play(instance.explosion, instance.explosionVolume, instance.minRepeatDelay);
    }

    public static void PlayPlayerDeath()
    {
        if (instance != null) instance.Play(instance.playerDeath, instance.playerDeathVolume, instance.minRepeatDelay);
    }

    public static void PlayMermaidShoot()
    {
        if (instance != null) instance.Play(instance.mermaidShoot, instance.mermaidShootVolume, instance.mermaidRepeatDelay);
    }

    public static void PlayBossIntro()
    {
        if (instance != null) instance.Play(instance.bossIntro, instance.bossIntroVolume, instance.minRepeatDelay);
    }

    public static void PlayBossAttack()
    {
        if (instance != null) instance.Play(instance.bossAttack, instance.bossAttackVolume, instance.minRepeatDelay);
    }
}
