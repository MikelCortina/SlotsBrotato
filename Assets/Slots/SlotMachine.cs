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
    float _chargeTimer;

    bool _spinning;
    bool _spinQueued;
    bool _hasPendingSymbols;
    bool _pendingIsJackpot;
    bool _chargeLockedFull;
    bool _isResolvingActivation;
    bool _rewindUsedThisWave;

    readonly List<(int reelIndex, SlotSymbolData data)> _pendingSymbols = new();
    readonly List<(int reelIndex, SlotSymbolData data)> _autoSymbols = new();

    public static SlotMachine Instance { get; private set; }
    Transform _playerTransform;
    PlayerStats _playerStats;

    private TemporaryBuffSystem _buffSystem;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
            _playerStats = playerObj.GetComponent<PlayerStats>();
        }
        else
            Debug.LogError("[SlotMachine] No se encontró ningún GameObject con tag 'Player'");

        _buffSystem = FindFirstObjectByType<TemporaryBuffSystem>();
    }

    void Start()
    {
        _chargeTimer = 0f;
        _charge = 0f;
        _overloadReserve = 0f;
        _spinning = false;
        _spinQueued = false;
        _hasPendingSymbols = false;
        _pendingIsJackpot = false;
        _chargeLockedFull = false;
        _isResolvingActivation = false;
        _rewindUsedThisWave = false;

        if (flashOverlay) flashOverlay.SetActive(false);
        if (pendingIndicator) pendingIndicator.SetActive(false);
        if (waveCover) waveCover.gameObject.SetActive(false);

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

        float chargeTime = GetSlotChargeTime();
        if (chargeTime <= 0f)
            chargeTime = 1f;

        _chargeTimer = Mathf.Min(chargeTime, _chargeTimer + Time.deltaTime);
        _charge = _chargeTimer;

        if (!_spinQueued && _chargeTimer >= chargeTime)
        {
            _chargeTimer = chargeTime;
            _charge = chargeTime;
            _spinQueued = true;
            StartCoroutine(DoSpin());
        }

        UpdateChargeUI();
    }

    public void OnCoinCollected(int amount)
    {
    }

    float GetSlotChargeTime()
    {
        if (_playerStats != null && _playerStats.slotChargeTime > 0f)
            return _playerStats.slotChargeTime;

        return 10f;
    }

    void UpdateChargeUI()
    {
        float chargeTime = GetSlotChargeTime();
        if (chargeTime <= 0f) chargeTime = 1f;

        float displayedMainCharge = _chargeLockedFull ? chargeTime : _chargeTimer;

        if (timerText)
        {
            if (_chargeLockedFull || _spinning || _spinQueued || _overloadReserve > 0f)
                timerText.text = $"{Mathf.Ceil(_overloadReserve)}/{Mathf.Ceil(chargeTime)}";
            else
                timerText.text = $"{Mathf.Ceil(displayedMainCharge)}/{Mathf.Ceil(chargeTime)}";
        }

        if (timerBarFill)
            timerBarFill.fillAmount = chargeTime > 0f ? displayedMainCharge / chargeTime : 0f;

        if (overloadBarFill)
        {
            bool showOverload = _chargeLockedFull || _spinning || _spinQueued || _overloadReserve > 0f;
            overloadBarFill.gameObject.SetActive(showOverload);
            overloadBarFill.fillAmount = chargeTime > 0f ? _overloadReserve / chargeTime : 0f;
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

        float chargeTime = GetSlotChargeTime();

        if (!_hasPendingSymbols)
        {
            _chargeTimer = 0f;
            _charge = 0f;

            if (_overloadReserve > 0f)
            {
                _chargeTimer = Mathf.Min(_overloadReserve, chargeTime);
                _charge = _chargeTimer;
                _overloadReserve = 0f;
            }
        }
        else
        {
            _chargeLockedFull = true;
            _chargeTimer = chargeTime;
            _charge = chargeTime;
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
        Debug.Log("JACKPOT = " + jackpot);

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
        if (symbols.Count < 2)
            return false;

        bool reducedJackpot =
            MechanicModifierManager.Instance != null &&
            MechanicModifierManager.Instance.HasModifier(
                MechanicModifierType.ReducedJackpot);

        Dictionary<SlotSymbolType, int> counts =
            new Dictionary<SlotSymbolType, int>();

        foreach (var symbol in symbols)
        {
            if (!counts.ContainsKey(symbol.data.symbolType))
                counts[symbol.data.symbolType] = 0;

            counts[symbol.data.symbolType]++;
        }

        int requiredMatches = reducedJackpot ? 2 : symbols.Count;

        foreach (var pair in counts)
        {
            if (pair.Value >= requiredMatches)
                return true;
        }

        return false;
    }

    IEnumerator ResolveAutoSymbolsRoutine()
    {
        bool jackpotAuto = _autoSymbols.Count > 0 && AreAllSameType(_autoSymbols);

        for (int i = 0; i < _autoSymbols.Count; i++)
        {
            var symbol = _autoSymbols[i];
            int jackpotValue = 3;

            if (MechanicModifierManager.Instance != null &&
                MechanicModifierManager.Instance.HasModifier(
                    MechanicModifierType.ExtendedJackpot))
            {
                jackpotValue = 5;
            }

            int amount = jackpotAuto ? jackpotValue : 1;

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
            Debug.Log("Activando jackpot. Pendientes: " + _pendingSymbols.Count);
            for (int i = 0; i < _pendingSymbols.Count; i++)
            {
                var p = _pendingSymbols[i];
                int jackpotValue = 3;

                if (MechanicModifierManager.Instance != null &&
                    MechanicModifierManager.Instance.HasModifier(
                        MechanicModifierType.ExtendedJackpot))
                {
                    jackpotValue = 5;
                }

                yield return StartCoroutine(
                    ResolveSingleSymbolVisual(p, jackpotValue));

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

            float chargeTime = GetSlotChargeTime();
            _chargeTimer = 0f;
            _charge = 0f;
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

            RunConfig.Instance?.RegisterActivatedSymbol(symbol.data);
            ApplyByType(symbol.data.symbolType, amount);
        }
        else
        {
            RunConfig.Instance?.RegisterActivatedSymbol(symbol.data);
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

            case SlotSymbolType.Berserk:
                ApplyBerserk(amount);
                break;

            case SlotSymbolType.Power:
                ApplyPower(amount);
                break;

            case SlotSymbolType.DamageUp:
                ApplyDamageUp(amount);
                break;

            case SlotSymbolType.FireRateUp:
                ApplyFireRateUp(amount);
                break;

            case SlotSymbolType.MaxHealthUp:
                ApplyMaxHealthUp(amount);
                break;

            case SlotSymbolType.MoveSpeedUp:
                ApplyMoveSpeedUp(amount);
                break;

            case SlotSymbolType.CritChanceUp:
                ApplyCritChanceUp(amount);
                break;

            case SlotSymbolType.CritDamageUp:
                ApplyCritDamageUp(amount);
                break;

            case SlotSymbolType.RegenUp:
                ApplyRegenUp(amount);
                break;

            case SlotSymbolType.DamageReductionUp:
                ApplyDamageReductionUp(amount);
                break;

            case SlotSymbolType.PickupRadiusUp:
                ApplyPickupRadiusUp(amount);
                break;

            case SlotSymbolType.SlotChargeUp:
                ApplySlotChargeUp(amount);
                break;
        }
    }

    void ApplyShield(int amount)
    {
        int level = RunConfig.Instance.GetSymbolLevel(SlotSymbolType.Shield);

        int shieldAmount =
            amount >= 3
            ? level * 5
            : amount * level;

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

    void ApplyDamageUp(int amount)
    {
        if (_playerStats == null) return;

        float gain = amount >= 3 ? 15f : 5f * amount;
        _playerStats.AddDamage(gain);

        Debug.Log($"+{gain} damage");
    }

    void ApplyFireRateUp(int amount)
    {
        if (_playerStats == null) return;

        float gain = amount >= 3 ? 1.5f : 0.5f * amount;
        _playerStats.AddFireRate(gain);

        Debug.Log($"+{gain} fire rate");
    }

    void ApplyMaxHealthUp(int amount)
    {
        if (_playerStats == null) return;

        int gain = amount >= 3 ? 3 : amount;
        _playerStats.AddMaxHealth(gain);

        Debug.Log($"+{gain} max health");
    }

    void ApplyMoveSpeedUp(int amount)
    {
        if (_playerStats == null) return;

        float gain = amount >= 3 ? 3f : 1f * amount;
        _playerStats.AddMoveSpeed(gain);

        Debug.Log($"+{gain} move speed");
    }

    void ApplyCritChanceUp(int amount)
    {
        if (_playerStats == null) return;

        float gain = amount >= 3 ? 0.15f : 0.05f * amount;
        _playerStats.AddCritChance(gain);

        Debug.Log($"+{gain * 100f}% crit chance");
    }

    void ApplyCritDamageUp(int amount)
    {
        if (_playerStats == null) return;

        float gain = amount >= 3 ? 1f : 0.35f * amount;
        _playerStats.AddCritMultiplier(gain);

        Debug.Log($"+{gain} crit multiplier");
    }

    void ApplyRegenUp(int amount)
    {
        if (_playerStats == null) return;

        float gain = amount >= 3 ? 1.5f : 0.5f * amount;
        _playerStats.AddRegeneration(gain);

        Debug.Log($"+{gain} regeneration");
    }

    void ApplyDamageReductionUp(int amount)
    {
        if (_playerStats == null) return;

        float gain = amount >= 3 ? 0.15f : 0.05f * amount;
        _playerStats.AddDamageReduction(gain);

        Debug.Log($"+{gain * 100f}% damage reduction");
    }

    void ApplyPickupRadiusUp(int amount)
    {
        if (_playerStats == null) return;

        float gain = amount >= 3 ? 3f : 1f * amount;
        _playerStats.AddCoinPickupRadius(gain);

        Debug.Log($"+{gain} pickup radius");
    }

    void ApplySlotChargeUp(int amount)
    {
        if (_playerStats == null) return;

        float reduction = amount >= 3 ? 3f : 1f * amount;
        _playerStats.ReduceSlotChargeTime(reduction);

        Debug.Log($"-{reduction}s slot charge time");
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

    void ApplyBerserk(int amount)
    {
        if (_buffSystem == null) return;

        int level = RunConfig.Instance.GetSymbolLevel(SlotSymbolType.Berserk);
        float baseBuff = 10f * level;
        float damageBuff = baseBuff * amount;
        float duration = 5f;

        _buffSystem.ApplyDamageBuff(damageBuff, duration);
    }

    void ApplyPower(int amount)
    {
        if (_playerTransform == null) return;

        PlayerStats stats = _playerTransform.GetComponent<PlayerStats>();
        if (stats == null) return;

        int level = RunConfig.Instance.GetSymbolLevel(SlotSymbolType.Power);
        float baseValue = 2f * level;

        float damageGain =
            amount >= 3
            ? baseValue * 5f
            : baseValue * amount;

        stats.damage += damageGain;

        Debug.Log($"+{damageGain} daño permanente");
    }

    public void AddCharge(float amount)
    {
        if (_spinning || _chargeLockedFull || _isResolvingActivation)
            return;

        float chargeTime = GetSlotChargeTime();

        _chargeTimer = Mathf.Min(chargeTime, _chargeTimer + amount);
        _charge = _chargeTimer;

        UpdateChargeUI();
    }

    public bool CanRewind()
    {
        return !_rewindUsedThisWave &&
               !_spinning &&
               !_hasPendingSymbols &&
               !_isResolvingActivation;
    }

    public void RewindSpin()
    {
        if (!CanRewind())
            return;

        _rewindUsedThisWave = true;

        _chargeTimer = GetSlotChargeTime();
        _charge = _chargeTimer;

        StartCoroutine(DoSpin());
    }

    public void ResetRewind()
    {
        _rewindUsedThisWave = false;
    }
}