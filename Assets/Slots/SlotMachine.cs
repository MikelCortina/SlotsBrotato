using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotMachine : MonoBehaviour
{
    [Header("Charge")]
    public float maxCharge = 10f;
    public float maxOverloadReserve = 5f;
    public float chargePerCoin = 1f;

    [Header("UI")]
    public SlotReel[] reels;
    public TextMeshProUGUI timerText;
    public Image timerBarFill;
    public Image overloadBarFill;
    public GameObject flashOverlay;
    public GameObject pendingIndicator;

    [Header("Manual Activation")]
    public KeyCode activateKey = KeyCode.Space;

    [Header("Activation Timing")]
    [Tooltip("Delay entre un símbolo resuelto y el siguiente.")]
    [Min(0f)] public float activationStepDelay = 0.12f;

    [Tooltip("Tiempo que se espera tras el punch antes de aplicar el efecto.")]
    [Min(0f)] public float activationResolveDelay = 0.10f;

    [Header("Spin Config")]
    public float reelSpinDuration = 1.2f;
    public float reelStaggerDelay = 0.18f;

    [Header("Wave Visuals")]
    public Image waveCover;

    float _charge;
    float _overloadReserve;

    bool _spinning;
    bool _spinQueued;
    bool _hasPendingSymbols;
    bool _pendingIsJackpot;
    bool _chargeLockedFull;
    bool _isResolvingActivation;

    readonly List<(int reelIndex, SlotSymbolData data)> _pendingSymbols = new();
    readonly List<(int reelIndex, SlotSymbolData data)> _autoSymbols = new();

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
        _overloadReserve = 0f;
        _spinning = false;
        _spinQueued = false;
        _hasPendingSymbols = false;
        _pendingIsJackpot = false;
        _chargeLockedFull = false;
        _isResolvingActivation = false;

        if (flashOverlay) flashOverlay.SetActive(false);
        if (pendingIndicator) pendingIndicator.SetActive(false);
        if (waveCover) waveCover.gameObject.SetActive(false);

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
        if (_hasPendingSymbols && !_isResolvingActivation && Input.GetKeyDown(activateKey))
        {
            StartCoroutine(ActivatePendingSymbolsRoutine());
            return;
        }

        if (_spinning || _chargeLockedFull || _isResolvingActivation)
            return;

        if (!_spinQueued && _charge >= maxCharge)
        {
            _charge = maxCharge;
            _spinQueued = true;
            StartCoroutine(DoSpin());
        }
    }

    public void OnCoinCollected(int amount)
    {
        float gain = amount * chargePerCoin;
        bool currentSpinAlreadyCommitted = _spinQueued || _spinning || _chargeLockedFull;

        if (currentSpinAlreadyCommitted)
            _overloadReserve = Mathf.Min(maxOverloadReserve, _overloadReserve + gain);
        else
            _charge = Mathf.Min(maxCharge, _charge + gain);

        UpdateChargeUI();
    }

    public void OnPlayerHit()
    {
        if (_chargeLockedFull || _spinning || _spinQueued)
        {
            _overloadReserve = 0f;
        }
        else
        {
            _charge = 0f;
            _overloadReserve = 0f;
        }

        UpdateChargeUI();
    }

    void UpdateChargeUI()
    {
        float displayedMainCharge = _chargeLockedFull ? maxCharge : _charge;

        if (timerText)
        {
            if (_chargeLockedFull || _spinning || _spinQueued || _overloadReserve > 0f)
                timerText.text = $"{Mathf.Ceil(_overloadReserve)}/{maxCharge}";
            else
                timerText.text = $"{Mathf.Ceil(displayedMainCharge)}/{maxCharge}";
        }

        if (timerBarFill)
            timerBarFill.fillAmount = maxCharge > 0f ? displayedMainCharge / maxCharge : 0f;

        if (overloadBarFill)
        {
            bool showOverload = _chargeLockedFull || _spinning || _spinQueued || _overloadReserve > 0f;
            overloadBarFill.gameObject.SetActive(showOverload);
            overloadBarFill.fillAmount = maxOverloadReserve > 0f ? _overloadReserve / maxOverloadReserve : 0f;
        }
    }

    IEnumerator DoSpin()
    {
        _spinning = true;
        _spinQueued = false;

        if (waveCover) waveCover.gameObject.SetActive(true);

        for (int i = 0; i < reels.Length; i++)
        {
            float stopDelay = reelSpinDuration + i * reelStaggerDelay;
            reels[i].StartSpin(stopDelay);
        }

        float totalDuration = reelSpinDuration + (reels.Length - 1) * reelStaggerDelay + 0.1f;
        yield return new WaitForSeconds(totalDuration);

        CollectResults();

        if (_autoSymbols.Count > 0)
        {
            _isResolvingActivation = true;
            yield return StartCoroutine(ResolveAutoSymbolsRoutine());
            _isResolvingActivation = false;
        }

        if (!_hasPendingSymbols)
        {
            _charge = 0f;

            if (_overloadReserve > 0f)
            {
                _charge = Mathf.Min(_overloadReserve, maxCharge);
                _overloadReserve = 0f;
            }
        }
        else
        {
            _chargeLockedFull = true;
            _charge = maxCharge;
        }

        UpdateChargeUI();
        _spinning = false;
    }

    void CollectResults()
    {
        _pendingSymbols.Clear();
        _autoSymbols.Clear();

        List<(int reelIndex, SlotSymbolData data)> results = new();

        for (int i = 0; i < reels.Length; i++)
        {
            var reel = reels[i];
            if (reel == null || reel.CurrentSymbol == null) continue;
            results.Add((i, reel.CurrentSymbol));
        }

        if (results.Count == 0) return;

        bool jackpot = IsJackpot(results);

        foreach (var result in results)
        {
            if (result.data.activateInstantly)
                _autoSymbols.Add(result);
            else
                _pendingSymbols.Add(result);
        }

        _autoSymbols.Sort((a, b) => a.reelIndex.CompareTo(b.reelIndex));
        _pendingSymbols.Sort((a, b) => a.reelIndex.CompareTo(b.reelIndex));

        _pendingIsJackpot = jackpot && _pendingSymbols.Count > 0;
        _hasPendingSymbols = _pendingSymbols.Count > 0;

        if (pendingIndicator)
            pendingIndicator.SetActive(_hasPendingSymbols);

        if (jackpot)
            StartCoroutine(WinFlash());
    }

    bool IsJackpot(List<(int reelIndex, SlotSymbolData data)> symbols)
    {
        if (symbols.Count < 2) return false;

        var first = symbols[0].data.symbolType;
        for (int i = 1; i < symbols.Count; i++)
        {
            if (symbols[i].data.symbolType != first)
                return false;
        }

        return true;
    }

    IEnumerator ResolveAutoSymbolsRoutine()
    {
        bool jackpotAuto = _autoSymbols.Count > 0 && AreAllSameType(_autoSymbols);

        for (int i = 0; i < _autoSymbols.Count; i++)
        {
            var symbol = _autoSymbols[i];
            int amount = jackpotAuto ? 3 : 1;

            yield return StartCoroutine(ResolveSingleSymbolVisual(symbol, amount));

            if (i < _autoSymbols.Count - 1 && activationStepDelay > 0f)
                yield return new WaitForSeconds(activationStepDelay);
        }

        _autoSymbols.Clear();
    }

    IEnumerator ActivatePendingSymbolsRoutine()
    {
        if (!_hasPendingSymbols || _pendingSymbols.Count == 0)
            yield break;

        _isResolvingActivation = true;

        if (_pendingIsJackpot)
        {
            for (int i = 0; i < _pendingSymbols.Count; i++)
            {
                var p = _pendingSymbols[i];
                yield return StartCoroutine(ResolveSingleSymbolVisual(p, 3));

                if (i < _pendingSymbols.Count - 1 && activationStepDelay > 0f)
                    yield return new WaitForSeconds(activationStepDelay);
            }

            _pendingSymbols.Clear();
        }
        else
        {
            var p = _pendingSymbols[0];
            yield return StartCoroutine(ResolveSingleSymbolVisual(p, 1));
            _pendingSymbols.RemoveAt(0);
        }

        if (_pendingSymbols.Count == 0)
        {
            _hasPendingSymbols = false;
            _pendingIsJackpot = false;
            _chargeLockedFull = false;

            if (pendingIndicator)
                pendingIndicator.SetActive(false);

            _charge = Mathf.Min(_overloadReserve, maxCharge);
            _overloadReserve = 0f;
        }

        UpdateChargeUI();
        _isResolvingActivation = false;
    }

    IEnumerator ResolveSingleSymbolVisual((int reelIndex, SlotSymbolData data) symbol, int amount)
    {
        if (symbol.reelIndex >= 0 && symbol.reelIndex < reels.Length && reels[symbol.reelIndex] != null)
        {
            var reel = reels[symbol.reelIndex];
            reel.PlayActivationPunch();

            if (activationResolveDelay > 0f)
                yield return new WaitForSeconds(activationResolveDelay);

            ApplyByType(symbol.data.symbolType, amount);
            reel.ShowLock();
        }
        else
        {
            ApplyByType(symbol.data.symbolType, amount);
        }
    }

    bool AreAllSameType(List<(int reelIndex, SlotSymbolData data)> symbols)
    {
        if (symbols.Count == 0) return false;

        var first = symbols[0].data.symbolType;
        for (int i = 1; i < symbols.Count; i++)
        {
            if (symbols[i].data.symbolType != first)
                return false;
        }

        return true;
    }

    void ApplyByType(SlotSymbolType type, int amount)
    {
        switch (type)
        {
            case SlotSymbolType.Shield:
                ApplyShield(amount);
                break;

            case SlotSymbolType.Coin:
                ApplyCoins(amount);
                break;

            case SlotSymbolType.Static:
                ApplyStatik(amount);
                break;
        }
    }

    void ApplyShield(int amount)
    {
        int shieldAmount = amount >= 3 ? 10 : amount;
        var playerShield = FindFirstObjectByType<PlayerShield>();
        if (playerShield != null)
            playerShield.AddShield(shieldAmount);
    }

    void ApplyCoins(int amount)
    {
        int coinAmount = amount >= 3 ? 10 : amount;
        if (PlayerWallet.Instance != null)
            PlayerWallet.Instance.AddCoins(coinAmount);
    }

    void ApplyStatik(int amount)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        int aliveEnemies = 0;
        foreach (var e in enemies)
        {
            var eh = e.GetComponent<EnemyHealth>();
            if (eh != null && eh.currentHealth > 0)
                aliveEnemies++;
        }

        int chains = amount >= 3 ? 10 : amount * 3;
        chains = Mathf.Max(chains, aliveEnemies);

        if (ChainLightning.Instance != null && _playerTransform != null)
            ChainLightning.Instance.Trigger(_playerTransform, chains, float.MaxValue, 20f);
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

    public void SetActivationStepDelay(float newDelay)
    {
        activationStepDelay = Mathf.Max(0f, newDelay);
    }

    public void SetActivationResolveDelay(float newDelay)
    {
        activationResolveDelay = Mathf.Max(0f, newDelay);
    }
}