using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SlotReel : MonoBehaviour
{
    [Header("Símbolos")]
    public SlotSymbolData[] currentSymbols;
    public Image displayImage;
    public Image topBlur;
    public Image botBlur;

    public SlotSymbolData CurrentSymbol { get; private set; }

    [Header("Animación")]
    public float fastScrollInterval = 0.055f;
    public float slowScrollInterval = 0.12f;

    [Header("Lock Visual")]
    [Tooltip("Imagen que se muestra cuando el símbolo se activa (cubre el slot)")]
    public Image lockImage;

    [Header("Activation Visual")]
    public float activationPunchScale = 1.28f;
    public float activationPunchDuration = 0.16f;

    private bool _spinning;
    private int _displayIndex;
    private Coroutine _scaleRoutine;

    void Awake()
    {
        BuildSymbolPool();
        SetInitialSymbol();

        if (lockImage != null)
            lockImage.gameObject.SetActive(false);
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
            displayImage.sprite = CurrentSymbol.icon;
    }

    public void StartSpin(float stopAfter)
    {
        if (_spinning) return;

        HideLock();
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

            if (displayImage)
                PlayScalePunch(1.08f, 0.07f);

            elapsed += interval;
            yield return new WaitForSeconds(interval);
        }

        _displayIndex = result;
        CurrentSymbol = currentSymbols[result];

        if (displayImage)
            displayImage.sprite = CurrentSymbol.icon;

        SetBlurVisible(false);
        _spinning = false;

        if (displayImage)
            PlayScalePunch(1.18f, 0.12f);
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
        if (displayImage != null)
            displayImage.transform.localScale = Vector3.one;

        if (_scaleRoutine != null)
        {
            StopCoroutine(_scaleRoutine);
            _scaleRoutine = null;
        }
    }

    public void ForceShowLock()
    {
        if (lockImage != null)
            lockImage.gameObject.SetActive(true);
    }

    public void ShowLock()
    {
        if (lockImage != null)
            lockImage.gameObject.SetActive(true);
    }

    public void HideLock()
    {
        if (lockImage != null)
            lockImage.gameObject.SetActive(false);
    }
}