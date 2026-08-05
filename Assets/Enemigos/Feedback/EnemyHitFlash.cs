using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyHitFlash : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer targetSpriteRenderer;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.06f;
    [SerializeField][Range(0f, 1f)] private float flashAmount = 1f;

    private EnemyHealth _health;
    private Coroutine _flashCoroutine;
    private MaterialPropertyBlock _propertyBlock;

    private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");
    private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");

    void Awake()
    {
        _health = GetComponent<EnemyHealth>();

        if (targetSpriteRenderer == null)
            targetSpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (targetSpriteRenderer != null)
            _propertyBlock = new MaterialPropertyBlock();
    }

    void OnEnable()
    {
        if (_health != null)
            _health.OnDamaged += HandleDamaged;

        ResetFlash();
    }

    void OnDisable()
    {
        if (_health != null)
            _health.OnDamaged -= HandleDamaged;

        if (_flashCoroutine != null)
            StopCoroutine(_flashCoroutine);

        _flashCoroutine = null;
        ResetFlash();
    }

    private void HandleDamaged(
        float amount,
        float currentHp,
        bool isCritical
    )
    {
        if (targetSpriteRenderer == null)
        {
            Debug.LogWarning($"{name}: targetSpriteRenderer es null");
            return;
        }

        if (_propertyBlock == null)
            _propertyBlock = new MaterialPropertyBlock();

        if (_flashCoroutine != null)
            StopCoroutine(_flashCoroutine);

        _flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        SetFlash(flashColor, flashAmount);
        yield return new WaitForSeconds(flashDuration);
        ResetFlash();
        _flashCoroutine = null;
    }

    private void SetFlash(Color color, float amount)
    {
        if (targetSpriteRenderer == null)
            return;

        targetSpriteRenderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor(FlashColorId, color);
        _propertyBlock.SetFloat(FlashAmountId, amount);
        targetSpriteRenderer.SetPropertyBlock(_propertyBlock);
    }

    private void ResetFlash()
    {
        if (targetSpriteRenderer == null)
            return;

        if (_propertyBlock == null)
            _propertyBlock = new MaterialPropertyBlock();

        targetSpriteRenderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor(FlashColorId, flashColor);
        _propertyBlock.SetFloat(FlashAmountId, 0f);
        targetSpriteRenderer.SetPropertyBlock(_propertyBlock);
    }
}