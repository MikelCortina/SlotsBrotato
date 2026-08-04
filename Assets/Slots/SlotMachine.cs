using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SlotMachine : MonoBehaviour
{
    [Header("Charge")]
    [Min(1f)] public float maxCharge = 10f;
    [Min(0f)] public float maxOverloadReserve = 10f;
    [Min(1)] public int totalSegments = 10;
    [Min(1f)] public float chargePerCoinSegments = 1f;

    [Header("Fourth Reel")]
    public GameObject fourthReelObject;
    public TextMeshProUGUI jackpotMessageText;

    [SerializeField] private UnityEngine.UI.Image fourthReelLockImage;
    [SerializeField] private Sprite fourthReelClosedSprite;
    [SerializeField] private Sprite fourthReelOpenSprite;


    [Header("Fifth Reel")]
    [SerializeField] private UnityEngine.UI.Image fifthReelLockImage;
    [SerializeField] private Sprite fifthReelClosedSprite;
    [SerializeField] private Sprite fifthReelOpenSprite;
    [Header("UI")]
    public SlotReel[] reels;
    public TextMeshProUGUI timerText;
    public GameObject flashOverlay;
    public GameObject pendingIndicator;
    public SegmentedChargeBar segmentedChargeBar;

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
    public UnityEngine.UI.Image waveCover;

    [Header("Reward Popup")]
    [SerializeField] private RewardPopup rewardPopupPrefab;
    [SerializeField] private Vector3 rewardPopupOffset = new Vector3(0f, 1.8f, 0f);
    [SerializeField] private float rewardPopupRandomX = 0.35f;

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


    [Header("Spin Animation")]
    [SerializeField] private Animator spinAnimator;         // ← NUEVO
    [SerializeField] private string spinAnimationTrigger = "SpinStart"; // ← NUEVO (nombre del trigger en el Animator)

    public bool IsChargeLocked { get; set; } = false;


    SlotSymbolType? _lastActivatedSymbolType;
    readonly List<(int reelIndex, SlotSymbolData data)> _pendingSymbols = new();
    readonly List<(int reelIndex, SlotSymbolData data)> _autoSymbols = new();

    public static SlotMachine Instance { get; private set; }
    Transform _playerTransform;
    PlayerStats _playerStats;
    TemporaryBuffSystem _buffSystem;



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
        {
            Debug.LogError("[SlotMachine] No se encontró ningún GameObject con tag 'Player'");
        }

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

        if (jackpotMessageText)
            jackpotMessageText.text = "";

        if (flashOverlay) flashOverlay.SetActive(false);
        if (pendingIndicator) pendingIndicator.SetActive(false);
        if (waveCover) waveCover.gameObject.SetActive(false);

        if (reels != null)
        {
            for (int i = 0; i < reels.Length; i++)
            {
                if (reels[i] == null) continue;

                bool active = IsReelEnabledByModifier(i);
                reels[i].gameObject.SetActive(active);

                if (active)
                    reels[i].ForceShowLock();
            }
        }

        UpdateChargeUI();
        RefreshReelVisibility();
    }

    void Update()
    {
        if (IsChargeLocked) return;


        if (_hasPendingSymbols && !_isResolvingActivation && Input.GetKeyDown(activateKey))
        {
            StartCoroutine(ActivatePendingSymbolsRoutine());
            return;
        }

        float chargeTime = GetSlotChargeTime();
        if (chargeTime <= 0f) chargeTime = 1f;

        bool currentSpinUnavailable = _spinning || _chargeLockedFull || _isResolvingActivation;

        if (!currentSpinUnavailable)
        {
            _chargeTimer = Mathf.Min(chargeTime, _chargeTimer + Time.deltaTime);
            _charge = _chargeTimer;

            if (!_spinQueued && _chargeTimer >= chargeTime)
            {
                _chargeTimer = chargeTime;
                _charge = _chargeTimer;
                _spinQueued = true;
                StartCoroutine(DoSpin());
            }
        }
        else
        {
            float overloadCap = GetOverloadCap(chargeTime);
            _overloadReserve = Mathf.Min(overloadCap, _overloadReserve + Time.deltaTime);
        }

        UpdateChargeUI();
    }
    public void ResetChargeForNewWave()
    {
        _charge = 0f;
        _chargeTimer = 0f;
        _overloadReserve = 0f;
        _spinQueued = false;
        _chargeLockedFull = false;

        UpdateChargeUI();
    }

    public void OnCoinCollected(int amount)
    {

        if (IsChargeLocked) return;
        if (amount <= 0) return;

        float chargeTime = GetSlotChargeTime();
        if (chargeTime <= 0f) chargeTime = 1f;

        float chargePerSegment = chargeTime / Mathf.Max(1, totalSegments);
        float addedCharge = amount * chargePerCoinSegments * chargePerSegment;

        bool currentSpinUnavailable = _spinning || _chargeLockedFull || _isResolvingActivation;

        if (currentSpinUnavailable)
        {
            float overloadCap = GetOverloadCap(chargeTime);
            _overloadReserve = Mathf.Min(overloadCap, _overloadReserve + addedCharge);
        }
        else
        {
            _chargeTimer = Mathf.Min(chargeTime, _chargeTimer + addedCharge);
            _charge = _chargeTimer;

            if (!_spinQueued && _chargeTimer >= chargeTime)
            {
                _chargeTimer = chargeTime;
                _charge = _chargeTimer;
                _spinQueued = true;
                StartCoroutine(DoSpin());
            }
        }

        UpdateChargeUI();
    }

    float GetSlotChargeTime()
    {
        if (_playerStats != null && _playerStats.slotChargeTime > 0f)
            return _playerStats.slotChargeTime;

        return maxCharge > 0f ? maxCharge : 10f;
    }

    float GetOverloadCap(float chargeTime)
    {
        if (maxOverloadReserve > 0f)
            return Mathf.Min(maxOverloadReserve, chargeTime);

        return chargeTime;
    }

    void UpdateChargeUI()
    {
        float chargeTime = GetSlotChargeTime();
        if (chargeTime <= 0f) chargeTime = 1f;

        if (timerText)
        {
            int mainSegs = Mathf.Clamp(
                Mathf.RoundToInt((_chargeTimer / chargeTime) * totalSegments),
                0,
                totalSegments);

            int overSegs = Mathf.Clamp(
                Mathf.RoundToInt((_overloadReserve / chargeTime) * totalSegments),
                0,
                totalSegments - mainSegs);

            if (_hasPendingSymbols)
                timerText.text = $"{mainSegs}+{overSegs}/{totalSegments} READY";
            else
                timerText.text = $"{mainSegs}+{overSegs}/{totalSegments}";
        }

        if (segmentedChargeBar != null)
            segmentedChargeBar.SetByValues(_chargeTimer, _overloadReserve, chargeTime);
    }
    // Llamado desde el Animation Event
    public void OnSpinAnimationReady()
    {
        StartCoroutine(SpinReels());
    }
    IEnumerator DoSpin()
    {
        RefreshReelVisibility();
        _spinning = true;
        _spinQueued = false;

        if (waveCover) waveCover.gameObject.SetActive(true);

        // Dispara la animación — los reels arrancan desde el Animation Event
        if (spinAnimator != null)
            spinAnimator.SetTrigger(spinAnimationTrigger);

        // Espera a que SpinReels termine (lo lanzará el Animation Event)
        yield return new WaitUntil(() => !_spinning);
    }
    IEnumerator SpinReels()
    {
        for (int i = 0; i < reels.Length; i++)
        {
            if (!IsReelEnabledByModifier(i)) continue;
            if (reels[i] == null) continue;
            if (!reels[i].gameObject.activeInHierarchy) continue;

            float stopDelay = reelSpinDuration + i * reelStaggerDelay;
            reels[i].StartSpin(stopDelay);
        }

        int activeReelCount = 0;
        for (int i = 0; i < reels.Length; i++)
            if (IsReelEnabledByModifier(i)) activeReelCount++;

        float totalDuration = reelSpinDuration + (activeReelCount - 1) * reelStaggerDelay + 0.1f;
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
            _chargeTimer = Mathf.Min(_overloadReserve, chargeTime);
            _charge = _chargeTimer;
            _overloadReserve = 0f;
        }
        else
        {
            _chargeLockedFull = true;
            _chargeTimer = chargeTime;
            _charge = _chargeTimer;
        }

        UpdateChargeUI();
        _spinning = false;  // ← aquí se desbloquea el WaitUntil de DoSpin
    }

    void CollectResults()
    {
        _pendingSymbols.Clear();
        _autoSymbols.Clear();

        List<(int reelIndex, SlotSymbolData data)> results = new();

        for (int i = 0; i < reels.Length; i++)
        {
            if (!IsReelEnabledByModifier(i)) continue;

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
        {
            StartCoroutine(WinFlash());

            bool extended =
                MechanicModifierManager.Instance != null &&
                MechanicModifierManager.Instance.HasModifier(
                    MechanicModifierType.ExtendedJackpot);

            StartCoroutine(
                ShowJackpotMessage(
                    extended ? "EXTENDED JACKPOT!" : "JACKPOT!"
                )
            );
        }
    }

    bool IsJackpot(List<(int reelIndex, SlotSymbolData data)> symbols)
    {
        if (symbols == null || symbols.Count < 2)
            return false;

        bool reducedJackpot =
            MechanicModifierManager.Instance != null &&
            MechanicModifierManager.Instance.HasModifier(
                MechanicModifierType.ReducedJackpot
            );

        int requiredMatches = reducedJackpot ? 2 : 3;

        Dictionary<SlotSymbolType, int> counts =
            new Dictionary<SlotSymbolType, int>();

        foreach (var symbol in symbols)
        {
            if (symbol.data == null)
                continue;

            SlotSymbolType type = symbol.data.symbolType;

            if (!counts.ContainsKey(type))
                counts[type] = 0;

            counts[type]++;

            if (counts[type] >= requiredMatches)
                return true;
        }

        return false;
    }
    IEnumerator ResolveAutoSymbolsRoutine()
    {
        for (int i = 0; i < _autoSymbols.Count; i++)
        {
            var symbol = _autoSymbols[i];

            bool isPartOfJackpot =
                HasJackpotMatch(
                    _autoSymbols,
                    symbol.data.symbolType
                );

            int amount = isPartOfJackpot
                ? GetJackpotAmount()
                : 1;

            yield return StartCoroutine(
                ResolveSingleSymbolVisual(symbol, amount)
            );

            if (i < _autoSymbols.Count - 1 &&
                activationStepDelay > 0f)
            {
                yield return new WaitForSeconds(
                    activationStepDelay
                );
            }
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
                int jackpotValue = GetJackpotAmount();

                yield return StartCoroutine(ResolveSingleSymbolVisual(p, jackpotValue));

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
            for (int i = 0; i < reels.Length; i++)
            {
                if (!IsReelEnabledByModifier(i))
                    continue;

                if (reels[i] != null)
                    reels[i].ForceShowLock();
            }

            _hasPendingSymbols = false;
            _pendingIsJackpot = false;
            _chargeLockedFull = false;

            if (pendingIndicator)
                pendingIndicator.SetActive(false);

            float chargeTime = GetSlotChargeTime();
            _chargeTimer = Mathf.Min(_overloadReserve, chargeTime);
            _charge = _chargeTimer;
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
        }

        RunConfig.Instance?.RegisterActivatedSymbol(symbol.data);

        if (symbol.data.symbolType != SlotSymbolType.Echo)
            _lastActivatedSymbolType = symbol.data.symbolType;

        ApplyByType(symbol.data.symbolType, amount);

        // ✅ AÑADE ESTO:
        string popupText = BuildRewardPopupText(symbol.data.symbolType, amount);
        Color popupColor = GetRewardPopupColor(symbol.data.symbolType);
        SpawnRewardPopup(popupText, popupColor);
    }

    int GetJackpotAmount()
    {
        bool extended =
            MechanicModifierManager.Instance != null &&
            MechanicModifierManager.Instance.HasModifier(
                MechanicModifierType.ExtendedJackpot);

        return extended ? 5 : 3;
    }

    SlotSymbolData GetSymbolData(SlotSymbolType type)
    {
        return RunConfig.Instance != null ? RunConfig.Instance.GetSymbolData(type) : null;
    }

    int GetSymbolLevel(SlotSymbolType type)
    {
        return RunConfig.Instance != null ? RunConfig.Instance.GetSymbolLevel(type) : 1;
    }

    float GetSymbolBaseValue(SlotSymbolType type)
    {
        SlotSymbolData data = GetSymbolData(type);
        if (data == null) return 0f;

        int level = GetSymbolLevel(type);
        return data.baseEffectValue + data.valuePerLevel * (level - 1);
    }

    float GetSymbolMultiplier(SlotSymbolType type, int amount)
    {
        SlotSymbolData data = GetSymbolData(type);
        if (data == null) return 0f;

        return amount >= 3 ? data.jackpotMultiplier : amount;
    }

    float GetSymbolFinalValue(SlotSymbolType type, int amount)
    {
        SlotSymbolData data = GetSymbolData(type);
        if (data == null) return 0f;

        float baseValue = GetSymbolBaseValue(type);
        float multiplier = GetSymbolMultiplier(type, amount);
        return baseValue * multiplier;
    }

    int GetSymbolFinalIntValue(SlotSymbolType type, int amount)
    {
        return Mathf.RoundToInt(GetSymbolFinalValue(type, amount));
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

    void SpawnRewardPopup(string text, Color color)
    {
        if (rewardPopupPrefab == null || _playerTransform == null || string.IsNullOrEmpty(text))
            return;

        Vector3 randomOffset = new Vector3(Random.Range(-rewardPopupRandomX, rewardPopupRandomX), 0f, 0f);
        Vector3 spawnPos = _playerTransform.position + rewardPopupOffset + randomOffset;

        RewardPopup popup = Instantiate(rewardPopupPrefab, spawnPos, Quaternion.identity);
        popup.Show(text, color);
    }

    string BuildRewardPopupText(SlotSymbolType type, int amount)
    {
        float value = GetSymbolFinalValue(type, amount);
        int intValue = Mathf.RoundToInt(value);

        switch (type)
        {
            case SlotSymbolType.Shield:
                return $"+{intValue} Shield";

            case SlotSymbolType.Coin:
                return $"+{intValue} Coins";

            case SlotSymbolType.Static:
                return $"Static x{intValue}";

            case SlotSymbolType.Berserk:
                return $"+{value:0.#} Berserk";

            case SlotSymbolType.Power:
                return $"+{value:0.#} Damage";

            case SlotSymbolType.DamageUp:
                return $"+{value:0.#} Damage";

            case SlotSymbolType.FireRateUp:
                return $"+{value:0.##} Fire Rate";

            case SlotSymbolType.MaxHealthUp:
                return $"+{intValue} Max HP";

            case SlotSymbolType.MoveSpeedUp:
                return $"+{value:0.#} Move Speed";

            case SlotSymbolType.CritChanceUp:
                return $"+{value * 100f:0.#}% Crit";

            case SlotSymbolType.CritDamageUp:
                return $"+{value:0.#} Crit Dmg";

            case SlotSymbolType.RegenUp:
                return $"+{value:0.#} Regen";

            case SlotSymbolType.DamageReductionUp:
                return $"+{value * 100f:0.#}% DR";

            case SlotSymbolType.PickupRadiusUp:
                return $"+{value:0.#} Pickup Radius";

            case SlotSymbolType.SlotChargeUp:
                return $"-{value:0.#}s Slot Charge";
        }

        return type.ToString();
    }

    Color GetRewardPopupColor(SlotSymbolType type)
    {
        switch (type)
        {
            case SlotSymbolType.Coin:
                return new Color(1f, 0.85f, 0.2f);

            case SlotSymbolType.Shield:
                return new Color(0.4f, 0.9f, 1f);

            case SlotSymbolType.Static:
                return new Color(0.7f, 0.5f, 1f);

            case SlotSymbolType.Berserk:
                return new Color(1f, 0.35f, 0.35f);

            case SlotSymbolType.Power:
                return new Color(1f, 0.5f, 0.2f);

            case SlotSymbolType.DamageUp:
                return new Color(1f, 0.4f, 0.4f);

            case SlotSymbolType.FireRateUp:
                return new Color(1f, 0.65f, 0.3f);

            case SlotSymbolType.MaxHealthUp:
                return new Color(0.45f, 1f, 0.45f);

            case SlotSymbolType.MoveSpeedUp:
                return new Color(0.45f, 1f, 0.8f);

            case SlotSymbolType.CritChanceUp:
                return new Color(1f, 0.5f, 0.85f);

            case SlotSymbolType.CritDamageUp:
                return new Color(1f, 0.25f, 0.75f);

            case SlotSymbolType.RegenUp:
                return new Color(0.3f, 1f, 0.55f);

            case SlotSymbolType.DamageReductionUp:
                return new Color(0.55f, 0.75f, 1f);

            case SlotSymbolType.PickupRadiusUp:
                return new Color(1f, 0.95f, 0.45f);

            case SlotSymbolType.SlotChargeUp:
                return new Color(0.55f, 1f, 1f);

            default:
                return Color.white;
        }
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
            case SlotSymbolType.SpeedToDamage:
                ApplySpeedToDamage(amount);
                break;
            case SlotSymbolType.HealthToShield:
                ApplyHealthToShield(amount);
                break;
            case SlotSymbolType.Echo:
                ApplyEcho(amount);
                break;

        }
    }

    void ApplyShield(int amount)
    {
        var playerShield = FindFirstObjectByType<PlayerShield>();
        if (playerShield == null) return;

        int shieldAmount = GetSymbolFinalIntValue(SlotSymbolType.Shield, amount);
        playerShield.AddShield(shieldAmount);
    }

    void ApplyCoins(int amount)
    {
        if (PlayerWallet.Instance == null) return;

        int coinAmount = GetSymbolFinalIntValue(SlotSymbolType.Coin, amount);
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

        int chains = GetSymbolFinalIntValue(SlotSymbolType.Static, amount);
        chains = Mathf.Max(chains, aliveEnemies);

        if (ChainLightning.Instance != null && _playerTransform != null)
            ChainLightning.Instance.Trigger(_playerTransform, chains, float.MaxValue, 20f);
    }

    void ApplyDamageUp(int amount)
    {
        if (_playerStats == null) return;

        float gain = GetSymbolFinalValue(SlotSymbolType.DamageUp, amount);
        _playerStats.AddDamage(gain);

        Debug.Log($"+{gain} damage");
    }

    void ApplyFireRateUp(int amount)
    {
        if (_playerStats == null) return;

        float gain = GetSymbolFinalValue(SlotSymbolType.FireRateUp, amount);
        _playerStats.AddFireRate(gain);

        Debug.Log($"+{gain} fire rate");
    }

    void ApplyMaxHealthUp(int amount)
    {
        if (_playerStats == null) return;

        int gain = GetSymbolFinalIntValue(SlotSymbolType.MaxHealthUp, amount);
        _playerStats.AddMaxHealth(gain);

        Debug.Log($"+{gain} max health");
    }

    void ApplyMoveSpeedUp(int amount)
    {
        if (_playerStats == null) return;

        float gain = GetSymbolFinalValue(SlotSymbolType.MoveSpeedUp, amount);
        _playerStats.AddMoveSpeed(gain);

        Debug.Log($"+{gain} move speed");
    }

    void ApplyCritChanceUp(int amount)
    {
        if (_playerStats == null) return;

        float gain = GetSymbolFinalValue(SlotSymbolType.CritChanceUp, amount);
        _playerStats.AddCritChance(gain);

        Debug.Log($"+{gain * 100f}% crit chance");
    }

    void ApplyCritDamageUp(int amount)
    {
        if (_playerStats == null) return;

        float gain = GetSymbolFinalValue(SlotSymbolType.CritDamageUp, amount);
        _playerStats.AddCritMultiplier(gain);

        Debug.Log($"+{gain} crit multiplier");
    }

    void ApplyRegenUp(int amount)
    {
        if (_playerStats == null) return;

        float gain = GetSymbolFinalValue(SlotSymbolType.RegenUp, amount);
        _playerStats.AddRegeneration(gain);

        Debug.Log($"+{gain} regeneration");
    }

    void ApplyDamageReductionUp(int amount)
    {
        if (_playerStats == null) return;

        float gain = GetSymbolFinalValue(SlotSymbolType.DamageReductionUp, amount);
        _playerStats.AddDamageReduction(gain);

        Debug.Log($"+{gain * 100f}% damage reduction");
    }

    void ApplyPickupRadiusUp(int amount)
    {
        if (_playerStats == null) return;

        float gain = GetSymbolFinalValue(SlotSymbolType.PickupRadiusUp, amount);
        _playerStats.AddCoinPickupRadius(gain);

        Debug.Log($"+{gain} pickup radius");
    }

    void ApplySlotChargeUp(int amount)
    {
        if (_playerStats == null) return;

        float reduction = GetSymbolFinalValue(SlotSymbolType.SlotChargeUp, amount);
        _playerStats.ReduceSlotChargeTime(reduction);

        Debug.Log($"-{reduction}s slot charge time");
        ClampChargeAfterChargeTimeChanged();
    }

    void ApplyStatSymbol(SlotSymbolType type, int amount)
    {
        if (_playerStats == null)
            return;

        SlotSymbolData symbolData =
            RunConfig.Instance.GetSymbolData(type);

        if (symbolData == null)
            return;

        int level =
            RunConfig.Instance.GetSymbolLevel(type);

        float baseValue =
            symbolData.baseEffectValue +
            symbolData.valuePerLevel * (level - 1);

        float multiplier =
            amount >= 3
            ? symbolData.jackpotMultiplier
            : amount;

        float finalValue = baseValue * multiplier;

        switch (type)
        {
            case SlotSymbolType.DamageUp:
                _playerStats.damage += finalValue;
                break;
        }
    }
    void ApplySpeedToDamage(int amount)
    {
        if (_playerStats == null) return;

        SlotSymbolData symbolData =
            RunConfig.Instance.GetSymbolData(SlotSymbolType.SpeedToDamage);

        if (symbolData == null) return;

        int level =
            RunConfig.Instance.GetSymbolLevel(SlotSymbolType.SpeedToDamage);

        float conversionRatio =
            symbolData.baseEffectValue +
            symbolData.valuePerLevel * (level - 1);

        float multiplier =
            amount >= 3
            ? symbolData.jackpotMultiplier
            : amount;

        float damageGain =
            _playerStats.moveSpeed * conversionRatio * multiplier;

        _playerStats.AddDamage(damageGain);

        Debug.Log($"Furia Cinética: +{damageGain} daño por velocidad");
    }

    void ApplyHealthToShield(int amount)
    {
        if (_playerStats == null) return;

        SlotSymbolData symbolData =
            RunConfig.Instance.GetSymbolData(SlotSymbolType.HealthToShield);

        if (symbolData == null) return;

        int level =
            RunConfig.Instance.GetSymbolLevel(SlotSymbolType.HealthToShield);

        float conversionRatio =
            symbolData.baseEffectValue +
            symbolData.valuePerLevel * (level - 1);

        float multiplier =
            amount >= 3
            ? symbolData.jackpotMultiplier
            : amount;

        int shieldGain =
            Mathf.RoundToInt(_playerStats.maxHealth * conversionRatio * multiplier);

        PlayerShield playerShield =
            FindFirstObjectByType<PlayerShield>();

        if (playerShield != null)
            playerShield.AddShield(shieldGain);

        Debug.Log($"Escudo Vital: +{shieldGain} escudo por vida máxima");
    }

    void ApplyEcho(int amount)
    {
        if (_lastActivatedSymbolType == null)
            return;

        Debug.Log($"Eco repite {_lastActivatedSymbolType}");

        ApplyByType(_lastActivatedSymbolType.Value, amount);
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

        float damageBuff = GetSymbolFinalValue(SlotSymbolType.Berserk, amount);
        float duration = 5f;

        _buffSystem.ApplyDamageBuff(damageBuff, duration);
    }

    void ApplyPower(int amount)
    {
        if (_playerStats == null) return;

        float damageGain = GetSymbolFinalValue(SlotSymbolType.Power, amount);
        _playerStats.AddDamage(damageGain);

        Debug.Log($"+{damageGain} daño permanente");
    }

    public void AddCharge(float amount)
    {
        if (amount <= 0f) return;

        float chargeTime = GetSlotChargeTime();
        bool currentSpinUnavailable = _spinning || _chargeLockedFull || _isResolvingActivation;

        if (currentSpinUnavailable)
        {
            float overloadCap = GetOverloadCap(chargeTime);
            _overloadReserve = Mathf.Min(overloadCap, _overloadReserve + amount);
        }
        else
        {
            _chargeTimer = Mathf.Min(chargeTime, _chargeTimer + amount);
            _charge = _chargeTimer;

            if (!_spinQueued && _chargeTimer >= chargeTime)
            {
                _chargeTimer = chargeTime;
                _charge = _chargeTimer;
                _spinQueued = true;
                StartCoroutine(DoSpin());
            }
        }

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

    public int GetReelCount()
    {
        int count = Mathf.Min(3, reels != null ? reels.Length : 0);

        if (MechanicModifierManager.Instance != null &&
            MechanicModifierManager.Instance.HasModifier(
                MechanicModifierType.FourthReel
            ))
        {
            count++;
        }

        if (MechanicModifierManager.Instance != null &&
            MechanicModifierManager.Instance.HasModifier(
                MechanicModifierType.FifthReel
            ))
        {
            count++;
        }

        return count;
    }

    bool IsReelEnabledByModifier(int index)
    {
        // Las tres ruletas principales siempre funcionan.
        if (index < 3)
            return true;

        // Índice 3: cuarta ruleta.
        if (index == 3)
        {
            return MechanicModifierManager.Instance != null &&
                   MechanicModifierManager.Instance.HasModifier(
                       MechanicModifierType.FourthReel
                   );
        }

        // Índice 4: quinta ruleta.
        if (index == 4)
        {
            return MechanicModifierManager.Instance != null &&
                   MechanicModifierManager.Instance.HasModifier(
                       MechanicModifierType.FifthReel
                   );
        }

        return false;
    }

    public void RefreshReelVisibility()
    {
        bool fourthReelActive =
            MechanicModifierManager.Instance != null &&
            MechanicModifierManager.Instance.HasModifier(
                MechanicModifierType.FourthReel
            );

        bool fifthReelActive =
            MechanicModifierManager.Instance != null &&
            MechanicModifierManager.Instance.HasModifier(
                MechanicModifierType.FifthReel
            );

        // Solo cambiamos sprites. No desactivamos ningún borde.
        if (fourthReelLockImage != null)
        {
            fourthReelLockImage.sprite = fourthReelActive
                ? fourthReelOpenSprite
                : fourthReelClosedSprite;
        }

        if (fifthReelLockImage != null)
        {
            fifthReelLockImage.sprite = fifthReelActive
                ? fifthReelOpenSprite
                : fifthReelClosedSprite;
        }

        if (reels == null)
            return;

        // Todos permanecen activos para no modificar el layout.
        for (int i = 0; i < reels.Length; i++)
        {
            if (reels[i] != null)
                reels[i].gameObject.SetActive(true);
        }
    }
    IEnumerator ShowJackpotMessage(string message)
    {
        if (jackpotMessageText == null)
            yield break;

        jackpotMessageText.text = message;
        jackpotMessageText.gameObject.SetActive(true);

        yield return new WaitForSeconds(1.2f);

        jackpotMessageText.text = "";
    }

    public void ClampChargeAfterChargeTimeChanged()
    {
        float chargeTime = GetSlotChargeTime();

        _chargeTimer = Mathf.Min(_chargeTimer, chargeTime);
        _charge = Mathf.Min(_charge, chargeTime);

        UpdateChargeUI();
    }

    bool HasJackpotMatch(
    List<(int reelIndex, SlotSymbolData data)> symbols,
    SlotSymbolType type)
    {
        bool reducedJackpot =
            MechanicModifierManager.Instance != null &&
            MechanicModifierManager.Instance.HasModifier(
                MechanicModifierType.ReducedJackpot
            );

        int requiredMatches = reducedJackpot ? 2 : 3;
        int matches = 0;

        foreach (var symbol in symbols)
        {
            if (symbol.data != null &&
                symbol.data.symbolType == type)
            {
                matches++;
            }
        }

        return matches >= requiredMatches;
    }
}