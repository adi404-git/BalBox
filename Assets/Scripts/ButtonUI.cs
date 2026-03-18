using UnityEngine;

public class ButtonUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
        
        if(!Application.isMobilePlatform)
        {gameObject.SetActive(false);}
    }

}
