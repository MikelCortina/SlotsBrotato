using UnityEngine;
using System.Collections.Generic;

public class TailChainInertia2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private Transform playerVisual;
    [SerializeField] private List<Transform> bones = new List<Transform>();

    [Header("Motion")]
    [SerializeField] private float maxAnglePerBone = 25f;
    [SerializeField] private float horizontalInfluence = 2.0f;
    [SerializeField] private float verticalInfluence = 1.25f;
    [SerializeField] private float followSpeed = 12f;
    [SerializeField] private float damping = 8f;
    [SerializeField] private float delayPerBone = 0.85f;
    [SerializeField] private float minSpeedForInertia = 0.05f;

    [Header("Clamp by Axis")]
    [SerializeField] private float maxHorizontalAngle = 20f;
    [SerializeField] private float maxVerticalAngle = 18f;

    [Header("Wave")]
    [SerializeField] private bool enableWave = true;
    [SerializeField] private float waveAmplitude = 6f;
    [SerializeField] private float waveSpeed = 8f;
    [SerializeField] private float waveOffsetPerBone = 0.45f;
    [SerializeField] private float minSpeedForWave = 0.15f;
    [SerializeField] private AnimationCurve waveFalloff = AnimationCurve.Linear(0f, 0.3f, 1f, 1f);

    private float[] currentAngles;
    private float[] angleVelocities;
    private Quaternion[] initialLocalRotations;

    void Awake()
    {
        if (bones == null || bones.Count < 2)
        {
            Debug.LogWarning("TailChainInertia2D necesita al menos 2 bones.");
            enabled = false;
            return;
        }

        currentAngles = new float[bones.Count];
        angleVelocities = new float[bones.Count];
        initialLocalRotations = new Quaternion[bones.Count];

        for (int i = 0; i < bones.Count; i++)
        {
            initialLocalRotations[i] = bones[i].localRotation;
            currentAngles[i] = 0f;
            angleVelocities[i] = 0f;
        }
    }

    void LateUpdate()
    {
        if (playerRb == null || playerVisual == null || bones == null || bones.Count < 2)
            return;

        Vector2 v = playerRb.linearVelocity;
        float speed = v.magnitude;

        bool hasInertiaMotion = speed > minSpeedForInertia;
        bool hasWaveMotion = enableWave && speed > minSpeedForWave;

        bones[0].localRotation = initialLocalRotations[0];

        if (!hasInertiaMotion)
            v = Vector2.zero;

        float facingSign = Mathf.Sign(playerVisual.localScale.x);

        float moveX = v.x * facingSign;
        float moveY = v.y;

        float horizontalAngle = -moveX * horizontalInfluence;
        horizontalAngle = Mathf.Clamp(horizontalAngle, -maxHorizontalAngle, maxHorizontalAngle);

        float verticalAngle = moveY * verticalInfluence;
        verticalAngle = Mathf.Clamp(verticalAngle, -maxVerticalAngle, maxVerticalAngle);

        float baseTarget = horizontalAngle + verticalAngle;
        baseTarget = Mathf.Clamp(baseTarget, -maxAnglePerBone, maxAnglePerBone);

        for (int i = 1; i < bones.Count; i++)
        {
            float influence = Mathf.Pow(delayPerBone, i - 1);
            float targetAngle = baseTarget * influence;

            currentAngles[i] = Mathf.SmoothDamp(
                currentAngles[i],
                targetAngle,
                ref angleVelocities[i],
                1f / followSpeed,
                Mathf.Infinity,
                Time.deltaTime
            );

            currentAngles[i] = Mathf.Lerp(
                currentAngles[i],
                targetAngle,
                damping * 0.1f * Time.deltaTime
            );

            float waveAngle = 0f;
            if (hasWaveMotion)
            {
                float normalizedIndex = (float)i / (bones.Count - 1);
                float falloff = waveFalloff.Evaluate(normalizedIndex);
                float phase = Time.time * waveSpeed + i * waveOffsetPerBone;
                waveAngle = Mathf.Sin(phase) * waveAmplitude * falloff;
            }

            Quaternion inertiaRot = Quaternion.Euler(0f, 0f, currentAngles[i]);
            Quaternion waveRot = Quaternion.Euler(0f, 0f, waveAngle);

            bones[i].localRotation = initialLocalRotations[i] * inertiaRot * waveRot;
        }
    }
}