using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button backButton;

    private void Start()
    {
        AudioManager manager = AudioManager.Instance;

        if (manager != null)
        {
            bgmSlider.value = manager.BgmVolume;
            seSlider.value = manager.SeVolume;
        }

        bgmSlider.onValueChanged.AddListener(OnBgmChanged);
        seSlider.onValueChanged.AddListener(OnSeChanged);
        creditsButton.onClick.AddListener(() => SceneManager.LoadScene(SceneNames.Credits));
        backButton.onClick.AddListener(() => SceneManager.LoadScene(SceneNames.Home));
    }

    private void OnBgmChanged(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetBgmVolume(value);
    }

    private void OnSeChanged(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetSeVolume(value);
    }
}