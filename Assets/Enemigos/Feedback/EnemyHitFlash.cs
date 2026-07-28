using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyHitFlash : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private string hitTriggerName = "Hit";

    [Header("Hit State")]
    [SerializeField] private string hitStateName = "Hit";
    [SerializeField] private int animatorLayer = 0;

    [Header("Block Hit On States")]
    [Tooltip("Estados en los que NO se lanzará el hit. Ej: Base Layer.Death")]
    [SerializeField] private List<string> blockedStates = new List<string>();

    [Header("Blocked Hit Flash")]
    [SerializeField] private SpriteRenderer flashSpriteRenderer;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.06f;

    private EnemyHealth _health;
    private int _hitTriggerHash;
    private int _hitStateHash;

    private Color _originalColor;
    private Coroutine _flashCoroutine;

    void Awake()
    {
        _health = GetComponent<EnemyHealth>();

        if (targetAnimator == null)
            targetAnimator = GetComponentInChildren<Animator>();

        _hitTriggerHash = Animator.StringToHash(hitTriggerName);
        _hitStateHash = Animator.StringToHash(hitStateName);

        if (flashSpriteRenderer != null)
            _originalColor = flashSpriteRenderer.color;
    }

    void OnEnable()
    {
        if (_health != null)
            _health.OnDamaged += HandleDamaged;
    }

    void OnDisable()
    {
        if (_health != null)
            _health.OnDamaged -= HandleDamaged;

        if (_flashCoroutine != null)
            StopCoroutine(_flashCoroutine);

        RestoreColor();
    }

    private void HandleDamaged(float amount, float currentHp)
    {
        if (targetAnimator == null)
        {
            Debug.LogWarning($"{name}: targetAnimator es null");
            return;
        }

        if (IsInBlockedState())
        {
            TriggerBlockedFlash();
            return;
        }

        targetAnimator.ResetTrigger(_hitTriggerHash);
        targetAnimator.SetTrigger(_hitTriggerHash);
        targetAnimator.Play(_hitStateHash, animatorLayer, 0f);
        targetAnimator.Update(0f);
    }

    private bool IsInBlockedState()
    {
        AnimatorStateInfo currentState = targetAnimator.GetCurrentAnimatorStateInfo(animatorLayer);

        for (int i = 0; i < blockedStates.Count; i++)
        {
            string stateName = blockedStates[i];

            if (string.IsNullOrWhiteSpace(stateName))
                continue;

            if (currentState.IsName(stateName))
                return true;
        }

        return false;
    }

    private void TriggerBlockedFlash()
    {
        if (flashSpriteRenderer == null)
            return;

        if (_flashCoroutine != null)
            StopCoroutine(_flashCoroutine);

        _flashCoroutine = StartCoroutine(BlockedFlashCoroutine());
    }

    private IEnumerator BlockedFlashCoroutine()
    {
        flashSpriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        RestoreColor();
        _flashCoroutine = null;
    }

    private void RestoreColor()
    {
        if (flashSpriteRenderer == null)
            return;

        flashSpriteRenderer.color = _originalColor;
    }
}