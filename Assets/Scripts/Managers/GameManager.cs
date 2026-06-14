using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private string gameOverTitle = "GAME OVER";
    [SerializeField] private string gameOverMessage = "Ratusz zosta≥ zniszczony.";
    [SerializeField] private bool pauseOnGameOver = true;

    private Canvas canvas;
    private GameObject gameOverPanel;
    private bool isGameOver = false;

    public bool IsGameOver => isGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;

        EnsureEventSystem();
        CreateGameOverUi();
        HideGameOverUi();
    }

    public void GameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        ShowGameOverUi();

        if (pauseOnGameOver)
            Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    private void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
            return;

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private void CreateGameOverUi()
    {
        Canvas existingCanvas = FindFirstObjectByType<Canvas>();

        if (existingCanvas != null)
        {
            canvas = existingCanvas;
        }
        else
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvas.transform, false);

        Image background = gameOverPanel.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.82f);

        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI titleText = CreateText(
            "Title",
            gameOverPanel.transform,
            gameOverTitle,
            new Vector2(0f, 90f),
            new Vector2(700f, 80f),
            54
        );

        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;

        TextMeshProUGUI messageText = CreateText(
            "Message",
            gameOverPanel.transform,
            gameOverMessage,
            new Vector2(0f, 25f),
            new Vector2(700f, 50f),
            24
        );

        messageText.alignment = TextAlignmentOptions.Center;

        Button restartButton = CreateButton(
            "RestartButton",
            gameOverPanel.transform,
            "Restart",
            new Vector2(0f, -55f),
            new Vector2(220f, 48f)
        );

        restartButton.onClick.AddListener(RestartGame);

        Button quitButton = CreateButton(
            "QuitButton",
            gameOverPanel.transform,
            "Wyjdü",
            new Vector2(0f, -115f),
            new Vector2(220f, 42f)
        );

        quitButton.onClick.AddListener(QuitGame);
    }

    private TextMeshProUGUI CreateText(string name, Transform parent, string textValue, Vector2 position, Vector2 size, int fontSize)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = textValue;
        text.fontSize = fontSize;
        text.color = Color.white;

        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        return text;
    }

    private Button CreateButton(string name, Transform parent, string label, Vector2 position, Vector2 size)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.18f, 0.18f, 0.18f, 1f);

        Button button = buttonObject.AddComponent<Button>();

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI buttonText = CreateText("Text", buttonObject.transform, label, Vector2.zero, size, 20);
        buttonText.alignment = TextAlignmentOptions.Center;

        return button;
    }

    private void ShowGameOverUi()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    private void HideGameOverUi()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
}