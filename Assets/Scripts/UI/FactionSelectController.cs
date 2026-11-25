using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FactionSelectController : MonoBehaviour
{
    private const float HoverScale = 1.06f; // カーソルを合わせたときの拡大

    [SerializeField] private DeckData heroDeck;
    [SerializeField] private DeckData demonDeck;
    [SerializeField] private Button heroButton;
    [SerializeField] private Button demonButton;
    [SerializeField] private Button heroPanelButton;   // 勇者の絵の枠(クリックで選べる)
    [SerializeField] private Button demonPanelButton;  // 魔王の絵の枠
    [SerializeField] private Button backButton;

    private void Start()
    {
        heroButton.onClick.AddListener(() => StartBattle(heroDeck, demonDeck));
        demonButton.onClick.AddListener(() => StartBattle(demonDeck, heroDeck));
        heroPanelButton.onClick.AddListener(() => StartBattle(heroDeck, demonDeck));
        demonPanelButton.onClick.AddListener(() => StartBattle(demonDeck, heroDeck));
        backButton.onClick.AddListener(() => SceneManager.LoadScene(SceneNames.Home));

        // 枠とその下の名前のボタンを、いっしょに大きくする
        AddHoverScale(heroPanelButton.gameObject, heroPanelButton.transform, heroButton.transform);
        AddHoverScale(demonPanelButton.gameObject, demonPanelButton.transform, demonButton.transform);
        AddHoverScale(heroButton.gameObject, heroPanelButton.transform, heroButton.transform);
        AddHoverScale(demonButton.gameObject, demonPanelButton.transform, demonButton.transform);
    }

    // 自分が使うデッキと、相手(CPU)が使うデッキを決めて、対戦を始める
    private void StartBattle(DeckData playerDeck, DeckData enemyDeck)
    {
        GameSession.PlayerDeck = playerDeck;
        GameSession.EnemyDeck = enemyDeck;
        SceneManager.LoadScene(SceneNames.Battle);
    }

    // 対象にカーソルを合わせたとき、枠と名前のボタンをいっしょに大きくする
    private void AddHoverScale(GameObject target, Transform panel, Transform nameButton)
    {
        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null) trigger = target.AddComponent<EventTrigger>();

        AddEntry(trigger, EventTriggerType.PointerEnter, () => SetScale(panel, nameButton, HoverScale));
        AddEntry(trigger, EventTriggerType.PointerExit, () => SetScale(panel, nameButton, 1f));
    }

    private void AddEntry(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(_ => action());
        trigger.triggers.Add(entry);
    }

    private void SetScale(Transform panel, Transform nameButton, float scale)
    {
        panel.localScale = Vector3.one * scale;
        nameButton.localScale = Vector3.one * scale;
    }
}