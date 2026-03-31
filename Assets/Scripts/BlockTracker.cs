using UnityEngine;
using System.Collections.Generic;

public class BlockTracker : MonoBehaviour
{
    public static BlockTracker Instance;

    public List<Rigidbody> allBlocks = new List<Rigidbody>();
    public float stabilityThreshold = 0.08f;

    public float requiredStableDuration = 0.5f;

    // Per-block stable timer
    private Dictionary<Rigidbody, float> stableTimers = new Dictionary<Rigidbody, float>();

    // Live stable count — so that it goes up and down with physics
    public int stableCount { get; private set; } = 0;

    private bool _chaosLatched = false;
    public bool IsChaosMode
    {
        get
        {
            if (!_chaosLatched && stableCount >= 2)
                _chaosLatched = true;
            return _chaosLatched;
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        int current = 0;
        foreach (Rigidbody rb in allBlocks)
        {
            if (rb == null) continue;
            if (rb.isKinematic) continue;

            bool belowThreshold = rb.linearVelocity.magnitude  < stabilityThreshold
                                && rb.angularVelocity.magnitude < stabilityThreshold;

            if (!stableTimers.ContainsKey(rb))
                stableTimers[rb] = 0f;

            if (belowThreshold)
            {
                stableTimers[rb] += Time.deltaTime;
                if (stableTimers[rb] >= requiredStableDuration)
                    current++;
            }
            else
            {
                stableTimers[rb] = 0f;
            }
        }
        stableCount = current;
    }

    public void RegisterBlock(Rigidbody rb)
    {
        if (rb != null && !allBlocks.Contains(rb))
        {
            allBlocks.Add(rb);
            stableTimers[rb] = 0f;
        }
    }

    public void ResetTracker()
    {
        allBlocks.Clear();
        stableTimers.Clear();
        stableCount    = 0;
        _chaosLatched  = false;
    }
}
