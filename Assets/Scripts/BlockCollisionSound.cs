using UnityEngine;

public class BlockCollisionSound : MonoBehaviour
{
    public float minImpactVelocity = 1.5f;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude >= minImpactVelocity)
            AudioManager.Instance?.PlayHit();
    }
}
