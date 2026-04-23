using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuControl : MonoBehaviour
{
    [Header("Painéis do Menu")]
    public GameObject painelBotoesPrincipais; // O que tem Start e Sair
    public GameObject mensagemCliqueInicial;   // O que diz "Clique para Jogar"

    // static para o valor sobreviver entre cenas
    public static bool saltarIntro = false;

    void Start()
    {
        // Se viermos do Game Over (saltarIntro = true), mostra logo os botões
        if (saltarIntro)
        {
            AtivarMenuDireto();
        }
        else
        {
            // Se abrirmos o jogo normal, mostra a mensagem e esconde os botões
            if (mensagemCliqueInicial != null) mensagemCliqueInicial.SetActive(true);
            if (painelBotoesPrincipais != null) painelBotoesPrincipais.SetActive(false);
        }
    }

    void Update()
    {
        // Se a mensagem inicial estiver ativa e o jogador clicar no ecrã...
        if (mensagemCliqueInicial != null && mensagemCliqueInicial.activeSelf)
        {
            if (Input.GetMouseButtonDown(0)) // Clique do botão esquerdo do rato
            {
                AtivarMenuDireto();
            }
        }
    }

    public void AtivarMenuDireto()
    {
        if (mensagemCliqueInicial != null) mensagemCliqueInicial.SetActive(false);
        if (painelBotoesPrincipais != null) painelBotoesPrincipais.SetActive(true);
        
        // Garante que o rato aparece para clicar nos botões
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
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
