using UnityEngine;

public class DropperMovement : MonoBehaviour
{
    public Transform pivot;
    public float radius = 5f;
    public float speed =2f;
    public float heightOffset=5f;
    private float t=0f;
    void Update()
    {
        if (pivot==null) return;
        
        //This is the funtion of infinity which i am trying to move the dropper as
        
        t+=Time.deltaTime*speed;
        float x = Mathf.Sin(t)*radius;
        float z = Mathf.Sin(t)*Mathf.Cos(t)*radius;

        Vector3 position = pivot.position+ new Vector3(x,heightOffset,z);
        transform.position=position;
    }
}
