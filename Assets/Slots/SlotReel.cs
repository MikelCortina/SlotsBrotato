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

    private bool _spinning;
    private int _displayIndex;
    private Sprite _originalSprite;

    void Awake()
    {
        BuildSymbolPool();
        SetInitialSymbol();

        if (displayImage != null)
            _originalSprite = displayImage.sprite;

        // Inicializar lockImage INACTIVO (se activará en Start de SlotMachine)
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

        // Ocultar lockImage cuando el reel empieza a girar
        HideLock();

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
                StartCoroutine(PunchScale(displayImage.transform));

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
            StartCoroutine(PunchScale(displayImage.transform, 1.22f));
    }

    void SetBlurVisible(bool visible)
    {
        if (topBlur) topBlur.gameObject.SetActive(visible);
        if (botBlur) botBlur.gameObject.SetActive(visible);
    }

    IEnumerator PunchScale(Transform t, float peakScale = 1.1f)
    {
        float duration = 0.07f;
        float half = duration / 2f;
        float elapsed = 0f;

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
    }

    // ── Métodos de bloqueo ────────────────────────────────────────────

    /// <summary>
    /// Muestra lockImage (forzado, sin importar estado anterior)
    /// </summary>
    public void ForceShowLock()
    {
        if (lockImage != null)
            lockImage.gameObject.SetActive(true);
    }

    /// <summary>
    /// Muestra lockImage cubriendo el slot (cuando el símbolo se activa)
    /// </summary>
    public void ShowLock()
    {
        if (lockImage != null)
            lockImage.gameObject.SetActive(true);
    }

    /// <summary>
    /// Oculta lockImage y vuelve a mostrar el símbolo (cuando el reel empieza a girar)
    /// </summary>
    public void HideLock()
    {
        if (lockImage != null)
            lockImage.gameObject.SetActive(false);
    }
}