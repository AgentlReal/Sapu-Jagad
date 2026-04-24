using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public VisualTreeAsset miniGameAsset;

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

    private List<string> targetWords;
    private List<string> currentAttempt;
    private float miniTimer = 10f;

    private Dictionary<GameObject, VisualElement> minimapIcons = new Dictionary<GameObject, VisualElement>();
    private float mapScale = 5f; // Units per pixel approx

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        uiDocument = GetComponent<UIDocument>();
    }

    private void Start()
    {
        root = uiDocument.rootVisualElement;
        
        timerLabel = root.Q<Label>("Timer");
        empathyLabel = root.Q<Label>("Empathy");
        cleanlinessLabel = root.Q<Label>("Cleanliness");
        minimapRoot = root.Q<VisualElement>("Minimap");

        // Instantiate Mini-game Overlay and hide it
        if (miniGameAsset != null)
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
        }

        Debug.Log("UIManager Started. HUD Labels: " + (timerLabel != null));
    }

    private void Update()
    {
        if (GameManager.Instance != null && timerLabel != null)
        {
            float time = GameManager.Instance.currentTime;
            int mins = Mathf.FloorToInt(time / 60);
            int secs = Mathf.FloorToInt(time % 60);
            timerLabel.text = string.Format("{0:00}:{1:00}", mins, secs);

            empathyLabel.text = "Empati: " + GameManager.Instance.empathyScore;
            cleanlinessLabel.text = "Kebersihan: " + Mathf.FloorToInt(GameManager.Instance.GetCleanlinessPercentage()) + "%";
        }

        UpdateMinimap();

        if (miniGameOverlay != null && miniGameOverlay.style.display == DisplayStyle.Flex)
        {
            miniTimer -= Time.deltaTime;
            miniTimerLabel.text = Mathf.CeilToInt(miniTimer).ToString();
            if (miniTimer <= 0)
            {
                EndMiniGame(false);
            }
        }
    }

    private void UpdateMinimap()
    {
        if (minimapRoot == null) return;

        // Simple approach: Find all objects with relevant tags
        UpdateIconsForTag("Player", Color.green, 6);
        UpdateIconsForTag("Trash", new Color(0.5f, 0.25f, 0), 4);
        UpdateIconsForTag("NPC", Color.magenta, 6);

        // Remove destroyed objects
        var toRemove = minimapIcons.Keys.Where(k => k == null).ToList();
        foreach (var k in toRemove)
        {
            var icon = minimapIcons[k];
            if (icon.parent != null) icon.parent.Remove(icon);
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

            var pos = obj.transform.position;
            // Center is (75, 75) in the 150x150 box
            icon.style.left = 75f + (pos.x * mapScale);
            icon.style.top = 75f - (pos.y * mapScale);
        }
    }

    public void StartMiniGame(TeguranData data)
    {
        if (miniGameOverlay == null) return;

        GameManager.Instance.isInteracting = true;
        miniGameOverlay.style.display = DisplayStyle.Flex;
        miniTimer = 10f;
        currentAttempt = new List<string>();
        currentSentenceLabel.text = "";

        targetWords = data.fullSentence.Split(' ', System.StringSplitOptions.RemoveEmptyEntries).ToList();
        List<string> scrambled = targetWords.OrderBy(x => Random.value).ToList();

        wordContainer.Clear();
        foreach (var word in scrambled)
        {
            Button btn = new Button();
            btn.text = word;
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
        if (success)
        {
            Debug.Log("Mini Game Success!");
            GameManager.Instance.ModifyEmpathy(2);
        }
        else
        {
            Debug.Log("Mini Game Fail!");
            GameManager.Instance.ModifyEmpathy(-10);
        }
        
        miniGameOverlay.style.display = DisplayStyle.None;
        GameManager.Instance.isInteracting = false;
    }
}