using System.Collections;
using TMPro;
using UnityEngine;

public class EnemyDamagePopupSpawner : MonoBehaviour
{
    [Header("Popup")]
    [SerializeField] private DamagePopup popupPrefab;
    [SerializeField] private Vector3 popupOffset = new Vector3(0f, 0.8f, 0f);
    [SerializeField] private float randomHorizontalOffset = 0.2f;

    private EnemyHealth _health;

    void Awake()
    {
        _health = GetComponent<EnemyHealth>();
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
    }

    private void HandleDamaged(float amount, float currentHp)
    {
        if (popupPrefab == null) return;

        Vector3 pos = transform.position + popupOffset;
        pos.x += Random.Range(-randomHorizontalOffset, randomHorizontalOffset);

        DamagePopup popup = Instantiate(popupPrefab, pos, Quaternion.identity);
        popup.Show(Mathf.RoundToInt(amount));
    }
}