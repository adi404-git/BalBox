using UnityEngine;
using System.Collections;
public class DropManager : MonoBehaviour
{
    [Header("Block Prefabs")]
    public GameObject[] blockPrefabs;
    public bool spawnRandomly = true;

    [Header("Timing")]
    [Tooltip("Real seconds after dropping before next block appears. Not affected by slow-mo.")]
    public float dropCooldown = 1.8f;

    [Header("Chaos Mode — Scale")]
    public float chaosMinScale = 0.7f;
    public float chaosMaxScale = 1.4f;

    [Header("Chaos Mode — Tilt")]
    public float chaosTiltMax = 10f;

    [Header("Chaos Mode — Physics")]
    public float chaosMinBounciness = 0.05f;
    public float chaosMaxBounciness = 0.35f;

    [Header("Drop Momentum")]
    public float momentumFraction = 0.2f;

    [Header("LMB Click Detection")]
    public float lmbDragThreshold = 5f;

    // UI stat bars (0-1 normalised)
    public float CurrentBounciness { get; private set; } = 0f;
    public float CurrentTilt       { get; private set; } = 0f;

    private int       sequenceIndex = 0;
    private Rigidbody currentBlock;
    private bool      canDrop       = false;
    private bool      chaosNotified = false;

    private Vector3   lastDropperPos;
    private Vector3   dropperVelocity;
    private Vector2   lmbDownPos;

    private Coroutine spawnCoroutine;

    void Start()
    {
        lastDropperPos = transform.position;
        SpawnBlock();
    }

    void Update()
    {
        dropperVelocity = (transform.position - lastDropperPos) / Time.unscaledDeltaTime;
        lastDropperPos  = transform.position;

        // Track LMB down position always
        if (Input.GetMouseButtonDown(0))
            lmbDownPos = Input.mousePosition;

        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        HandleInput();
        CheckChaosNotification();
    }

    void HandleInput()
    {
        if (!canDrop || currentBlock == null) return;

        bool space    = Input.GetKeyDown(KeyCode.Space);
        bool lmbClick = Input.GetMouseButtonUp(0)
                     && Vector2.Distance(Input.mousePosition, lmbDownPos) <= lmbDragThreshold;

        if (space || lmbClick)
            DropBlock();
    }

    void DropBlock()
    {
        canDrop = false;

        if (currentBlock == null)
        {
            StartSpawn(0.1f);
            return;
        }

        currentBlock.transform.SetParent(null);
        currentBlock.isKinematic = false;

        Collider col = currentBlock.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        // Use unscaled velocity so momentum is correct even if timeScale != 1
        currentBlock.linearVelocity = new Vector3(
            dropperVelocity.x * momentumFraction,
            0f,
            dropperVelocity.z * momentumFraction
        );

        AudioManager.Instance?.PlayDrop();
        StartSpawn(dropCooldown);
    }

    void StartSpawn(float delay)
    {
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnAfterDelay(delay));
    }

    IEnumerator SpawnAfterDelay(float delay)
    {
        // If we used WaitForSeconds and timeScale went to 0, this would freeze.
        yield return new WaitForSecondsRealtime(delay);
        SpawnBlock();
    }

    void SpawnBlock()
    {
        if (blockPrefabs == null || blockPrefabs.Length == 0)
        {
            Debug.LogError("[DropManager] No prefabs assigned!");
            return;
        }

        GameObject prefab = spawnRandomly
            ? blockPrefabs[Random.Range(0, blockPrefabs.Length)]
            : blockPrefabs[sequenceIndex % blockPrefabs.Length];

        if (!spawnRandomly) sequenceIndex++;

        if (prefab == null)
        {
            Debug.LogError("[DropManager] Prefab slot is null.");
            StartSpawn(0.5f);
            return;
        }

        GameObject nb = Instantiate(prefab);

        Collider  col = nb.GetComponent<Collider>();
        Rigidbody rb  = nb.GetComponent<Rigidbody>();

        if (col == null || rb == null)
        {
            Debug.LogError("[DropManager] Prefab missing Collider or Rigidbody: " + prefab.name);
            Destroy(nb);
            StartSpawn(0.5f);
            return;
        }

        rb.isKinematic = true;
        col.enabled    = false;

        CurrentBounciness = 0f;
        CurrentTilt       = 0f;

        // chaos mode
        bool isChaos = BlockTracker.Instance != null && BlockTracker.Instance.IsChaosMode;
        if (isChaos)
        {
            float s = Random.Range(chaosMinScale, chaosMaxScale);
            nb.transform.localScale = new Vector3(s, s, s);

            float tx = Random.Range(-chaosTiltMax, chaosTiltMax);
            float tz = Random.Range(-chaosTiltMax, chaosTiltMax);
            nb.transform.localRotation = Quaternion.Euler(tx, Random.Range(0f, 360f), tz);

            float tiltDeg     = Vector3.Angle(Vector3.up, nb.transform.up);
            CurrentTilt       = Mathf.Clamp01(tiltDeg / (chaosTiltMax * 1.5f));

            float bRaw        = Random.Range(chaosMinBounciness, chaosMaxBounciness);
            CurrentBounciness = Mathf.InverseLerp(chaosMinBounciness, chaosMaxBounciness, bRaw);

            PhysicsMaterial mat = new PhysicsMaterial("Chaos");
            mat.bounciness      = bRaw;
            mat.dynamicFriction = Random.Range(0.3f, 0.8f);
            mat.frictionCombine = PhysicsMaterialCombine.Average;
            mat.bounceCombine   = PhysicsMaterialCombine.Maximum;
            col.material        = mat;
        }
        else
        {
            nb.transform.localRotation = Quaternion.identity;
        }

        //centering the blocks not as per transform position component
        col.enabled = true;
        Physics.SyncTransforms();
        nb.transform.position += transform.position - col.bounds.center;
        col.enabled = false;

        nb.transform.SetParent(this.transform);
        currentBlock = rb;

        BlockTracker.Instance?.RegisterBlock(rb);

        if (nb.GetComponent<BlockCollisionSound>() == null)
            nb.AddComponent<BlockCollisionSound>();

        canDrop = true;
    }

    void CheckChaosNotification()
    {
        if (!chaosNotified && BlockTracker.Instance != null && BlockTracker.Instance.IsChaosMode)
        {
            chaosNotified = true;
            AudioManager.Instance?.PlayChaos();
            Debug.Log("[DropManager] CHAOS MODE ACTIVATED");
        }
    }
}
