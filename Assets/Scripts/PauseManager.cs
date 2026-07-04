using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Interfaz de Pausa")]
    public GameObject menuPausaUI;

    public bool EstaPausado => isPaused;

    private bool isPaused = false;
    private PlayerControls controls;

    private void Awake()
    {
        GameOverMenuView.ResetRuntimeState();
        AlphaCompletionView.ResetRuntimeState();
        controls = new PlayerControls();
        controls.Player.Pause.performed += _ => TogglePausa();

        if (menuPausaUI == null)
        {
            menuPausaUI = RetroPauseMenuView.CreatePauseCanvas().gameObject;
        }

        RetroPauseMenuView view = menuPausaUI.GetComponent<RetroPauseMenuView>();
        if (view == null)
        {
            view = menuPausaUI.AddComponent<RetroPauseMenuView>();
        }

        view.Build(this);
        menuPausaUI.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnEnable()
    {
        controls?.Enable();
    }

    private void OnDisable()
    {
        controls?.Disable();
        Time.timeScale = 1f;
    }

    public void AlternarPausa()
    {
        TogglePausa();
    }

    public void TogglePausa()
    {
        if (GameOverMenuView.GameOverActivo || AlphaCompletionView.Activo)
        {
            return;
        }

        if (isPaused)
        {
            Reanudar();
        }
        else
        {
            Pausar();
        }
    }

    public void Pausar()
    {
        if (GameOverMenuView.GameOverActivo || AlphaCompletionView.Activo)
        {
            return;
        }

        if (menuPausaUI != null)
        {
            menuPausaUI.SetActive(true);
        }

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Reanudar()
    {
        if (menuPausaUI != null)
        {
            menuPausaUI.SetActive(false);
        }

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void ReiniciarNivel()
    {
        GameOverMenuView.ResetRuntimeState();
        AlphaCompletionView.ResetRuntimeState();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void VolverAlMenuPrincipal()
    {
        GameOverMenuView.ResetRuntimeState();
        AlphaCompletionView.ResetRuntimeState();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuInicio");
    }

    public void CerrarPausaPorGameOver()
    {
        if (menuPausaUI != null)
        {
            menuPausaUI.SetActive(false);
        }

        isPaused = false;
    }

    public void SalirDelJuego()
    {
        Debug.Log("Cerrando el juego...");
        Application.Quit();
    }
}
