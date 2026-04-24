using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("Interfaz de Pausa")]
    public GameObject menuPausaUI; // Aquí conectaremos el Canvas

    private bool isPaused = false;
    private PlayerControls controls;

    void Awake()
    {
        controls = new PlayerControls();

        // Suscribimos la acción de pausa al botón Escape
        controls.Player.Pause.performed += ctx => AlternarPausa();
    }

    void OnEnable() { controls.Enable(); }
    void OnDisable() { controls.Disable(); }

    // Función que decide si pausar o reanudar
    public void AlternarPausa()
    {
        if (isPaused) Reanudar();
        else Pausar();
    }

    public void Reanudar()
    {
        menuPausaUI.SetActive(false); // Oculta el menú
        Time.timeScale = 1f;          // El tiempo vuelve a la normalidad (100%)
        isPaused = false;
    }

    void Pausar()
    {
        menuPausaUI.SetActive(true);  // Muestra el menú
        Time.timeScale = 0f;          // Congela el tiempo y las físicas (0%)
        isPaused = true;
    }

    public void SalirDelJuego()
    {
        Debug.Log("Cerrando el juego...");
        Application.Quit(); // Nota: Esto solo funciona al compilar el juego (.exe), no en el editor.
    }
}