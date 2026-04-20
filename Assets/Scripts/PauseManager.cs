using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI; // Arrastas o Painel aqui
    private bool jogoPausado = false;

    void Update()
    {
        // Se carregar na tecla ESC ou P
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (jogoPausado)
            {
                Continuar();
            }
            else
            {
                Pausar();
            }
        }
    }

    public void Continuar()
    {
        pauseMenuUI.SetActive(false); // Esconde o menu
        Time.timeScale = 1f;          // O tempo volta ao normal
        jogoPausado = false;
    }

    void Pausar()
    {
        pauseMenuUI.SetActive(true);  // Mostra o menu
        Time.timeScale = 0f;          // O tempo PARA totalmente
        jogoPausado = true;
    }

    public void IrParaMenu()
    {
        Time.timeScale = 1f; // Importante: repor o tempo antes de mudar de cena
        SceneManager.LoadScene("StageSelect"); // Nome da tua cena de menu
    }

    public void SairDoJogo()
    {
    Debug.Log("O utilizador clicou em Sair!");
    Application.Quit(); // Este comando fecha o ficheiro .exe ou .app final
    }
}
