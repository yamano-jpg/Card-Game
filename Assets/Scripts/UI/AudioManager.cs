using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private const string BgmKey = "BgmVolume";
    private const string SeKey = "SeVolume";

    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;
    [SerializeField] private AudioSource jingleSource;  
    [SerializeField] private AudioClip[] homeBgms;
    [SerializeField] private AudioClip[] battleBgms;
    [SerializeField] private AudioClip clickSe;

    public float BgmVolume { get; private set; }
    public float SeVolume { get; private set; }

    // true の間は、ボタンを押したときの自動のクリック音を鳴らさない(戦闘中に使う)
    public bool SuppressAutoClick { get; set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BgmVolume = PlayerPrefs.GetFloat(BgmKey, 0.4f);
        SeVolume = PlayerPrefs.GetFloat(SeKey, 0.5f);
        ApplyVolume();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (Instance != this) return;

        HookButtons();
        PlayBgmFrom(homeBgms);
    }

    private void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 画面が切り替わったら、ジングルは止める
        jingleSource.Stop();

        HookButtons();

        if (scene.name == SceneNames.Battle)
        {
            PlayBgmFrom(battleBgms);
        }
        else if (scene.name == SceneNames.Home
              || scene.name == SceneNames.FactionSelect
              || scene.name == SceneNames.Settings
              || scene.name == SceneNames.Credits)
        {
            PlayBgmFrom(homeBgms);
        }
    }

    private void HookButtons()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include);
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(PlayClick);
        }
    }

    private void PlayBgmFrom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;

        if (bgmSource.isPlaying && System.Array.IndexOf(clips, bgmSource.clip) >= 0) return;

        int index = Random.Range(0, clips.Length);

        if (clips.Length > 1 && clips[index] == bgmSource.clip)
        {
            index = (index + 1) % clips.Length;
        }

        bgmSource.clip = clips[index];
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlaySe(AudioClip clip)
    {
        if (clip == null) return;
        seSource.PlayOneShot(clip);
    }

     // 勝ち・負けのジングルなど: BGMを止めて、1回だけ鳴らす
    public void PlayJingle(AudioClip clip)
    {
        if (clip == null) return;

        bgmSource.Stop();
        jingleSource.clip = clip;
        jingleSource.loop = false;
        jingleSource.Play();
    }

    // ボタンを押したときの自動のクリック音
    public void PlayClick()
    {
        if (SuppressAutoClick) return;
        PlaySe(clickSe);
    }

    // 自動のクリック音を止めている画面でも、鳴らしたいときに呼ぶ
    public void PlayMenuClick()
    {
        PlaySe(clickSe);
    }

    public void SetBgmVolume(float value)
    {
        BgmVolume = value;
        PlayerPrefs.SetFloat(BgmKey, value);
        ApplyVolume();
    }

    public void SetSeVolume(float value)
    {
        SeVolume = value;
        PlayerPrefs.SetFloat(SeKey, value);
        ApplyVolume();
    }

    private void ApplyVolume()
    {
        bgmSource.volume = BgmVolume;
        seSource.volume = SeVolume;
        jingleSource.volume = SeVolume;
    }
}