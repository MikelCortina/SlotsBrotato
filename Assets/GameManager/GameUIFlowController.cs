using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GameUIFlowController : MonoBehaviour
{
    [Serializable]
    public class UIPanel
    {
        public string id;
        public GameObject root;
        public CanvasGroup canvasGroup;
        public bool disableWhenHidden = true;
        public bool interactableWhenVisible = true;
    }

    [Header("Paneles")]
    [SerializeField] private UIPanel gameplayHUD;
    [SerializeField] private UIPanel shopPanel;
    [SerializeField] private UIPanel gameOverPanel;

    [Header("Timings")]
    [SerializeField, Min(0f)] private float endWaveDelay = 0f;
    [SerializeField, Min(0f)] private float zoomOutDuration = 1f;
    [SerializeField, Min(0f)] private float gameOverDelay = 0f;

    [Header("Zoom cámara")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private CameraFollow2D cameraFollow2D;
    [SerializeField] private float perspectiveZoomOutOffset = 10f;
    [SerializeField] private float orthographicZoomOutOffset = 2f;

    [Header("Eventos opcionales")]
    public UnityEvent onWaveEndTransitionStarted;
    public UnityEvent onWaveEndTransitionFinished;
    public UnityEvent onGameOverTransitionStarted;

    private GameManager _gameManager;

    public void Initialize(GameManager manager)
    {
        _gameManager = manager;
        CacheAllPanels();

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (cameraFollow2D == null && targetCamera != null)
            cameraFollow2D = targetCamera.GetComponent<CameraFollow2D>();

        HideAllImmediate();
        ShowGameplayImmediate();
    }

    [ContextMenu("Cache CanvasGroups")]
    private void CacheAllPanels()
    {
        PreparePanel(gameplayHUD);
        PreparePanel(shopPanel);
        PreparePanel(gameOverPanel);
    }

    public bool IsGameOverPanelConfigured()
    {
        return gameOverPanel != null && gameOverPanel.root != null;
    }

    public void HideAllImmediate()
    {
        SetPanelVisibleImmediate(gameplayHUD, false);
        SetPanelVisibleImmediate(shopPanel, false);
        SetPanelVisibleImmediate(gameOverPanel, false);
    }

    public void ShowGameplayImmediate()
    {
        HideAllImmediate();
        SetPanelVisibleImmediate(gameplayHUD, true);
    }

    public void ShowShopImmediate()
    {
        HideAllImmediate();
        SetPanelVisibleImmediate(shopPanel, true);
    }

    public void ShowGameOverImmediate()
    {
        if (!IsGameOverPanelConfigured())
        {
            Debug.LogWarning("GameUIFlowController -> gameOverPanel no está configurado.");
            return;
        }

        HideAllImmediate();
        SetPanelVisibleImmediate(gameOverPanel, true);

        Debug.Log("GameUIFlowController -> GameOver panel mostrado. activeInHierarchy: " + gameOverPanel.root.activeInHierarchy);
    }

    public IEnumerator PlayWaveEndTransition(int waveNumber)
    {
        ShowGameplayImmediate();
        onWaveEndTransitionStarted?.Invoke();

        if (endWaveDelay > 0f)
            yield return new WaitForSecondsRealtime(endWaveDelay);

        if (zoomOutDuration > 0f)
            yield return PlayZoomOut();

        onWaveEndTransitionFinished?.Invoke();
    }

    public IEnumerator PlayGameOverTransition()
    {
        ShowGameplayImmediate();
        onGameOverTransitionStarted?.Invoke();

        if (gameOverDelay > 0f)
            yield return new WaitForSecondsRealtime(gameOverDelay);

        ShowGameOverImmediate();
    }

    private IEnumerator PlayZoomOut()
    {
        if (cameraFollow2D != null && targetCamera != null && targetCamera.orthographic)
        {
            cameraFollow2D.StartTransitionZoomOut(orthographicZoomOutOffset, zoomOutDuration);
            yield return new WaitForSecondsRealtime(zoomOutDuration);
            cameraFollow2D.ResetZoomImmediate();
            yield break;
        }

        if (targetCamera == null)
            yield break;

        if (targetCamera.orthographic)
        {
            float baseSize = targetCamera.orthographicSize;
            float target = baseSize + orthographicZoomOutOffset;

            yield return LerpUnscaled(zoomOutDuration, t =>
            {
                targetCamera.orthographicSize = Mathf.Lerp(baseSize, target, EaseOutCubic(t));
            });

            targetCamera.orthographicSize = baseSize;
        }
        else
        {
            float baseFov = targetCamera.fieldOfView;
            float targetFov = baseFov + perspectiveZoomOutOffset;

            yield return LerpUnscaled(zoomOutDuration, t =>
            {
                targetCamera.fieldOfView = Mathf.Lerp(baseFov, targetFov, EaseOutCubic(t));
            });

            targetCamera.fieldOfView = baseFov;
        }
    }

    private IEnumerator LerpUnscaled(float duration, Action<float> onUpdate)
    {
        if (duration <= 0f)
        {
            onUpdate?.Invoke(1f);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            onUpdate?.Invoke(t);
            yield return null;
        }

        onUpdate?.Invoke(1f);
    }

    private float EaseOutCubic(float t)
    {
        t = Mathf.Clamp01(t);
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private void SetPanelVisibleImmediate(UIPanel panel, bool show)
    {
        if (panel == null || panel.root == null)
            return;

        PreparePanel(panel);
        ApplyPanelState(panel, show, show ? 1f : 0f);
    }

    private void ApplyPanelState(UIPanel panel, bool show, float alpha)
    {
        CanvasGroup cg = panel.canvasGroup;

        if (show)
            panel.root.SetActive(true);

        cg.alpha = alpha;
        cg.interactable = show && panel.interactableWhenVisible;
        cg.blocksRaycasts = show && panel.interactableWhenVisible;

        if (!show && panel.disableWhenHidden)
            panel.root.SetActive(false);
    }

    private void PreparePanel(UIPanel panel)
    {
        if (panel == null || panel.root == null)
            return;

        if (panel.canvasGroup == null)
        {
            panel.canvasGroup = panel.root.GetComponent<CanvasGroup>();

            if (panel.canvasGroup == null)
                panel.canvasGroup = panel.root.AddComponent<CanvasGroup>();
        }
    }
}