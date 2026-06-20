using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Interfaz de Pausa")]
    public GameObject menuPausaUI;

    private bool isPaused = false;
    private PlayerControls controls;

    void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Pause.performed += ctx => AlternarPausa();
    }

    void OnEnable() { controls.Enable(); }
    void OnDisable() { controls.Disable(); }

    public void AlternarPausa()
    {
        if (isPaused) Reanudar();
        else Pausar();
    }

    public void Reanudar()
    {
        menuPausaUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Pausar()
    {
        menuPausaUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void VolverAlMenuPrincipal()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuInicio");
    }

    public void SalirDelJuego()
    {
        Debug.Log("Cerrando el juego...");
        Application.Quit();
    }
}