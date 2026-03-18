using UnityEngine;

public class DropManager : MonoBehaviour
{
    public Transform pivot;
    public float heightAbovePivot=10f;
    public float swingWidth=4f;
    public float swingDepth=2f;
    public float swingSpeed=1.5f;

    public GameObject[] blockPrefabs; //this is my array of prefabs
    public bool spawnRandomly = true;
    private int  sequenceIndex = 0;
    private Rigidbody currentBlock;
    private bool canDrop = false;

    void Start()
    {
        SpawnNextBlock();
    }
    void Update()
    {
        MoveDropper();
        HandleInput();
    }

    private void MoveDropper()
    {
        if (pivot==null) return;
//this is from lisajous curve equations for a graph that draws the letter 8 centered at origin
        float x = Mathf.Sin(Time.time * swingSpeed) * swingWidth;
        float z = Mathf.Sin(Time.time * swingSpeed*2f)*swingDepth;

        Vector3 newPosition = new Vector3(
            pivot.position.x+x,
            pivot.position.y + heightAbovePivot,
            pivot.position.z+z
        );
        transform.position = newPosition;

    }
    private void HandleInput()
    {
        if(!canDrop||currentBlock==null) return;
        if (Input.GetKeyDown(KeyCode.Space)||(Input.touchCount>0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            DropBlock();
        }
    }
    private void DropBlock()
    {
        canDrop=false;

        currentBlock.transform.SetParent(null); // I simply unparent the prefab so now it behaves independently
        currentBlock.isKinematic=false;


        //just for fun-if it doesnt work comment this -- Dropper momentum onto block.
        float vx = Mathf.Cos(Time.time * swingSpeed) * swingWidth * swingSpeed;
        float vz = Mathf.Cos(Time.time * swingSpeed * 2f) * swingDepth * swingSpeed * 2f;
        currentBlock.linearVelocity= new Vector3(vx, 0, vz);

        Invoke("SpawnNextBlock",2f);

    }
    private void SpawnNextBlock()
    {
        GameObject prefabToSpawn;
        if (spawnRandomly)
        {
            int randomIndex = Random.Range(0,blockPrefabs.Length);
            prefabToSpawn=blockPrefabs[randomIndex];
        }
        else
        {
            prefabToSpawn = blockPrefabs[sequenceIndex]; //If i dont want it randomly then it will drop in the order the blocks are given
            sequenceIndex++;
            if (sequenceIndex >= blockPrefabs.Length)
                {
                    sequenceIndex = 0;
                }
        }
        GameObject newBlock = Instantiate(prefabToSpawn,transform.position,transform.rotation);
        newBlock.transform.SetParent(this.transform);

        currentBlock = newBlock.GetComponent<Rigidbody>();
        currentBlock.isKinematic = true; //so it defies gravity until dropper enables it to

        canDrop=true;
    }
}
