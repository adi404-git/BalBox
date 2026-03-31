using UnityEngine;
public class IdleHint : MonoBehaviour
{
    public GameObject hintUI;
    public float firstDelay  = 5f;
    public float repeatDelay = 25f;

    private float timer     = 0f;
    private bool  firstShown = false;

    void Update()
    {
        if (hintUI == null) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        bool anyInput = Input.GetKeyDown(KeyCode.Space)
                     || Input.GetMouseButtonDown(0)
                     || Input.GetMouseButton(1)
                     || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)
                     || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow);

        if (anyInput)
        {
            timer = 0f;
            hintUI.SetActive(false);
        }
        else
        {
            timer += Time.deltaTime;
            float threshold = firstShown ? repeatDelay : firstDelay;
            if (timer >= threshold)
            {
                hintUI.SetActive(true);
                firstShown = true;
            }
        }
    }
}
