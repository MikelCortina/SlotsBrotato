using UnityEngine;
using UnityEngine.UI;

public class RotateToMouseRenderTexture : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private WeaponPivotAim weaponAim;
    [SerializeField] private SpriteRenderer spriteRendererToFlip;

    [Header("Animator cuerpo")]
    [SerializeField] private Animator bodyAnimator;
    [SerializeField] private int animatorLayer = 0;
    [SerializeField] private string idleStateName = "Base Layer.Idle";

    [Header("Rotación")]
    [SerializeField] private float angleOffset = 0f;
    [SerializeField] private float maxUpAngle = 60f;
    [SerializeField] private float maxDownAngle = 60f;

    [Header("Estabilidad del flip")]
    [SerializeField] private float flipDeadZone = 0.15f;

    [Header("Offset al mirar a la izquierda")]
    [SerializeField] private float leftOffsetX = -0.05f;

    [Header("Offset al mirar hacia abajo")]
    [SerializeField] private float downForwardOffsetY = -0.06f;
    [SerializeField] private float downForwardOffsetX = 0.02f;

    [Header("Offset al mirar hacia arriba")]
    [SerializeField] private float upOffsetY = 0.04f;
    [SerializeField] private float upOffsetX = 0.00f;

    [Header("Idle bobbing cabeza")]
    [SerializeField] private bool useIdleBobbing = true;
    [SerializeField] private Vector2 idleBobAmount = new Vector2(0f, 0.03f);
    [SerializeField] private float idleBobSpeedMultiplier = 1f;
    [SerializeField] private AnimationCurve idleBobCurve = AnimationCurve.EaseInOut(0, -1, 1, 1);

    private Vector3 initialLocalPosition;
    private int idleStateHash;
    private bool lastMouseOnLeft = false;

    void Awake()
    {
        initialLocalPosition = transform.localPosition;
        idleStateHash = Animator.StringToHash(idleStateName);
    }

    void LateUpdate()
    {
        if (weaponAim == null)
            return;

        if (!weaponAim.TryGetMouseWorldPosition(out Vector3 mouseWorld))
            return;

        Vector2 dir = mouseWorld - transform.position;

        if (dir.sqrMagnitude <= 0.0001f)
            return;

        float rawAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        bool mouseOnLeft = lastMouseOnLeft;

        if (dir.x < -flipDeadZone)
            mouseOnLeft = true;
        else if (dir.x > flipDeadZone)
            mouseOnLeft = false;

        lastMouseOnLeft = mouseOnLeft;

        float finalAngle;

        if (!mouseOnLeft)
        {
            finalAngle = Mathf.Clamp(rawAngle, -maxDownAngle, maxUpAngle);
        }
        else
        {
            float relativeToLeft = Mathf.DeltaAngle(180f, rawAngle);
            relativeToLeft = Mathf.Clamp(relativeToLeft, -maxUpAngle, maxDownAngle);
            finalAngle = 180f + relativeToLeft;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, finalAngle + angleOffset);

        if (spriteRendererToFlip != null)
            spriteRendererToFlip.flipY = mouseOnLeft;

        Vector3 targetLocalPos = initialLocalPosition;

        if (mouseOnLeft)
            targetLocalPos.x += leftOffsetX;

        float upwardT = 0f;
        float downwardT = 0f;

        if (!mouseOnLeft)
        {
            if (finalAngle > 0f)
                upwardT = Mathf.InverseLerp(0f, maxUpAngle, finalAngle);
            else if (finalAngle < 0f)
                downwardT = Mathf.InverseLerp(0f, -maxDownAngle, finalAngle);
        }
        else
        {
            float relativeToLeft = Mathf.DeltaAngle(180f, finalAngle);

            if (relativeToLeft < 0f)
                upwardT = Mathf.InverseLerp(0f, -maxUpAngle, relativeToLeft);
            else if (relativeToLeft > 0f)
                downwardT = Mathf.InverseLerp(0f, maxDownAngle, relativeToLeft);
        }

        targetLocalPos.y += upOffsetY * upwardT;
        targetLocalPos.y += downForwardOffsetY * downwardT;

        if (mouseOnLeft)
        {
            targetLocalPos.x -= upOffsetX * upwardT;
            targetLocalPos.x -= downForwardOffsetX * downwardT;
        }
        else
        {
            targetLocalPos.x += upOffsetX * upwardT;
            targetLocalPos.x += downForwardOffsetX * downwardT;
        }

        if (ShouldApplyIdleBobbing())
        {
            targetLocalPos += GetIdleBobOffset();
        }

        transform.localPosition = targetLocalPos;
    }

    private bool ShouldApplyIdleBobbing()
    {
        if (!useIdleBobbing || bodyAnimator == null)
            return false;

        if (bodyAnimator.IsInTransition(animatorLayer))
            return false;

        AnimatorStateInfo stateInfo = bodyAnimator.GetCurrentAnimatorStateInfo(animatorLayer);
        return stateInfo.fullPathHash == idleStateHash;
    }

    private Vector3 GetIdleBobOffset()
    {
        AnimatorStateInfo stateInfo = bodyAnimator.GetCurrentAnimatorStateInfo(animatorLayer);

        float t = Mathf.Repeat(stateInfo.normalizedTime * idleBobSpeedMultiplier, 1f);
        float curveValue = idleBobCurve.Evaluate(t);

        return new Vector3(
            idleBobAmount.x * curveValue,
            idleBobAmount.y * curveValue,
            0f
        );
    }
}