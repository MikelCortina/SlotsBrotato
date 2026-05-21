using System.Collections;
using System.Collections.Generic;
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

    [Header("Activación manual UI")]
    [Tooltip("Botón o tecla que activa los símbolos pendientes")]
    public KeyCode activateKey = KeyCode.Space;
    public GameObject pendingIndicator;

    [Header("Spin Config")]
    public float reelSpinDuration = 1.2f;
    public float reelStaggerDelay = 0.18f;

    [Header("Wave Visuals")]
    [Tooltip("Overlay que tapa los reels durante la oleada hasta que empieza a girar")]
    public Image waveCover;

    // ?? Estado interno ????????????????????????????????????????????????
    float _charge;
    bool _spinning;
    bool _hasPendingSymbols;
    bool _pendingIsJackpot;

    readonly List<(int reelIndex, SlotSymbolData data)> _pendingSymbols = new();

    public static SlotMachine Instance { get; private set; }
    Transform _playerTransform;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            _playerTransform = playerObj.transform;
        else
            Debug.LogError("[SlotMachine] No se encontró ningún GameObject con tag 'Player'");
    }

    void Start()
    {
        _charge = 0f;
        _hasPendingSymbols = false;

        if (flashOverlay) flashOverlay.SetActive(false);
        if (pendingIndicator) pendingIndicator.SetActive(false);
        if (waveCover) waveCover.gameObject.SetActive(false);

        // Asegurar que todos los reels empiecen con lockImage activo (cubiertos)
        if (reels != null)
        {
            foreach (var reel in reels)
            {
                if (reel != null)
                    reel.ForceShowLock();
            }
        }

        UpdateChargeUI();
    }

    void Update()
    {
        if (_hasPendingSymbols && Input.GetKeyDown(activateKey))
        {
            ActivatePendingSymbols();
            return;
        }

        if (_hasPendingSymbols || _spinning) return;

        if (_charge >= maxCharge)
        {
            _charge = maxCharge;
            StartCoroutine(DoSpin());
        }
    }

    public void OnEnemyDamaged(float amount)
    {
        if (_spinning || _hasPendingSymbols) return;
        _charge = Mathf.Min(maxCharge, _charge + chargePerDamage);
        UpdateChargeUI();
    }

    public void OnPlayerHit()
    {
        _charge = 0f;
        UpdateChargeUI();
    }

    void UpdateChargeUI()
    {
        if (timerText)
            timerText.text = $"{Mathf.Ceil(_charge)}/{maxCharge}";
        if (timerBarFill)
            timerBarFill.fillAmount = maxCharge > 0f ? _charge / maxCharge : 0f;
    }

    IEnumerator DoSpin()
    {
        _spinning = true;

        if (waveCover) waveCover.gameObject.SetActive(true);

        for (int i = 0; i < reels.Length; i++)
        {
            float stopDelay = reelSpinDuration + i * reelStaggerDelay;
            reels[i].StartSpin(stopDelay);
        }

        float totalDuration = reelSpinDuration + (reels.Length - 1) * reelStaggerDelay + 0.1f;
        yield return new WaitForSeconds(totalDuration);

        CollectResults();

        _charge = 0f;
        UpdateChargeUI();
        _spinning = false;
    }

    void CollectResults()
    {
        _pendingSymbols.Clear();

        for (int i = 0; i < reels.Length; i++)
        {
            var reel = reels[i];
            if (reel == null || reel.CurrentSymbol == null) continue;

            _pendingSymbols.Add((i, reel.CurrentSymbol));
        }

        if (_pendingSymbols.Count == 0) return;

        _pendingIsJackpot = IsJackpot(_pendingSymbols);

        _hasPendingSymbols = true;
        if (pendingIndicator) pendingIndicator.SetActive(true);

        if (_pendingIsJackpot)
            StartCoroutine(WinFlash());
    }

    bool IsJackpot(List<(int reelIndex, SlotSymbolData data)> symbols)
    {
        if (symbols.Count < 2) return false;
        var first = symbols[0].data.symbolType;
        for (int i = 1; i < symbols.Count; i++)
            if (symbols[i].data.symbolType != first) return false;
        return true;
    }

    void ActivatePendingSymbols()
    {
        if (!_hasPendingSymbols || _pendingSymbols.Count == 0) return;

        if (_pendingIsJackpot)
        {
            foreach (var p in _pendingSymbols)
            {
                ApplyByType(p.data.symbolType, 1);

                if (reels != null && p.reelIndex >= 0 && p.reelIndex < reels.Length)
                    reels[p.reelIndex].ShowLock();
            }
            _pendingSymbols.Clear();
        }
        else
        {
            var p = _pendingSymbols[0];
            _pendingSymbols.RemoveAt(0);

            ApplyByType(p.data.symbolType, 1);

            if (reels != null && p.reelIndex >= 0 && p.reelIndex < reels.Length)
                reels[p.reelIndex].ShowLock();
        }

        if (_pendingSymbols.Count == 0)
        {
            _hasPendingSymbols = false;
            _pendingIsJackpot = false;
            if (pendingIndicator) pendingIndicator.SetActive(false);
        }
    }

    void ApplyByType(SlotSymbolType type, int amount)
    {
        switch (type)
        {
            case SlotSymbolType.Shield: ApplyShield(amount); break;
            case SlotSymbolType.Coin: ApplyCoins(amount); break;
            case SlotSymbolType.Static: ApplyStatik(amount); break;
        }
    }

    void ApplyShield(int amount)
    {
        int shieldAmount = amount >= 3 ? 10 : amount;
        var playerShield = FindFirstObjectByType<PlayerShield>();
        if (playerShield != null) playerShield.AddShield(shieldAmount);
    }

    void ApplyCoins(int amount)
    {
        int coinAmount = amount >= 3 ? 10 : amount;
        if (PlayerWallet.Instance != null)
            PlayerWallet.Instance.AddCoins(coinAmount);
    }

    void ApplyStatik(int amount)
    {
        int chains = amount >= 3 ? 10 : amount * 3;
        if (ChainLightning.Instance != null && _playerTransform != null)
            ChainLightning.Instance.Trigger(_playerTransform, chains, 1000f, 20f);
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