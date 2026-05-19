using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotMachine : MonoBehaviour
{
    [Header("Combo Charge")]
    public float maxCharge = 10f;
    public float chargePerDamage = 1f;

    [Header("Referencias UI")]
    public SlotReel[] reels;
    public TextMeshProUGUI timerText;
    public Image timerBarFill;
    public GameObject flashOverlay;

    [Header("Spin Config")]
    public float reelSpinDuration = 1.2f;
    public float reelStaggerDelay = 0.18f;

    private float _charge;
    private bool _spinning;

    public static SlotMachine Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _charge = 0f;
        if (flashOverlay) flashOverlay.SetActive(false);
        UpdateTimerUI();
    }

    void Update()
    {
        if (_spinning) return;

        if (_charge >= maxCharge)
        {
            _charge = maxCharge;
            StartCoroutine(DoSpin());
        }
    }

    public void OnEnemyDamaged(float amount)
    {
        if (_spinning) return;

        _charge = Mathf.Min(maxCharge, _charge + chargePerDamage);
        UpdateTimerUI();
    }

    public void OnPlayerHit()
    {
        _charge = 0f;
        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        if (timerText)
            timerText.text = $"{Mathf.Ceil(_charge)}/{maxCharge}";

        if (timerBarFill)
            timerBarFill.fillAmount = maxCharge > 0f ? _charge / maxCharge : 0f;
    }

    IEnumerator DoSpin()
    {
        _spinning = true;

        for (int i = 0; i < reels.Length; i++)
        {
            float stopDelay = reelSpinDuration + i * reelStaggerDelay;
            reels[i].StartSpin(stopDelay);
        }

        float totalDuration = reelSpinDuration + (reels.Length - 1) * reelStaggerDelay + 0.1f;
        yield return new WaitForSeconds(totalDuration);

        CheckResult();

        yield return new WaitForSeconds(0.6f);

        _charge = 0f;
        UpdateTimerUI();
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