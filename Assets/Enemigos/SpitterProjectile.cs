using UnityEngine;

public class SpitterProjectile : MonoBehaviour
{
    public float speed = 6f;
    public float damage = 1f;
    public float lifetime = 4f;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public Sprite[] possibleSprites;

    [Header("Random Rotation")]
    public float minRandomRotation = -25f;
    public float maxRandomRotation = 25f;

    private Vector2 _direction;
    private bool _initialized;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        AssignRandomSprite();
        AssignRandomVisualRotation();
    }

    public void Init(Vector2 direction, float projectileDamage)
    {
        _direction = direction.normalized;
        damage = projectileDamage;
        _initialized = true;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!_initialized)
            return;

        transform.position += (Vector3)(_direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();

        if (health != null)
            health.TakeDamage(damage);

        Destroy(gameObject);
    }

    void AssignRandomSprite()
    {
        if (spriteRenderer == null || possibleSprites == null || possibleSprites.Length == 0)
            return;

        int randomIndex = Random.Range(0, possibleSprites.Length);
        spriteRenderer.sprite = possibleSprites[randomIndex];
    }

    void AssignRandomVisualRotation()
    {
        if (spriteRenderer == null)
            return;

        float randomZ = Random.Range(minRandomRotation, maxRandomRotation);
        spriteRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, randomZ);
    }
}