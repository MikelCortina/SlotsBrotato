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
    public GameObject pendingIndicator; // icono/panel que avisa de que hay símbolos por activar

    [Header("Spin Config")]
    public float reelSpinDuration = 1.2f;
    public float reelStaggerDelay = 0.18f;

    // ?? Estado interno ????????????????????????????????????????????????
    float _charge;
    bool _spinning;
    bool _hasPendingSymbols;
    bool _pendingIsJackpot;

    // Guarda el resultado de la última tirada sin aplicar aún
    readonly List<SlotSymbolData> _pendingSymbols = new();

    public static SlotMachine Instance { get; private set; }
    Transform _playerTransform;

    // ?????????????????????????????????????????????????????????????????

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
        UpdateChargeUI();
    }

    void Update()
    {
        // Activación manual: el jugador pulsa la tecla cuando quiere usar los símbolos
        if (_hasPendingSymbols && Input.GetKeyDown(activateKey))
        {
            ActivatePendingSymbols();
            return;
        }

        // La siguiente carga NO empieza mientras haya símbolos pendientes
        if (_hasPendingSymbols || _spinning) return;

        if (_charge >= maxCharge)
        {
            _charge = maxCharge;
            StartCoroutine(DoSpin());
        }
    }

    // ?? Carga ?????????????????????????????????????????????????????????

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

    // ?? Spin ??????????????????????????????????????????????????????????

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

        CollectResults();

        _charge = 0f;
        UpdateChargeUI();
        _spinning = false;
    }

    // ?? Recolectar resultado (sin aplicar todavía) ????????????????????

    void CollectResults()
    {
        _pendingSymbols.Clear();

        foreach (var reel in reels)
        {
            if (reel == null || reel.CurrentSymbol == null) continue;
            _pendingSymbols.Add(reel.CurrentSymbol);
        }

        if (_pendingSymbols.Count == 0) return;

        // Detectar jackpot: los 3 símbolos son del mismo tipo
        _pendingIsJackpot = IsJackpot(_pendingSymbols);

        _hasPendingSymbols = true;
        if (pendingIndicator) pendingIndicator.SetActive(true);

        if (_pendingIsJackpot)
            StartCoroutine(WinFlash());
    }

    bool IsJackpot(List<SlotSymbolData> symbols)
    {
        if (symbols.Count < 2) return false;
        var first = symbols[0].symbolType;
        for (int i = 1; i < symbols.Count; i++)
            if (symbols[i].symbolType != first) return false;
        return true;
    }

    // ?? Activación manual ?????????????????????????????????????????????

    void ActivatePendingSymbols()
    {
        if (!_hasPendingSymbols || _pendingSymbols.Count == 0) return;

        // Jackpot: activa los 3 de golpe en una sola pulsación
        if (_pendingIsJackpot)
        {
            ApplyJackpot(_pendingSymbols[0].symbolType);
            _pendingSymbols.Clear();
        }
        else
        {
            // Normal: consume solo el primer símbolo de la lista
            var symbol = _pendingSymbols[0];
            _pendingSymbols.RemoveAt(0);

            switch (symbol.symbolType)
            {
                case SlotSymbolType.Shield: ApplyShield(1); break;
                case SlotSymbolType.Coin: ApplyCoins(1); break;
                case SlotSymbolType.Static: ApplyStatik(1); break;
            }
        }

        // Si ya no quedan símbolos pendientes, limpiar estado
        if (_pendingSymbols.Count == 0)
        {
            _hasPendingSymbols = false;
            _pendingIsJackpot = false;
            if (pendingIndicator) pendingIndicator.SetActive(false);
        }
    }

    // ?? Aplicar efectos ???????????????????????????????????????????????

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

    void ApplyJackpot(SlotSymbolType type)
    {
        // Jackpot siempre usa multiplicador máximo (como si hubieran salido 3)
        switch (type)
        {
            case SlotSymbolType.Shield: ApplyShield(3); break;
            case SlotSymbolType.Coin: ApplyCoins(3); break;
            case SlotSymbolType.Static: ApplyStatik(3); break;
        }
    }

    // ?? Flash ?????????????????????????????????????????????????????????

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