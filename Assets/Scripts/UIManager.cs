using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
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
    private Image npcPortrait;

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
    private bool isMiniGameActive = false;

    private Dictionary<GameObject, VisualElement> minimapIcons = new Dictionary<GameObject, VisualElement>();
    
    [Header("Minimap Settings")]
    [Tooltip("Total world units covered horizontally by the minimap")]
    public float minimapCoverageWidth = 30f;
    [Tooltip("Total world units covered vertically by the minimap")]
    public float minimapCoverageHeight = 30f;

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
        if (root == null) root = uiDocument.rootVisualElement;
        if (root == null) return;

        // Clean existing clones if any
        var oldMini = root.Q("MiniGameOverlay");
        if (oldMini != null && miniGameOverlay == null) miniGameOverlay = oldMini.parent; 

        // Mini-game Overlay
        if (miniGameAsset != null && miniGameOverlay == null)
        {
            var container = miniGameAsset.Instantiate();
            container.style.position = Position.Absolute;
            container.style.width = Length.Percent(100);
            container.style.height = Length.Percent(100);
            container.pickingMode = PickingMode.Ignore; // Allow clicks to reach children
            root.Add(container);
            miniGameOverlay = container;
        }

        if (miniGameOverlay != null)
        {
            var actualOverlay = miniGameOverlay.Q("MiniGameOverlay");
            if (actualOverlay != null) actualOverlay.style.display = DisplayStyle.None;

            wordContainer = miniGameOverlay.Q<VisualElement>("WordContainer");
            miniTimerLabel = miniGameOverlay.Q<Label>("MiniTimer");
            currentSentenceLabel = miniGameOverlay.Q<Label>("CurrentSentence");
            hintLabel = miniGameOverlay.Q<Label>("Hint");
            npcPortrait = miniGameOverlay.Q<Image>("NPCPortrait");
        }

        // Evaluation Overlay
        var oldEval = root.Q("EvaluationOverlay");
        if (oldEval != null && evaluationOverlay == null) evaluationOverlay = oldEval.parent;

        if (evaluationAsset != null && evaluationOverlay == null)
        {
            var container = evaluationAsset.Instantiate();
            container.style.position = Position.Absolute;
            container.style.width = Length.Percent(100);
            container.style.height = Length.Percent(100);
            container.pickingMode = PickingMode.Ignore;
            root.Add(container);
            evaluationOverlay = container;
        }

        if (evaluationOverlay != null)
        {
            var actualOverlay = evaluationOverlay.Q("EvaluationOverlay");
            if (actualOverlay != null) actualOverlay.style.display = DisplayStyle.None;

            resultTitle = evaluationOverlay.Q<Label>("ResultTitle");
            resultMessage = evaluationOverlay.Q<Label>("ResultMessage");
            finalCleanliness = evaluationOverlay.Q<Label>("FinalCleanliness");
            finalEmpathy = evaluationOverlay.Q<Label>("FinalEmpathy");
            restartButton = evaluationOverlay.Q<Button>("RestartButton");
            
            if (restartButton != null)
            {
                restartButton.clicked -= RestartGame;
                restartButton.clicked += RestartGame;
            }
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

        if (isMiniGameActive)
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
            float scaleX = 150f / minimapCoverageWidth;
            float scaleY = 150f / minimapCoverageHeight;
            // Subtract half the size to ensure the icon's center aligns with the coordinate
            icon.style.left = 75f + (pos.x * scaleX) - (size / 2f);
            icon.style.top = 75f - (pos.y * scaleY) - (size / 2f);
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

        var actualOverlay = evaluationOverlay.Q("EvaluationOverlay");
        if (actualOverlay != null) actualOverlay.style.display = DisplayStyle.Flex;
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
        isMiniGameActive = true;
        
        var actualOverlay = miniGameOverlay.Q("MiniGameOverlay");
        if (actualOverlay != null) actualOverlay.style.display = DisplayStyle.Flex;

        // Set Initial Portrait
        if (npcPortrait != null && currentNPC.data != null)
            npcPortrait.sprite = currentNPC.data.faceNeutral;

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
        Debug.Log("Word Clicked: " + btn.text); // Added Debug
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
        if (!isMiniGameActive) return;
        isMiniGameActive = false;

        if (success)
        {
            GameManager.Instance.ModifyEmpathy(2);
            if (npcPortrait != null && currentNPC != null) npcPortrait.sprite = currentNPC.data.faceHappy;
        }
        else
        {
            GameManager.Instance.ModifyEmpathy(-10);
            if (npcPortrait != null && currentNPC != null) npcPortrait.sprite = currentNPC.data.faceAngry;
        }
        
        StartCoroutine(CloseMiniGameRoutine(success));
    }

    private IEnumerator CloseMiniGameRoutine(bool success)
    {
        yield return new WaitForSeconds(1.0f);

        if (currentNPC != null)
            currentNPC.OnInteractionEnd(success);

        var actualOverlay = miniGameOverlay.Q("MiniGameOverlay");
        if (actualOverlay != null) actualOverlay.style.display = DisplayStyle.None;

        GameManager.Instance.isInteracting = false;
        currentNPC = null;
    }
}