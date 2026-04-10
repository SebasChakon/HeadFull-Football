using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject creditsPanel;

    [Header("Créditos")]
    public RectTransform creditsText;
    public float scrollSpeed = 60f;

    private bool scrolling = false;
    private float startY;

    void Start()
    {
        creditsPanel.SetActive(false);
        startY = creditsText.anchoredPosition.y;
    }

    void Update()
    {
        if (!scrolling) return;

        creditsText.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        // Cuando termina de subir, reinicia
        if (creditsText.anchoredPosition.y > 1200f)
            creditsText.anchoredPosition = new Vector2(0, startY);
    }

    public void OnPlayButton()
    {
        SceneManager.LoadScene("CharacterSelect");
    }

    public void OnCreditsButton()
    {
        creditsPanel.SetActive(true);
        creditsText.anchoredPosition = new Vector2(0, startY);
        scrolling = true;
    }

    public void OnCloseCredits()
    {
        creditsPanel.SetActive(false);
        scrolling = false;
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }
}