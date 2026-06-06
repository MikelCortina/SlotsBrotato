using System.Collections.Generic;
using UnityEngine;

public enum SfxPriority
{
    Critical = 0,
    High = 32,
    Medium = 96,
    Low = 160,
    VeryLow = 220
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private int sfxPoolSize = 12;
    [SerializeField] private AudioSource musicSourcePrefab;
    [SerializeField] private AudioSource sfxSourcePrefab;

    [Header("Limits")]
    [SerializeField] private int maxSimultaneousSfx = 10;
    [SerializeField] private float defaultSameEventCooldown = 0.04f;
    [SerializeField] private int maxSameClipVoices = 2;
    [SerializeField] private float sameClipWindow = 0.12f;

    [Header("Volumes")]
    [SerializeField] private float masterSfxVolume = 1f;

    private readonly List<AudioSource> _sfxPool = new();
    private readonly Dictionary<string, float> _eventCooldowns = new();
    private readonly Dictionary<AudioClip, List<float>> _clipPlayTimes = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildPool();
    }

    private void BuildPool()
    {
        if (sfxSourcePrefab == null)
        {
            GameObject template = new GameObject("SFX_Source_Template");
            template.transform.SetParent(transform);
            AudioSource fallback = template.AddComponent<AudioSource>();
            fallback.playOnAwake = false;
            fallback.loop = false;
            fallback.spatialBlend = 0f;
            fallback.volume = 1f;
            fallback.priority = (int)SfxPriority.Medium;
            sfxSourcePrefab = fallback;
        }

        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource src = Instantiate(sfxSourcePrefab, transform);
            src.name = $"SFX_Source_{i}";
            src.playOnAwake = false;
            src.loop = false;
            src.spatialBlend = 0f;
            _sfxPool.Add(src);
        }
    }

    public void PlaySFX(
        AudioClip clip,
        string eventKey = null,
        float volume = 1f,
        SfxPriority priority = SfxPriority.Medium,
        float cooldown = -1f,
        int maxVoicesForThisClip = -1)
    {
        if (clip == null) return;

        float now = Time.unscaledTime;
        eventKey ??= clip.name;
        if (cooldown < 0f) cooldown = defaultSameEventCooldown;
        if (maxVoicesForThisClip < 0) maxVoicesForThisClip = maxSameClipVoices;

        if (IsEventOnCooldown(eventKey, now, cooldown))
            return;

        if (WouldExceedClipLimit(clip, now, maxVoicesForThisClip))
            return;

        AudioSource source = GetBestAvailableSource((int)priority);

        if (source == null)
            return;

        source.priority = (int)priority;
        source.volume = masterSfxVolume;
        source.pitch = 1f;
        source.spatialBlend = 0f;
        source.clip = null;
        source.Stop();

        source.PlayOneShot(clip, volume);

        _eventCooldowns[eventKey] = now;
        RegisterClipPlay(clip, now);
    }

    private bool IsEventOnCooldown(string eventKey, float now, float cooldown)
    {
        if (_eventCooldowns.TryGetValue(eventKey, out float lastTime))
        {
            if (now - lastTime < cooldown)
                return true;
        }

        return false;
    }

    private bool WouldExceedClipLimit(AudioClip clip, float now, int maxVoicesForThisClip)
    {
        if (!_clipPlayTimes.TryGetValue(clip, out List<float> times))
        {
            times = new List<float>();
            _clipPlayTimes[clip] = times;
        }

        for (int i = times.Count - 1; i >= 0; i--)
        {
            if (now - times[i] > sameClipWindow)
                times.RemoveAt(i);
        }

        return times.Count >= maxVoicesForThisClip;
    }

    private void RegisterClipPlay(AudioClip clip, float now)
    {
        if (!_clipPlayTimes.TryGetValue(clip, out List<float> times))
        {
            times = new List<float>();
            _clipPlayTimes[clip] = times;
        }

        times.Add(now);
    }

    private AudioSource GetBestAvailableSource(int requestedPriority)
    {
        int playingCount = 0;

        for (int i = 0; i < _sfxPool.Count; i++)
        {
            if (_sfxPool[i].isPlaying)
                playingCount++;
        }

        for (int i = 0; i < _sfxPool.Count; i++)
        {
            if (!_sfxPool[i].isPlaying)
                return _sfxPool[i];
        }

        if (playingCount < maxSimultaneousSfx)
            return null;

        AudioSource worstSource = null;
        int worstPriority = int.MinValue;

        for (int i = 0; i < _sfxPool.Count; i++)
        {
            AudioSource current = _sfxPool[i];

            if (current.priority > worstPriority)
            {
                worstPriority = current.priority;
                worstSource = current;
            }
        }

        if (worstSource != null && requestedPriority < worstSource.priority)
            return worstSource;

        return null;
    }

    public void SetSfxVolume(float value)
    {
        masterSfxVolume = Mathf.Clamp01(value);
    }
}