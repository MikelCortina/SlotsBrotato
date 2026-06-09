using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SegmentedChargeBar : MonoBehaviour
{
    [SerializeField] private List<Image> segments = new();

    [Header("Colors")]
    [SerializeField] private Color emptyColor = new Color(0.12f, 0.10f, 0.08f, 0.9f);
    [SerializeField] private Gradient mainGradient;
    [SerializeField] private Gradient overloadGradient;

    [Header("Animation")]
    [SerializeField] private float totalTransitionDuration = 0.2f;
    [SerializeField] private float colorLerpDuration = 0.08f;

    public int SegmentCount => segments.Count;

    int _visualMain;
    int _visualOverload;

    int _targetMain;
    int _targetOverload;

    Coroutine _animateRoutine;

    Color[] _currentColors;
    Color[] _targetColors;

    void Awake()
    {
        InitializeColorBuffers();

        _visualMain = 0;
        _visualOverload = 0;
        _targetMain = 0;
        _targetOverload = 0;

        UpdateTargetColors(_visualMain, _visualOverload, true);
    }

    void Reset()
    {
        segments.Clear();
        GetComponentsInChildren(true, segments);
    }

    void Update()
    {
        if (_currentColors == null || _targetColors == null || _currentColors.Length != segments.Count)
            InitializeColorBuffers();

        float t = colorLerpDuration > 0f ? Time.deltaTime / colorLerpDuration : 1f;
        t = Mathf.Clamp01(t);

        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] == null) continue;

            _currentColors[i] = Color.Lerp(_currentColors[i], _targetColors[i], t);
            segments[i].color = _currentColors[i];
        }
    }

    void InitializeColorBuffers()
    {
        int count = segments.Count;
        _currentColors = new Color[count];
        _targetColors = new Color[count];

        for (int i = 0; i < count; i++)
        {
            _currentColors[i] = emptyColor;
            _targetColors[i] = emptyColor;

            if (segments[i] != null)
                segments[i].color = emptyColor;
        }
    }

    public void SetByValues(float mainValue, float overloadValue, float maxValue)
    {
        if (maxValue <= 0f) maxValue = 1f;

        int total = segments.Count;

        int mainSegments = Mathf.Clamp(
            Mathf.RoundToInt((mainValue / maxValue) * total),
            0,
            total);

        int overloadSegments = Mathf.Clamp(
            Mathf.RoundToInt((overloadValue / maxValue) * total),
            0,
            mainSegments);

        SetTargetSegments(mainSegments, overloadSegments);
    }

    public void SetTargetSegments(int mainFilled, int overloadFilled)
    {
        int total = segments.Count;
        _targetMain = Mathf.Clamp(mainFilled, 0, total);
        _targetOverload = Mathf.Clamp(overloadFilled, 0, _targetMain);

        if (_animateRoutine != null)
            StopCoroutine(_animateRoutine);

        _animateRoutine = StartCoroutine(AnimateToTarget());
    }

    public void SetInstant(int mainFilled, int overloadFilled)
    {
        int total = segments.Count;
        _targetMain = Mathf.Clamp(mainFilled, 0, total);
        _targetOverload = Mathf.Clamp(overloadFilled, 0, _targetMain);

        _visualMain = _targetMain;
        _visualOverload = _targetOverload;

        UpdateTargetColors(_visualMain, _visualOverload, true);
    }

    IEnumerator AnimateToTarget()
    {
        int deltaMain = Mathf.Abs(_targetMain - _visualMain);
        int deltaOverload = Mathf.Abs(_targetOverload - _visualOverload);
        int totalSteps = deltaMain + deltaOverload;

        if (totalSteps == 0)
            yield break;

        float stepDelay = totalSteps > 0 ? totalTransitionDuration / totalSteps : totalTransitionDuration;
        stepDelay = Mathf.Max(0.001f, stepDelay);

        while (_visualMain != _targetMain || _visualOverload != _targetOverload)
        {
            if (_visualMain != _targetMain)
            {
                _visualMain += (_visualMain < _targetMain) ? 1 : -1;

                if (_visualOverload > _visualMain)
                    _visualOverload = _visualMain;
            }
            else if (_visualOverload != _targetOverload)
            {
                _visualOverload += (_visualOverload < _targetOverload) ? 1 : -1;
            }

            UpdateTargetColors(_visualMain, _visualOverload, false);
            yield return new WaitForSeconds(stepDelay);
        }

        _animateRoutine = null;
    }

    void UpdateTargetColors(int mainFilled, int overloadFilled, bool instant)
    {
        int total = segments.Count;
        mainFilled = Mathf.Clamp(mainFilled, 0, total);
        overloadFilled = Mathf.Clamp(overloadFilled, 0, mainFilled);

        for (int i = 0; i < total; i++)
        {
            Color newTarget;

            if (i >= mainFilled)
            {
                newTarget = emptyColor;
            }
            else
            {
                float t = total <= 1 ? 1f : (float)i / (total - 1);
                bool isOverloadSegment = i < overloadFilled;

                newTarget = isOverloadSegment
                    ? overloadGradient.Evaluate(t)
                    : mainGradient.Evaluate(t);
            }

            _targetColors[i] = newTarget;

            if (instant)
            {
                _currentColors[i] = newTarget;
                if (segments[i] != null)
                    segments[i].color = newTarget;
            }
        }
    }
}