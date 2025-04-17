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
        root = GetComponent<UIDocument>().rootVisualElement;

        playButton = root.Q<Button>("PlayButton");
        quitButton = root.Q<Button>("QuitButton");

        playButton.clicked += OnPlayClicked;
        quitButton.clicked += OnQuitClicked;
    }

    private void OnPlayClicked()
    {
        SceneManager.LoadScene("SampleScene");
    }

    private void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
