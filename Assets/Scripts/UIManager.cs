using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public VisualTreeAsset miniGameAsset;
    public VisualTreeAsset evaluationAsset;

    private UIDocument uiDocument;
    private VisualElement root;

    // HUD Elements
    private Label timerLabel;
    private Label empathyLabel;
    private Label cleanlinessLabel;
    private VisualElement minimapRoot;

    // Mini-game Elements
    private VisualElement miniGameOverlay;
    private VisualElement wordContainer;
    private Label miniTimerLabel;
    private Label currentSentenceLabel;
    private Label hintLabel;

    // Evaluation Elements
    private VisualElement evaluationOverlay;
    private Label resultTitle;
    private Label resultMessage;
    private Label finalCleanliness;
    private Label finalEmpathy;
    private Button restartButton;

    private List<string> targetWords;
    private List<string> currentAttempt;
    private float miniTimer = 10f;
    private NPCBehavior currentNPC;

    private Dictionary<GameObject, VisualElement> minimapIcons = new Dictionary<GameObject, VisualElement>();
    private float mapScale = 5f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        uiDocument = GetComponent<UIDocument>();
    }

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        
        if (root == null) return;

        timerLabel = root.Q<Label>("Timer");
        empathyLabel = root.Q<Label>("Empathy");
        cleanlinessLabel = root.Q<Label>("Cleanliness");
        minimapRoot = root.Q<VisualElement>("Minimap");

        InitializeOverlays();
    }

    private void InitializeOverlays()
    {
        if (root == null) return;

        // Mini-game Overlay
        if (miniGameAsset != null && miniGameOverlay == null)
        {
            miniGameOverlay = miniGameAsset.Instantiate();
            miniGameOverlay.style.position = Position.Absolute;
            miniGameOverlay.style.width = Length.Percent(100);
            miniGameOverlay.style.height = Length.Percent(100);
            miniGameOverlay.style.display = DisplayStyle.None;
            root.Add(miniGameOverlay);
            
            wordContainer = miniGameOverlay.Q<VisualElement>("WordContainer");
            miniTimerLabel = miniGameOverlay.Q<Label>("MiniTimer");
            currentSentenceLabel = miniGameOverlay.Q<Label>("CurrentSentence");
            hintLabel = miniGameOverlay.Q<Label>("Hint");
        }

        // Evaluation Overlay
        if (evaluationAsset != null && evaluationOverlay == null)
        {
            evaluationOverlay = evaluationAsset.Instantiate();
            evaluationOverlay.style.position = Position.Absolute;
            evaluationOverlay.style.width = Length.Percent(100);
            evaluationOverlay.style.height = Length.Percent(100);
            evaluationOverlay.style.display = DisplayStyle.None;
            root.Add(evaluationOverlay);

            resultTitle = evaluationOverlay.Q<Label>("ResultTitle");
            resultMessage = evaluationOverlay.Q<Label>("ResultMessage");
            finalCleanliness = evaluationOverlay.Q<Label>("FinalCleanliness");
            finalEmpathy = evaluationOverlay.Q<Label>("FinalEmpathy");
            restartButton = evaluationOverlay.Q<Button>("RestartButton");
            
            if (restartButton != null)
                restartButton.clicked += RestartGame;
        }
    }

    private void Update()
    {
        if (root == null)
        {
            InitializeUI();
            return;
        }

        if (GameManager.Instance != null && !GameManager.Instance.isGameOver)
        {
            if (timerLabel != null)
            {
                float time = Mathf.Max(0, GameManager.Instance.currentTime);
                int mins = Mathf.FloorToInt(time / 60);
                int secs = Mathf.FloorToInt(time % 60);
                timerLabel.text = string.Format("{0:00}:{1:00}", mins, secs);
            }

            if (empathyLabel != null)
                empathyLabel.text = "Empati: " + GameManager.Instance.empathyScore;
            
            if (cleanlinessLabel != null)
                cleanlinessLabel.text = "Kebersihan: " + Mathf.FloorToInt(GameManager.Instance.GetCleanlinessPercentage()) + "%";
        }

        UpdateMinimap();

        if (miniGameOverlay != null && miniGameOverlay.style.display == DisplayStyle.Flex)
        {
            miniTimer -= Time.deltaTime;
            if (miniTimerLabel != null) miniTimerLabel.text = Mathf.CeilToInt(miniTimer).ToString();
            if (miniTimer <= 0) EndMiniGame(false);
        }
    }

    private void UpdateMinimap()
    {
        if (minimapRoot == null) return;

        UpdateIconsForTag("Player", Color.green, 8);
        UpdateIconsForTag("Trash", new Color(0.5f, 0.25f, 0), 4);
        UpdateIconsForTag("NPC", Color.magenta, 8);

        var toRemove = minimapIcons.Keys.Where(k => k == null).ToList();
        foreach (var k in toRemove)
        {
            var icon = minimapIcons[k];
            if (icon != null && icon.parent != null) icon.parent.Remove(icon);
            minimapIcons.Remove(k);
        }
    }

    private void UpdateIconsForTag(string tag, Color color, float size)
    {
        var objs = GameObject.FindGameObjectsWithTag(tag);
        foreach (var obj in objs)
        {
            VisualElement icon;
            if (!minimapIcons.TryGetValue(obj, out icon))
            {
                icon = new VisualElement();
                icon.style.width = size;
                icon.style.height = size;
                icon.style.backgroundColor = color;
                icon.style.position = Position.Absolute;
                minimapRoot.Add(icon);
                minimapIcons[obj] = icon;
            }

            Vector3 pos = obj.transform.position;
            icon.style.left = 75f + (pos.x * mapScale);
            icon.style.top = 75f - (pos.y * mapScale);
        }
    }

    public void ShowEvaluation(float cleanliness, float empathy)
    {
        if (evaluationOverlay == null) InitializeOverlays();
        if (evaluationOverlay == null) return;

        bool win = cleanliness >= 70f && empathy >= 80f;

        if (resultTitle != null) resultTitle.text = win ? "Pahlawan Kebersihan!" : "Gagal Menjaga Taman";
        if (resultMessage != null) resultMessage.text = win ? "Selamat! Pak Darmo berhasil menjaga kebersihan dan hati warga." : "Maaf, taman masih kotor atau warga merasa tidak nyaman.";
        if (finalCleanliness != null) finalCleanliness.text = "Kebersihan Akhir: " + Mathf.FloorToInt(cleanliness) + "%";
        if (finalEmpathy != null) finalEmpathy.text = "Empati Akhir: " + Mathf.FloorToInt(empathy);

        evaluationOverlay.style.display = DisplayStyle.Flex;
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void StartMiniGame(TeguranData data, NPCBehavior npc)
    {
        if (miniGameOverlay == null) InitializeOverlays();
        if (miniGameOverlay == null) return;

        currentNPC = npc;
        GameManager.Instance.isInteracting = true;
        miniGameOverlay.style.display = DisplayStyle.Flex;

        miniTimer = 10f;
        currentAttempt = new List<string>();
        currentSentenceLabel.text = "";
        
        if (hintLabel != null) hintLabel.text = "Hint: " + data.fullSentence;

        targetWords = data.fullSentence.Split(' ', System.StringSplitOptions.RemoveEmptyEntries).ToList();
        List<string> scrambled = targetWords.OrderBy(x => Random.value).ToList();

        wordContainer.Clear();
        foreach (var word in scrambled)
        {
            Button btn = new Button { text = word };
            btn.style.marginRight = 5;
            btn.style.marginBottom = 5;
            btn.clicked += () => OnWordClicked(btn);
            wordContainer.Add(btn);
        }
    }

    private void OnWordClicked(Button btn)
    {
        currentAttempt.Add(btn.text);
        currentSentenceLabel.text = string.Join(" ", currentAttempt);
        btn.SetEnabled(false);

        if (currentAttempt.Count == targetWords.Count)
        {
            EndMiniGame(currentAttempt.SequenceEqual(targetWords));
        }
    }

    private void EndMiniGame(bool success)
    {
        if (success) GameManager.Instance.ModifyEmpathy(2);
        else GameManager.Instance.ModifyEmpathy(-10);
        
        if (currentNPC != null) currentNPC.OnInteractionEnd(success);

        miniGameOverlay.style.display = DisplayStyle.None;
        GameManager.Instance.isInteracting = false;
        currentNPC = null;
    }
}