using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;
    private VisualElement settingsPanel;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
    }

    private void Start()
    {
        // Initialize audio settings from PlayerPrefs
        AudioSettings.Initialize();

        root = uiDocument.rootVisualElement;
        if (root == null) return;

        settingsPanel = root.Q<VisualElement>("SettingsPanel");

        // Play Button
        var playButton = root.Q<Button>("PlayButton");
        if (playButton != null)
        {
            playButton.clicked += OnPlayClicked;
        }

        // Settings Button
        var settingsButton = root.Q<Button>("SettingsButton");
        if (settingsButton != null)
        {
            settingsButton.clicked += OnSettingsClicked;
        }

        // Exit Button
        var exitButton = root.Q<Button>("ExitButton");
        if (exitButton != null)
        {
            exitButton.clicked += OnExitClicked;
        }

        // Close Settings Button
        var closeSettingsButton = root.Q<Button>("CloseSettingsButton");
        if (closeSettingsButton != null)
        {
            closeSettingsButton.clicked += OnCloseSettingsClicked;
        }

        // Volume Slider
        var volumeSlider = root.Q<Slider>("VolumeSlider");
        if (volumeSlider != null)
        {
            volumeSlider.value = AudioSettings.MasterVolume;
            volumeSlider.RegisterValueChangedCallback(evt =>
            {
                AudioSettings.MasterVolume = evt.newValue;
            });
        }

        // SFX Slider
        var sfxSlider = root.Q<Slider>("SFXSlider");
        if (sfxSlider != null)
        {
            sfxSlider.value = AudioSettings.SFXVolume;
            sfxSlider.RegisterValueChangedCallback(evt =>
            {
                AudioSettings.SFXVolume = evt.newValue;
            });
        }
    }

    private void OnPlayClicked()
    {
        GameManager.currentLevel = 1;
        SceneManager.LoadScene("MainGame");
    }

    private void OnSettingsClicked()
    {
        if (settingsPanel != null)
        {
            settingsPanel.style.display = DisplayStyle.Flex;
        }
    }

    private void OnCloseSettingsClicked()
    {
        if (settingsPanel != null)
        {
            settingsPanel.style.display = DisplayStyle.None;
        }
    }

    private void OnExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
