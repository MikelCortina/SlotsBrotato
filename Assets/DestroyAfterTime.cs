using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float lifetime = 0.15f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}