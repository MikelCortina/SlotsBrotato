using UnityEngine;

public class CoinDropController : MonoBehaviour
{
    [Header("Fall")]
    [SerializeField] private float gravity = 30f;
    [SerializeField] private float fallDistanceBeforeStop = 1.5f;
    [SerializeField] private float bounceHeight = 0.25f;
    [SerializeField] private float bounceGravity = 25f;

    private Vector2 velocity;
    private float initialY;
    private bool isStopped = false;
    private bool isPulled = false;
    private bool isBouncing = false;

    void Start()
    {
        initialY = transform.position.y;
        velocity = Vector2.zero;
    }

    void Update()
    {
        if (isPulled) return;
        if (isStopped) return;

        if (isBouncing)
        {
            velocity.y -= bounceGravity * Time.deltaTime;
            transform.position += (Vector3)velocity * Time.deltaTime;

            float stopY = initialY - fallDistanceBeforeStop;
            if (transform.position.y <= stopY && velocity.y < 0)
            {
                velocity.y = 0;
                if (transform.position.y < stopY)
                    transform.position = new Vector3(transform.position.x, stopY, transform.position.z);
                isBouncing = false;
                isStopped = true;
            }
            return;
        }

        velocity.y -= gravity * Time.deltaTime;

        Vector3 nextPos = transform.position + (Vector3)velocity * Time.deltaTime;

        float distanceFallen = initialY - nextPos.y;

        if (distanceFallen >= fallDistanceBeforeStop && velocity.y < 0)
        {
            if (nextPos.y < initialY - fallDistanceBeforeStop)
            {
                nextPos.y = initialY - fallDistanceBeforeStop;
            }
            transform.position = nextPos;
            velocity = Vector2.up * Mathf.Sqrt(2 * bounceGravity * bounceHeight);
            isBouncing = true;
            return;
        }

        transform.position = nextPos;
    }

    public void StartBeingPulled()
    {
        isPulled = true;
        velocity = Vector2.zero;
        isBouncing = false;
    }
}