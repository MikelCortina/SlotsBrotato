using TMPro;
using UnityEngine;

public class SymbolTooltipUI : MonoBehaviour
{
    public static SymbolTooltipUI Instance { get; private set; }

    [Header("UI")]
    public GameObject panel;
    public RectTransform panelRect;
    public RectTransform canvasRect;
    public Canvas canvas;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI unlockText;

    void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show(SlotSymbolData symbol, Vector2 screenPosition)
    {
        if (symbol == null || panel == null)
            return;

        panel.SetActive(true);

        if (titleText)
            titleText.text = symbol.symbolName;

        if (descriptionText)
        {
            descriptionText.text =
                $"{symbol.description}\n\n" +
                $"Valor: {symbol.baseEffectValue}\n" +
                $"Jackpot: {symbol.baseEffectValue * symbol.jackpotMultiplier}";
        }

        if (unlockText)
        {
            bool unlocked =
                SymbolUnlockManager.Instance == null ||
                SymbolUnlockManager.Instance.IsUnlocked(symbol);

            unlockText.text = unlocked
                ? "Desbloqueado"
                : $"Bloqueado: llega a la oleada {symbol.unlockWave}";
        }

        MoveToScreenPosition(screenPosition);
    }

    void MoveToScreenPosition(Vector2 screenPosition)
    {
        if (panelRect == null || canvasRect == null || canvas == null)
            return;

        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition + new Vector2(20f, -60f),
            canvas.worldCamera,
            out localPoint
        );

        panelRect.anchoredPosition = localPoint;
    }

    public void Hide()
    {
        if (panel)
            panel.SetActive(false);
    }
}