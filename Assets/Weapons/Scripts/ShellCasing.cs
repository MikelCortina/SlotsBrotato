using System.Collections;
using UnityEngine;

public class ShellCasing : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float lifeTime = 5f;

    private Vector3 _initialScale;
    private Coroutine _shrinkCoroutine;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        _initialScale = transform.localScale;
    }

    public void Init(Vector2 direction, float force, float torque)
    {
        if (rb == null)
            return;

        transform.localScale = _initialScale;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
        rb.AddTorque(torque, ForceMode2D.Impulse);

        if (_shrinkCoroutine != null)
            StopCoroutine(_shrinkCoroutine);

        _shrinkCoroutine = StartCoroutine(ShrinkOverLifetime());

        Destroy(gameObject, lifeTime);
    }

    private IEnumerator ShrinkOverLifetime()
    {
        float elapsed = 0f;

        while (elapsed < lifeTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / lifeTime);
            transform.localScale = Vector3.Lerp(_initialScale, Vector3.zero, t);
            yield return null;
        }

        transform.localScale = Vector3.zero;
    }
}