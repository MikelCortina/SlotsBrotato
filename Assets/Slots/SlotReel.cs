using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotReel : MonoBehaviour
{


    [Header("Símbolos")]
    public SlotSymbolType[] symbolTypes;
    public Sprite[] symbols;          // asigna tus sprites en orden en el Inspector
    public Image displayImage;     // la Image central que muestra el símbolo actual
    public Image topBlur;          // Image semitransparente arriba (efecto de blur/fade)
    public Image botBlur;          // Image semitransparente abajo
    public SlotSymbolType CurrentSymbolType { get; private set; }

    [Header("Animación")]
    public float fastScrollInterval = 0.055f;  // segundos entre cada tick mientras gira
    public float slowScrollInterval = 0.12f;   // se ralentiza al frenar

    public int CurrentSymbolId { get; private set; }

    private bool _spinning;
    private int _displayIndex;

    void Awake()
    {
        if (symbols == null || symbols.Length == 0)
        {
            Debug.LogWarning($"[SlotReel] {gameObject.name} no tiene símbolos asignados.");
            return;
        }
        _displayIndex = Random.Range(0, symbols.Length);
        CurrentSymbolId = _displayIndex;
        CurrentSymbolType = symbolTypes[_displayIndex];
        displayImage.sprite = symbols[_displayIndex];
    }

    // Llamado por SlotMachine
    public void StartSpin(float stopAfter)
    {
        if (_spinning) return;
        StartCoroutine(SpinRoutine(stopAfter));
    }

    IEnumerator SpinRoutine(float stopAfter)
    {
        if (symbols == null || symbols.Length == 0) yield break; // guard
        _spinning = true;
        SetBlurVisible(true);

        float elapsed = 0f;
        float interval = fastScrollInterval;

        // Decidir resultado ANTES de animar (así siempre es consistente)
        int result = Random.Range(0, symbols.Length);

        while (elapsed < stopAfter)
        {
            // Ralentizar en el tramo final
            float remaining = stopAfter - elapsed;
            if (remaining < 0.35f)
                interval = Mathf.Lerp(fastScrollInterval, slowScrollInterval, 1f - remaining / 0.35f);
            else
                interval = fastScrollInterval;

            // Avanzar un símbolo
            _displayIndex = (_displayIndex + 1) % symbols.Length;
            displayImage.sprite = symbols[_displayIndex];

            // Pequeño punch de escala
            StartCoroutine(PunchScale(displayImage.transform));

            elapsed += interval;
            yield return new WaitForSeconds(interval);
        }

        // Forzar símbolo final
        _displayIndex = result;
        CurrentSymbolId = result;
        CurrentSymbolType = symbolTypes[result];
        displayImage.sprite = symbols[result];

        SetBlurVisible(false);
        _spinning = false;

        // Punch final más grande
        StartCoroutine(PunchScale(displayImage.transform, 1.22f));
    }

    // ?? Helpers visuales ?????????????????????????????????????????????

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
}