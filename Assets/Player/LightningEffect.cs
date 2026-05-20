using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightningEffect : MonoBehaviour
{
    public float duration = 0.05f;
    public int segments = 10;
    public float jitter = 0.35f;
    public float thickness = 0.12f;
    public float segmentDisappearTime = 0.01f;
    public ParticleSystem impactParticles;

    LineRenderer _lr;
    readonly List<Vector3> _points = new List<Vector3>();

    void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        _lr.useWorldSpace = true;
    }

    public void Setup(Vector2 start, Vector2 end)
    {
        Vector2 dir = end - start;
        Vector2 perp = Vector2.Perpendicular(dir).normalized;

        _points.Clear();

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector2 p = Vector2.Lerp(start, end, t);

            if (i != 0 && i != segments)
            {
                float falloff = 1f - Mathf.Abs(t * 2f - 1f);
                p += perp * Random.Range(-jitter, jitter) * falloff;
            }

            _points.Add(new Vector3(p.x, p.y, transform.position.z));
        }

        _lr.positionCount = _points.Count;
        _lr.startWidth = thickness;
        _lr.endWidth = thickness;
        _lr.SetPositions(_points.ToArray());

        if (impactParticles != null)
            Instantiate(impactParticles, new Vector3(end.x, end.y, transform.position.z), Quaternion.identity);

        StartCoroutine(ShrinkFromStart());
    }

    IEnumerator ShrinkFromStart()
    {
        while (_points.Count > 1)
        {
            yield return new WaitForSeconds(segmentDisappearTime);

            _points.RemoveAt(0);
            _lr.positionCount = _points.Count;
            _lr.SetPositions(_points.ToArray());
        }

        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}