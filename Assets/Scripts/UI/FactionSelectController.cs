using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FactionSelectController : MonoBehaviour
{
    [SerializeField] private DeckData heroDeck;
    [SerializeField] private DeckData demonDeck;
    [SerializeField] private Button heroButton;
    [SerializeField] private Button demonButton;
    [SerializeField] private Button backButton;

    private void Start()
    {
        heroButton.onClick.AddListener(() => StartBattle(heroDeck, demonDeck));
        demonButton.onClick.AddListener(() => StartBattle(demonDeck, heroDeck));
        backButton.onClick.AddListener(() => SceneManager.LoadScene(SceneNames.Home));
    }

    // 自分が使うデッキと、相手(CPU)が使うデッキを決めて、対戦を始める
    private void StartBattle(DeckData playerDeck, DeckData enemyDeck)
    {
        GameSession.PlayerDeck = playerDeck;
        GameSession.EnemyDeck = enemyDeck;
        SceneManager.LoadScene(SceneNames.Battle);
    }
}