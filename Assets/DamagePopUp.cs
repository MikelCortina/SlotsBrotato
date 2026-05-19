using System.Collections;
using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshPro textMesh;

    [Header("Motion")]
    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private float lifetime = 0.5f;
    [SerializeField] private float scaleUpMultiplier = 1.15f;

    private Color _originalColor;
    private Transform _tr;

    void Awake()
    {
        _tr = transform;
        if (textMesh == null)
            textMesh = GetComponentInChildren<TextMeshPro>();

        if (textMesh != null)
            _originalColor = textMesh.color;
    }

    public void Show(int damage)
    {
        if (textMesh != null)
            textMesh.text = damage.ToString();

        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float t = 0f;
        Vector3 startScale = _tr.localScale;
        Vector3 targetScale = startScale * scaleUpMultiplier;

        while (t < lifetime)
        {
            t += Time.deltaTime;

            _tr.position += Vector3.up * floatSpeed * Time.deltaTime;
            _tr.localScale = Vector3.Lerp(startScale, targetScale, Mathf.PingPong(t * 4f, 1f));

            if (textMesh != null)
            {
                float alpha = Mathf.Lerp(1f, 0f, t / lifetime);
                textMesh.color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, alpha);
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}