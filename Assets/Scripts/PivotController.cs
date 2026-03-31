using UnityEngine;

public class PivotController : MonoBehaviour
{
    [Header("Smoothing")]
    public float smoothTime = 0.5f;
    public float maxSpeed   = 5f;

    [Header("XZ Drift Cap")]
    public float maxXZDrift = 3f;

    [Header("Stability Gate")]
    public float sameHeightTolerance = 0.15f;

    private Vector3 targetPosition;
    private Vector3 smoothVelocity;
    private Vector3 originXZ;

    void Start()
    {
        targetPosition = transform.position;
        originXZ       = new Vector3(transform.position.x, 0f, transform.position.z);
    }

    void LateUpdate()
    {
        FindTarget();
        transform.position = Vector3.SmoothDamp(
            transform.position, targetPosition, ref smoothVelocity, smoothTime, maxSpeed);

        if (GameManager.Instance != null)
            GameManager.Instance.UpdateCurrentHeight(transform.position.y);
    }

    void FindTarget()
    {
        if (BlockTracker.Instance == null) return;

        float     bestY = float.MinValue;
        Rigidbody best  = null;

        foreach (Rigidbody rb in BlockTracker.Instance.allBlocks)
        {
            if (rb == null || rb.isKinematic) continue;

            bool stable = rb.linearVelocity.magnitude  < BlockTracker.Instance.stabilityThreshold
                       && rb.angularVelocity.magnitude < BlockTracker.Instance.stabilityThreshold;
            if (!stable) continue;

            Collider col    = rb.GetComponent<Collider>();
            Vector3  center = col != null ? col.bounds.center : rb.position;
            float    y      = center.y;

            if (y > bestY)
            {
                bestY = y; best = rb;
            }
            else if (Mathf.Abs(y - bestY) < sameHeightTolerance && best != null)
            {
                Collider bc  = best.GetComponent<Collider>();
                Vector3  bc_ = bc  != null ? bc.bounds.center  : best.position;
                Vector3  c_  = col != null ? col.bounds.center : rb.position;
                float dNew = Vector2.Distance(new Vector2(c_.x,  c_.z),  new Vector2(transform.position.x, transform.position.z));
                float dOld = Vector2.Distance(new Vector2(bc_.x, bc_.z), new Vector2(transform.position.x, transform.position.z));
                if (dNew < dOld) { bestY = y; best = rb; }
            }
        }

        if (best == null) return;

        Collider wc  = best.GetComponent<Collider>();
        Vector3  win = wc != null ? wc.bounds.center : best.position;

        Vector3 xzOff = new Vector3(win.x - originXZ.x, 0f, win.z - originXZ.z);
        if (xzOff.magnitude > maxXZDrift)
            xzOff = xzOff.normalized * maxXZDrift;

        targetPosition = new Vector3(originXZ.x + xzOff.x, win.y, originXZ.z + xzOff.z);
    }
}