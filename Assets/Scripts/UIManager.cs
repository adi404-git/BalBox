using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    public GameObject startPanel;
    public GameObject cameraTutorialPanel;
    public GameObject hudPanel;
    public GameObject winPanel;
    public GameObject gameOverPanel;

    [Header("HUD")]
    public TextMeshProUGUI currentHeightText;
    public TextMeshProUGUI bestHeightText;
    public TextMeshProUGUI allTimeBestText;

    [Header("HUD Stat Bars")]
    public GameObject  statBarsContainer;
    public Slider      bouncinessBar;
    public Slider      tiltBar;

    [Header("HUD Chaos Flash")]
    public TextMeshProUGUI chaosText;

    [Header("HUD Idle Hint")]
    public GameObject idleHintObject;

    [Header("Win Panel")]
    public TextMeshProUGUI winHeightText;
    public TextMeshProUGUI winBestText;
    public TextMeshProUGUI winAllTimeText;
    public TextMeshProUGUI winNewRecordText;

    [Header("Game Over Panel")]
    public TextMeshProUGUI goHeightText;
    public TextMeshProUGUI goAllTimeText;
    public TextMeshProUGUI goNewRecordText;

    private DropManager dropper;

    private bool chaosFlashShown  = false;
    private bool tutorialDismissed = false;
    private float tutorialTimer   = 0f;
    private const float TutorialDuration = 5f;

    private bool  endPanelShown = false;
    private float endPanelTimer = 0f;
    private const float EndPanelDelay = 1.5f;

    private float idleTimer     = 0f;
    private bool  firstHintShown = false;
    private const float IdleFirst  = 5f;
    private const float IdleRepeat = 25f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        dropper = Object.FindFirstObjectByType<DropManager>();

        Show(startPanel);
        Hide(cameraTutorialPanel);
        Hide(hudPanel);
        Hide(winPanel);
        Hide(gameOverPanel);
        Hide(statBarsContainer);
        Hide(chaosText?.gameObject);
        Hide(idleHintObject);
        Hide(winNewRecordText?.gameObject);
        Hide(goNewRecordText?.gameObject);
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        GameState state = GameManager.Instance.currentState;

        if (state == GameState.Playing)
        {
            UpdateHUD();
            UpdateTutorial();
            UpdateIdleHint();
        }

        if (!endPanelShown && (state == GameState.GameOver || state == GameState.GameWon))
        {
            endPanelTimer += Time.unscaledDeltaTime;
            if (endPanelTimer >= EndPanelDelay)
            {
                endPanelShown = true;
                if (state == GameState.GameOver) ShowGameOverPanel();
                else ShowWinPanel();
            }
        }
    }

    public void OnGameStarted()
    {
        Hide(startPanel);
        Show(hudPanel);
        Show(cameraTutorialPanel);
        tutorialDismissed = false;
        tutorialTimer     = 0f;
        endPanelShown     = false;
        endPanelTimer     = 0f;
        chaosFlashShown   = false;
        firstHintShown    = false;
        idleTimer         = 0f;
    }

    public void OnGameWon()  {}
    public void OnGameOver() {}

    void UpdateHUD()
    {
        if (GameManager.Instance == null) return;

        float cur    = GameManager.Instance.CurrentHeight;
        float best   = GameManager.Instance.BestHeight;
        float allTime = GameManager.Instance.AllTimeBest;

        if (currentHeightText != null) currentHeightText.text = cur.ToString("F1")    + "m";
        if (bestHeightText    != null) bestHeightText.text    = "Best: " + best.ToString("F1")    + "m";
        if (allTimeBestText   != null) allTimeBestText.text   = "Record: " + allTime.ToString("F1") + "m";

        if (!chaosFlashShown && BlockTracker.Instance != null && BlockTracker.Instance.IsChaosMode)
        {
            chaosFlashShown = true;
            Show(statBarsContainer);
            if (chaosText != null)
            {
                Show(chaosText.gameObject);
                Invoke(nameof(HideChaosText), 3f);
            }
        }

        if (dropper != null)
        {
            if (bouncinessBar != null) bouncinessBar.value = dropper.CurrentBounciness;
            if (tiltBar       != null) tiltBar.value       = dropper.CurrentTilt;
        }
    }

    void HideChaosText() { Hide(chaosText?.gameObject); }

    void UpdateTutorial()
    {
        if (tutorialDismissed || cameraTutorialPanel == null || !cameraTutorialPanel.activeSelf) return;

        tutorialTimer += Time.deltaTime;

        bool usedCamera = Input.GetMouseButton(1)
                       || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)
                       || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow);

        if (tutorialTimer >= TutorialDuration || usedCamera)
        {
            tutorialDismissed = true;
            Hide(cameraTutorialPanel);
        }
    }

    void UpdateIdleHint()
    {
        if (idleHintObject == null) return;

        bool anyInput = Input.GetKeyDown(KeyCode.Space)
                     || Input.GetMouseButtonDown(0)
                     || Input.GetMouseButton(1)
                     || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)
                     || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow);

        if (anyInput)
        {
            idleTimer = 0f;
            Hide(idleHintObject);
        }
        else
        {
            idleTimer += Time.deltaTime;
            float threshold = firstHintShown ? IdleRepeat : IdleFirst;
            if (idleTimer >= threshold)
            {
                Show(idleHintObject);
                firstHintShown = true;
            }
        }
    }

    void ShowWinPanel()
    {
        Time.timeScale = 0f;
        Hide(hudPanel);
        Show(winPanel);

        float best    = GameManager.Instance.BestHeight;
        float allTime = GameManager.Instance.AllTimeBest;
        bool  newRec  = best >= allTime && best > 0.1f;

        if (winHeightText  != null) winHeightText.text  = "Height: " + best.ToString("F1") + "m";
        if (winBestText    != null) winBestText.text    = "This Run: " + best.ToString("F1") + "m";
        if (winAllTimeText != null) winAllTimeText.text = "Record: " + allTime.ToString("F1") + "m";
        if (winNewRecordText != null) winNewRecordText.gameObject.SetActive(newRec);

        AudioManager.Instance?.PlayWin();
    }

    void ShowGameOverPanel()
    {
        Time.timeScale = 0f;
        Hide(hudPanel);
        Show(gameOverPanel);

        float best    = GameManager.Instance.BestHeight;
        float allTime = GameManager.Instance.AllTimeBest;
        bool  newRec  = best >= allTime && best > 0.1f;

        if (goHeightText  != null) goHeightText.text  = "Height: " + best.ToString("F1") + "m";
        if (goAllTimeText != null) goAllTimeText.text = "Record: " + allTime.ToString("F1") + "m";
        if (goNewRecordText != null) goNewRecordText.gameObject.SetActive(newRec);

        AudioManager.Instance?.PlayGameOver();
    }

    void Show(GameObject g) { if (g != null) g.SetActive(true);  }
    void Hide(GameObject g) { if (g != null) g.SetActive(false); }
}