using UnityEditor.Rendering;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform pivot;
    public float baseRadius = 8f;
    public float expansionFactor = 0.5f;
    public float mouseDragSpeedX = 3f;
    public float mouseDragSpeedY = 3f;
    public float keyboardOrbitSpeed = 100f;
    public float touchDragSpeed = 0.2f;

    //to store calculated angles according to input variables 
    public float resetSpeed =5f;
    private float orbitYaw=0f;
    private float Yaw =0f; //this is for the rotation one
    private float previosOrbitYaw=0f;
    private float Pitch = 30f;
    private bool isResetting;
    // Lets use LateUpdate so it happens after Update where the game physics is done
    void LateUpdate()
    {
        if(pivot==null) return; //just in case i forget putting the pivot so it does nothing instead of crashing.
        HandleInputs();
        UpdatePosition();
        UpdateRotation();
    }

    private void HandleInputs()
    {
        //for PC controls: 
        if (Input.GetMouseButton(1)||Input.GetMouseButton(0)) //GetMouseButton(1) means holding RightClick
        {
            Yaw+= Input.GetAxis("Mouse X")*mouseDragSpeedX;
            Pitch-=Input.GetAxis("Mouse Y")*mouseDragSpeedY;            
        }
        //this line helps move along circumference
        orbitYaw+=Input.GetAxis("Horizontal")*keyboardOrbitSpeed*Time.deltaTime;

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if(t.phase == TouchPhase.Moved)
                {
                    Yaw+=t.deltaPosition.x*touchDragSpeed;
                    Pitch-=t.deltaPosition.y*touchDragSpeed;
                }
        }
        
        Pitch=Mathf.Clamp(Pitch,-80f,80f); //so player doesnt get like, stuck in rotation i clamp it here.

        //Resetting camera 
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartReset();
        }
    }
    void UpdatePosition()
    {
        float radius = baseRadius + pivot.position.y*expansionFactor;

        Quaternion orbitRotation=Quaternion.Euler(0,orbitYaw,0);
        Vector3 offset = orbitRotation*new Vector3(0,0,-radius);

        transform.position=pivot.position+offset;
    }
    void UpdateRotation()
    {   
        // i want it to keep rotation aligned with orbit soo
        float deltaOrbit = orbitYaw-previosOrbitYaw;
        if ((!Input.GetMouseButton(1)) && (!Input.GetMouseButton(0)))
        {
            Yaw+=deltaOrbit;
        }
        previosOrbitYaw=orbitYaw;

        if (isResetting)
        {   
            // rotate smoothly to look at pivot tower.
            Quaternion targetRotation=Quaternion.LookRotation(pivot.position-transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation,targetRotation,resetSpeed*Time.deltaTime);

            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.5f)
            {
                isResetting = false;

                //Sync look angles after reset 
                Vector3 euler = transform.rotation.eulerAngles;
                Yaw=euler.y;
                Pitch=euler.x;       
            }
        }
        else
        {
            Quaternion lookRotation = Quaternion.Euler(Pitch,Yaw,0);
            transform.rotation=lookRotation;

        }
    }
    public void MovePivotUp(float amount)
    {
        pivot.position+=Vector3.up*amount;
    }

    public void StartReset()
    {
        isResetting=true;
    }
}
