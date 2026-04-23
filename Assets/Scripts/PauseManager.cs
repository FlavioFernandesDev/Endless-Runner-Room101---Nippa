using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI; // Arrastas o Painel aqui
    private bool jogoPausado = false;

    void Update()
    {
        if (SceneManager.GetActiveScene().name != RunManager.GameplaySceneName || RunManager.Instance.IsGameOver)
        {
            return;
        }

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
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        RunManager.Instance.ResumeRun();
        jogoPausado = false;
    }

    void Pausar()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }

        RunManager.Instance.PauseRun();
        jogoPausado = true;
    }

    public void IrParaMenu()
    {
        jogoPausado = false;
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        RunManager.Instance.ExitToStageSelect();
    }

    public void SairDoJogo()
    {
        Debug.Log("O utilizador clicou em Sair!");
        Application.Quit();
    }
}
