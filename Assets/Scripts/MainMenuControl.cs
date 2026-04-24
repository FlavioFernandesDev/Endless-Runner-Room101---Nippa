using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuControl : MonoBehaviour
{
    [Header("Painéis do Menu")]
    public GameObject painelBotoesPrincipais; // O que tem Start e Sair
    public GameObject mensagemCliqueInicial;   // O que diz "Clique para Jogar"

    // static para o valor sobreviver entre cenas
    public static bool saltarIntro = false;

    private const float IntroDurationSeconds = 1.5f;

    private TrocaCamara trocaCamara;
    private Coroutine introRoutine;
    private bool introAtiva;

    void Awake()
    {
        trocaCamara = GetComponent<TrocaCamara>();
    }

    void Start()
    {
        ConfigurarHudContadores();

        if (painelBotoesPrincipais == null && mensagemCliqueInicial == null)
        {
            return;
        }

        // Se viermos do Game Over (saltarIntro = true), mostra logo os botões
        if (saltarIntro)
        {
            AtivarMenuDireto();
        }
        else
        {
            IniciarIntro();
        }
    }

    void Update()
    {
        // Se a mensagem inicial estiver ativa e o jogador clicar no ecrã...
        if (introAtiva && mensagemCliqueInicial != null && mensagemCliqueInicial.activeSelf)
        {
            if (Input.GetMouseButtonDown(0)) // Clique do botão esquerdo do rato
            {
                AtivarMenuDireto();
            }
        }
    }

    private void IniciarIntro()
    {
        introAtiva = true;
        if (mensagemCliqueInicial != null) mensagemCliqueInicial.SetActive(true);
        if (painelBotoesPrincipais != null) painelBotoesPrincipais.SetActive(false);
        if (trocaCamara != null) trocaCamara.PlayIntro();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (introRoutine != null)
        {
            StopCoroutine(introRoutine);
        }

        introRoutine = StartCoroutine(MostrarMenuDepoisDaIntro());
    }

    private IEnumerator MostrarMenuDepoisDaIntro()
    {
        yield return new WaitForSeconds(IntroDurationSeconds);
        introRoutine = null;
        AtivarMenuDireto();
    }

    public void AtivarMenuDireto()
    {
        if (introRoutine != null)
        {
            StopCoroutine(introRoutine);
            introRoutine = null;
        }

        introAtiva = false;
        if (mensagemCliqueInicial != null) mensagemCliqueInicial.SetActive(false);
        if (painelBotoesPrincipais != null) painelBotoesPrincipais.SetActive(true);
        if (trocaCamara != null) trocaCamara.ShowStatic();
        
        // Garante que o rato aparece para clicar nos botões
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void ConfigurarHudContadores()
    {
        Canvas canvas = EncontrarCanvasDaCena();
        if (canvas == null)
        {
            return;
        }

        ConfigurarLinhaContador(canvas, "CoinBack", "CoinSide", "CoinIcon", "CoinCount", -24f);
        ConfigurarLinhaContador(canvas, "KeyBack", "KeySide", "KeyIcon", "KeyCount", -88f);
        ConfigurarLinhaContador(canvas, "RunBack", "RunSide", "RunIcon", "RunCount", -152f);
    }

    private Canvas EncontrarCanvasDaCena()
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
        foreach (Canvas canvas in canvases)
        {
            if (canvas != null && canvas.gameObject.scene == gameObject.scene)
            {
                return canvas;
            }
        }

        return null;
    }

    private void ConfigurarLinhaContador(Canvas canvas, string backName, string sideName, string iconName, string countName, float y)
    {
        RectTransform back = EncontrarRect(canvas, backName);
        RectTransform side = EncontrarRect(canvas, sideName);
        RectTransform icon = EncontrarRect(canvas, iconName);
        RectTransform count = EncontrarRect(canvas, countName);

        ConfigurarRectNoCanvas(back, canvas.transform, new Vector2(24f, y), new Vector2(260f, 56f), new Vector2(0f, 1f));
        ConfigurarRectNoCanvas(side, canvas.transform, new Vector2(24f, y), new Vector2(6f, 56f), new Vector2(0f, 1f));
        ConfigurarRectNoCanvas(icon, canvas.transform, new Vector2(38f, y - 8f), new Vector2(40f, 40f), new Vector2(0f, 1f));
        ConfigurarRectNoCanvas(count, canvas.transform, new Vector2(92f, y), new Vector2(180f, 56f), new Vector2(0f, 1f));

        ConfigurarTextoContador(count);

        if (back != null) back.SetAsLastSibling();
        if (side != null) side.SetAsLastSibling();
        if (icon != null) icon.SetAsLastSibling();
        if (count != null) count.SetAsLastSibling();
    }

    private static RectTransform EncontrarRect(Canvas canvas, string objectName)
    {
        RectTransform[] rects = canvas.GetComponentsInChildren<RectTransform>(true);
        foreach (RectTransform rect in rects)
        {
            if (rect != null && rect.gameObject.name == objectName)
            {
                return rect;
            }
        }

        return null;
    }

    private static void ConfigurarRectNoCanvas(RectTransform rect, Transform parent, Vector2 anchoredPosition, Vector2 size, Vector2 pivot)
    {
        if (rect == null)
        {
            return;
        }

        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static void ConfigurarTextoContador(RectTransform count)
    {
        if (count == null)
        {
            return;
        }

        TMP_Text text = count.GetComponent<TMP_Text>();
        if (text == null)
        {
            return;
        }

        text.margin = Vector4.zero;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.enableAutoSizing = true;
        text.fontSizeMin = 24f;
        text.fontSizeMax = 44f;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        text.color = Color.black;
    }

    // --- FUNÇÃO PARA O BOTÃO QUIT (Aquele que está no ecrã de Game Over) ---
    public void VoltarParaStartGame()
    {
        Time.timeScale = 1f;
        saltarIntro = true; // Ativa o gatilho
        SceneManager.LoadScene("MainMenu"); // Garante que o nome da cena está correto aqui!
    }

    public void StartGame()
    {
        
        saltarIntro = false; // Reset para quando começar a jogar
        SceneManager.LoadScene("StageSelect"); 
    }

    public void OpenAchievements()
    {
        saltarIntro = true;
        SceneManager.LoadScene(RunManager.AchievementsSceneName);
    }

    public void SairDoJogoTodo()
    {
        Debug.Log("O Jogo fechou!");
        Application.Quit();
    }
}
