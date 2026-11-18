using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HowToPlayPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text pageText;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private string[] titles;
    [SerializeField, TextArea(6, 16)] private string[] pages;

    private int pageIndex;

    private void Awake()
    {
        prevButton.onClick.AddListener(() => ShowPage(pageIndex - 1));
        nextButton.onClick.AddListener(() => ShowPage(pageIndex + 1));
        closeButton.onClick.AddListener(Close);
    }

    // 開くたびに、1ページ目から表示する
    private void OnEnable()
    {
        ShowPage(0);
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void ShowPage(int index)
    {
        if (pages == null || pages.Length == 0) return;

        pageIndex = Mathf.Clamp(index, 0, pages.Length - 1);
        titleText.text = (titles != null && pageIndex < titles.Length) ? titles[pageIndex] : "";
        bodyText.text = pages[pageIndex];
        pageText.text = (pageIndex + 1) + " / " + pages.Length;
        prevButton.interactable = pageIndex > 0;
        nextButton.interactable = pageIndex < pages.Length - 1;
    }
}