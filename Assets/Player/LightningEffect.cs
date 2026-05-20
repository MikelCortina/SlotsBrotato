using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightningEffect : MonoBehaviour
{
    public float duration = 0.08f;

    private LineRenderer _lr;

    void Awake()
    {
        _lr = GetComponent<LineRenderer>();
    }

    public void Setup(Vector2 start, Vector2 end)
    {
        _lr.SetPosition(0, start);
        _lr.SetPosition(1, end);

        StartCoroutine(DestroyRoutine());
    }

    IEnumerator DestroyRoutine()
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}