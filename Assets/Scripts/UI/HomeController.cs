using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomeController : MonoBehaviour
{
    [SerializeField] private Button cpuBattleButton;
    [SerializeField] private Button settingsButton;

    private void Start()
    {
        cpuBattleButton.onClick.AddListener(() => SceneManager.LoadScene(SceneNames.FactionSelect));
        settingsButton.onClick.AddListener(() => SceneManager.LoadScene(SceneNames.Settings));
    }
}