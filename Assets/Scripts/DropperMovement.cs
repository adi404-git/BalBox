using UnityEngine;

public class DropperMovement : MonoBehaviour
{
    [Header("References")]
    public Transform pivot;

    [Header("Base Movement")]
    public float baseSpeed    = 1.2f;
    public float baseRadius   = 4f;
    public float heightOffset = 10f;

    [Header("Difficulty Scaling")]
    public float speedIncreaseFactor  = 0.06f;
    public float radiusIncreaseFactor = 0.08f;
    public float maxSpeedFactor  = 2.5f;
    public float maxRadiusFactor = 1.8f;

    [Header("Chaos Boost")]
    public float chaosBoostAmount = 1.15f;

    [Header("Smoothing")]
    public float smoothSpeed = 1.2f;

    private float initialPivotY;
    private float speedMult  = 1f;
    private float radiusMult = 1f;
    private float t = 0f;

    void Start()
    {
        initialPivotY = pivot != null ? pivot.position.y : 0f;
    }

    void Update()
    {
        if (pivot == null) return;
        // so player sees the dropper before they play
        if (GameManager.Instance != null)
        {
            GameState s = GameManager.Instance.currentState;
            if (s == GameState.GameOver || s == GameState.GameWon) return;
        }

        float heightDiff  = Mathf.Max(0f, pivot.position.y - initialPivotY);
        float heightScale = Mathf.Sqrt(heightDiff);

        float chaos  = (BlockTracker.Instance != null && BlockTracker.Instance.IsChaosMode)
                       ? chaosBoostAmount : 1f;
        float idle   = GameManager.Instance != null ? GameManager.Instance.timeMultiplier : 1f;

        float tSpeed = Mathf.Clamp((1f + heightScale * speedIncreaseFactor)  * chaos * idle, 1f, maxSpeedFactor);
        float tRad   = Mathf.Clamp((1f + heightScale * radiusIncreaseFactor) * chaos,        1f, maxRadiusFactor);

        speedMult  = Mathf.Lerp(speedMult,  tSpeed, Time.deltaTime * smoothSpeed);
        radiusMult = Mathf.Lerp(radiusMult, tRad,   Time.deltaTime * smoothSpeed);

        t += Time.deltaTime * baseSpeed * speedMult;

        float r = baseRadius * radiusMult;
        float x = Mathf.Sin(t) * r;
        float z = Mathf.Sin(t) * Mathf.Cos(t) * r;

        transform.position = pivot.position + new Vector3(x, heightOffset, z);
    }
}
