using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuManager : MonoBehaviour
{
    private VisualElement root;
    private Button playButton;
    private Button optionsButton;
    private Button quitButton;
    private Button optionsBackButton;
    private Slider volumeSlider;
    private Slider brightnessSlider;
    private VisualElement optionsPanel;

    void OnEnable()
    {
        // Get the root UI element from the UIDocument.
        root = GetComponent<UIDocument>().rootVisualElement;

        // Query UI elements by name defined in the UXML.
        playButton = root.Q<Button>("PlayButton");
        optionsButton = root.Q<Button>("OptionsButton");
        quitButton = root.Q<Button>("QuitButton");
        optionsBackButton = root.Q<Button>("OptionsBackButton");
        volumeSlider = root.Q<Slider>("VolumeSlider");
        brightnessSlider = root.Q<Slider>("BrightnessSlider");
        optionsPanel = root.Q<VisualElement>("OptionsPanel");

        // Hook up button events.
        playButton.clicked += OnPlayClicked;
        optionsButton.clicked += OnOptionsClicked;
        quitButton.clicked += OnQuitClicked;
        optionsBackButton.clicked += OnOptionsBackClicked;

        // Register slider callbacks to update settings.
        volumeSlider.RegisterValueChangedCallback(evt =>
        {
            AudioListener.volume = evt.newValue;
        });
        brightnessSlider.RegisterValueChangedCallback(evt =>
        {
            RenderSettings.ambientIntensity = evt.newValue;
        });

        // Hide the Options Panel initially.
        optionsPanel.style.display = DisplayStyle.None;
    }

    private void OnPlayClicked()
    {
        // Fade-out animation could be added here before scene change.
        SceneManager.LoadScene("SampleScene");
    }

    private void OnOptionsClicked()
    {
        // Show the Options Panel.
        optionsPanel.style.display = DisplayStyle.Flex;
    }

    private void OnOptionsBackClicked()
    {
        // Hide the Options Panel.
        optionsPanel.style.display = DisplayStyle.None;
    }

    private void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
