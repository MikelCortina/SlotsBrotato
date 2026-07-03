using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotReel : MonoBehaviour
{
    [Header("Símbolos")]
    public SlotSymbolData[] currentSymbols;
    public Image displayImage;
    public Image topBlur;
    public Image botBlur;
    public TextMeshProUGUI symbolLevelText;

    [Header("Lock Visual")]
    public GameObject lockObject;

    public SlotSymbolData CurrentSymbol { get; private set; }

    [Header("Animación")]
    public float fastScrollInterval = 0.055f;
    public float slowScrollInterval = 0.12f;

    [Header("Activation Visual")]
    public float activationPunchScale = 1.28f;
    public float activationPunchDuration = 0.16f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip reelTickClip;
    [Range(0f, 1f)] public float reelTickVolume = 0.4f;
    public AudioClip reelStopClip;
    [Range(0f, 1f)] public float reelStopVolume = 0.6f;
    public float pitchMin = 0.96f;
    public float pitchMax = 1.04f;

    private bool _spinning;
    private int _displayIndex;
    private Coroutine _scaleRoutine;

    void Awake()
    {
        BuildSymbolPool();
        SetInitialSymbol();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void BuildSymbolPool()
    {
        if (RunConfig.Instance == null) return;
        currentSymbols = RunConfig.Instance.selectedSymbols.ToArray();
    }

    void SetInitialSymbol()
    {
        if (currentSymbols == null || currentSymbols.Length == 0)
        {
            Debug.LogWarning($"[SlotReel] {gameObject.name} no tiene símbolos seleccionados.");
            return;
        }

        _displayIndex = Random.Range(0, currentSymbols.Length);
        CurrentSymbol = currentSymbols[_displayIndex];

        if (displayImage)
            UpdateSymbolVisual();
    }

    public void StartSpin(float stopAfter)
    {
        if (_spinning) return;

        ResetVisualState();
        BuildSymbolPool();

        if (currentSymbols == null || currentSymbols.Length == 0)
        {
            Debug.LogWarning($"[SlotReel] {gameObject.name} no puede girar porque no hay símbolos.");
            return;
        }

        StartCoroutine(SpinRoutine(stopAfter));
    }

    IEnumerator SpinRoutine(float stopAfter)
    {
        _spinning = true;
        SetBlurVisible(true);

        float elapsed = 0f;
        float interval = fastScrollInterval;
        int result = Random.Range(0, currentSymbols.Length);

        while (elapsed < stopAfter)
        {
            float remaining = stopAfter - elapsed;

            if (remaining < 0.35f)
                interval = Mathf.Lerp(fastScrollInterval, slowScrollInterval, 1f - remaining / 0.35f);
            else
                interval = fastScrollInterval;

            _displayIndex = (_displayIndex + 1) % currentSymbols.Length;

            if (displayImage)
                displayImage.sprite = currentSymbols[_displayIndex].icon;

            PlayReelTick();

            if (displayImage)
                PlayScalePunch(1.08f, 0.07f);

            elapsed += interval;
            yield return new WaitForSeconds(interval);
        }

        _displayIndex = result;
        CurrentSymbol = currentSymbols[result];

        if (displayImage)
            UpdateSymbolVisual();

        SetBlurVisible(false);
        _spinning = false;

        PlayReelStop();

        if (displayImage)
            PlayScalePunch(1.18f, 0.12f);
    }

    void PlayReelTick()
    {
        if (audioSource == null || reelTickClip == null) return;

        audioSource.pitch = Random.Range(pitchMin, pitchMax);
        audioSource.PlayOneShot(reelTickClip, reelTickVolume);
    }

    void PlayReelStop()
    {
        if (audioSource == null || reelStopClip == null) return;

        audioSource.pitch = 1f;
        audioSource.PlayOneShot(reelStopClip, reelStopVolume);
    }

    void SetBlurVisible(bool visible)
    {
        if (topBlur) topBlur.gameObject.SetActive(visible);
        if (botBlur) botBlur.gameObject.SetActive(visible);
    }

    public void PlayActivationPunch()
    {
        if (displayImage == null) return;
        PlayScalePunch(activationPunchScale, activationPunchDuration);
    }

    void PlayScalePunch(float peakScale, float duration)
    {
        if (displayImage == null) return;

        if (_scaleRoutine != null)
            StopCoroutine(_scaleRoutine);

        _scaleRoutine = StartCoroutine(PunchScaleRoutine(displayImage.transform, peakScale, duration));
    }

    IEnumerator PunchScaleRoutine(Transform t, float peakScale, float duration)
    {
        float half = duration * 0.5f;
        float elapsed = 0f;

        t.localScale = Vector3.one;

        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float s = Mathf.Lerp(1f, peakScale, elapsed / half);
            t.localScale = Vector3.one * s;
            yield return null;
        }

        elapsed = 0f;

        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float s = Mathf.Lerp(peakScale, 1f, elapsed / half);
            t.localScale = Vector3.one * s;
            yield return null;
        }

        t.localScale = Vector3.one;
        _scaleRoutine = null;
    }

    public void ResetVisualState()
    {
        if (_scaleRoutine != null)
        {
            StopCoroutine(_scaleRoutine);
            _scaleRoutine = null;
        }

        if (displayImage != null)
            displayImage.transform.localScale = Vector3.one;
    }

    void UpdateSymbolVisual()
    {
        if (CurrentSymbol == null) return;

        if (displayImage)
            displayImage.sprite = CurrentSymbol.icon;

        if (symbolLevelText != null && RunConfig.Instance != null)
        {
            int level = RunConfig.Instance.GetSymbolLevel(CurrentSymbol.symbolType);
            symbolLevelText.text = $"Lv.{level}";
        }
    }

    public void ForceShowLock()
    {
        ShowLock();
    }

    public void ShowLock()
    {
        if (lockObject != null)
            lockObject.SetActive(true);
    }

    public void HideLock()
    {
        if (lockObject != null)
            lockObject.SetActive(false);
    }
}