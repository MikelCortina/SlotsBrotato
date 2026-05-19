using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotMachine : MonoBehaviour
{
    [Header("Timer")]
    public float spinInterval = 10f;  // segundos entre spin
    public float timerReduceAmount = 1f;  // segundos que quita cada kill

    [Header("Referencias UI")]
    public SlotReel[] reels;              // array de 3 SlotReel
    public TextMeshProUGUI timerText;
    public Image timerBarFill;
    public GameObject flashOverlay;       // panel semitransparente que flashea al ganar

    [Header("Spin Config")]
    public float reelSpinDuration = 1.2f;
    public float reelStaggerDelay = 0.18f; // cada rueda para un poco después

    // ?? Privado ??????????????????????????????????????????????????????
    private float _timer;
    private bool _spinning;

    public static SlotMachine Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _timer = spinInterval;
        if (flashOverlay) flashOverlay.SetActive(false);
    }

    void Update()
    {
        if (_spinning) return;

        _timer -= Time.deltaTime;
        UpdateTimerUI();

        if (_timer <= 0f)
        {
            _timer = 0f;
            StartCoroutine(DoSpin());
        }
    }

    // Llamado por GameManager cuando muere un enemigo
    public void OnEnemyKilled()
    {
        if (_spinning) return;
        _timer = Mathf.Max(0f, _timer - timerReduceAmount);
        UpdateTimerUI();
    }

    // ?? UI Timer ?????????????????????????????????????????????????????

    void UpdateTimerUI()
    {
        if (timerText)
            timerText.text = Mathf.Ceil(_timer).ToString("0") + "s";

        if (timerBarFill)
            timerBarFill.fillAmount = _timer / spinInterval;
    }

    // ?? Spin ?????????????????????????????????????????????????????????

    IEnumerator DoSpin()
    {
        _spinning = true;

        // Lanzar las 3 ruedas con stagger
        for (int i = 0; i < reels.Length; i++)
        {
            float stopDelay = reelSpinDuration + i * reelStaggerDelay;
            reels[i].StartSpin(stopDelay);
        }

        // Esperar a que paren todas
        float totalDuration = reelSpinDuration + (reels.Length - 1) * reelStaggerDelay + 0.1f;
        yield return new WaitForSeconds(totalDuration);

        // Comprobar resultado
        CheckResult();

        // Pausa breve antes del siguiente ciclo
        yield return new WaitForSeconds(0.6f);

        _timer = spinInterval;
        _spinning = false;
    }

    void CheckResult()
    {
        if (reels.Length < 2) return;

        bool allMatch = true;
        int firstId = reels[0].CurrentSymbolId;
        for (int i = 1; i < reels.Length; i++)
        {
            if (reels[i].CurrentSymbolId != firstId) { allMatch = false; break; }
        }

        if (allMatch) StartCoroutine(WinFlash());
    }

    IEnumerator WinFlash()
    {
        if (flashOverlay == null) yield break;
        flashOverlay.SetActive(true);
        yield return new WaitForSeconds(0.12f);
        flashOverlay.SetActive(false);
        yield return new WaitForSeconds(0.08f);
        flashOverlay.SetActive(true);
        yield return new WaitForSeconds(0.12f);
        flashOverlay.SetActive(false);
    }
}