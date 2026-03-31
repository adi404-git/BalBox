using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Start, Playing, GameOver, GameWon }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState currentState = GameState.Start;
    public bool IsPlaying => currentState == GameState.Playing;

    [Header("Win Condition")]
    public float escapeHeight = 25f;

    [Header("Idle Penalty")]
    public float idleGracePeriod   = 8f;
    public float idlePenaltyRate   = 0.025f;
    public float maxIdleMultiplier = 1.5f;
    [HideInInspector] public float timeMultiplier = 1f;

    public float CurrentHeight  { get; private set; } = 0f;
    public float BestHeight     { get; private set; } = 0f;
    public float AllTimeBest    { get; private set; } = 0f;

    private const string AllTimeBestKey = "BalBox_AllTimeBestHeight";
    private float idleTimer = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        AllTimeBest = PlayerPrefs.GetFloat(AllTimeBestKey, 0f);
    }

    void Update()
    {
        if (currentState == GameState.Start)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                StartGame();
            return;
        }

        if (currentState != GameState.Playing) return;

        UpdateIdlePenalty();
        CheckWin();
    }

    public void UpdateCurrentHeight(float pivotY)
    {
        CurrentHeight = pivotY;
        if (CurrentHeight > BestHeight)
            BestHeight = CurrentHeight;
    }

    public void StartGame()
    {
        currentState   = GameState.Playing;
        Time.timeScale = 1f;
        idleTimer      = 0f;
        timeMultiplier = 1f;
        CurrentHeight  = 0f;
        BestHeight     = 0f;
        UIManager.Instance?.OnGameStarted();
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.8f, 0.8f, 0.8f); 
        DynamicGI.UpdateEnvironment();
    }

    public void TriggerWin()
    {
        if (currentState == GameState.GameWon || currentState == GameState.GameOver) return;
        currentState = GameState.GameWon;
        SaveScore();
        Time.timeScale = 0.5f;
    }

    public void TriggerGameOver()
    {
        if (currentState == GameState.GameOver || currentState == GameState.GameWon) return;
        currentState = GameState.GameOver;
        SaveScore();
        Time.timeScale = 0.35f;
    }

    void CheckWin()
    {
        if (BlockTracker.Instance == null) return;
        foreach (Rigidbody rb in BlockTracker.Instance.allBlocks)
        {
            if (rb == null || rb.isKinematic) continue;
            if (rb.position.y >= escapeHeight)
            {
                TriggerWin();
                return;
            }
        }
    }

    void UpdateIdlePenalty()
    {
        bool dropped = Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
        if (dropped)
        {
            idleTimer      = 0f;
            timeMultiplier = 1f;
        }
        else
        {
            idleTimer += Time.deltaTime;
            float penalty  = Mathf.Max(0f, idleTimer - idleGracePeriod);
            timeMultiplier = Mathf.Clamp(1f + penalty * idlePenaltyRate, 1f, maxIdleMultiplier);
        }
    }

    void SaveScore()
    {
        if (BestHeight > AllTimeBest)
        {
            AllTimeBest = BestHeight;
            PlayerPrefs.SetFloat(AllTimeBestKey, AllTimeBest);
            PlayerPrefs.Save();
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        int loadedSceneCount = SceneManager.sceneCount;
        string[] scenesToReload = new string[loadedSceneCount];
        
        for (int i = 0; i < loadedSceneCount; i++)
        {
            scenesToReload[i] = SceneManager.GetSceneAt(i).name;
        }

        SceneManager.LoadScene(scenesToReload[0], LoadSceneMode.Single);
        for (int i = 1; i < scenesToReload.Length; i++)
        {
            SceneManager.LoadScene(scenesToReload[i], LoadSceneMode.Additive);
        }
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}