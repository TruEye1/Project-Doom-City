using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlMenu : MonoBehaviour
{
    public AudioSource audioSource;
    public GameObject panelConfiguracion;

    public void Jugar()
    {
        SceneManager.LoadScene("PantallaCarga");
    }

    public void Salir()
    {
        Debug.Log("Saliendo de Doom City...");
        Application.Quit();
    }

    public void AbrirConfiguracion()
    {
        panelConfiguracion.SetActive(true);
    }

    public void CerrarConfiguracion()
    {
        panelConfiguracion.SetActive(false);
    }

    public void SubirVolumen()
    {
        audioSource.volume = Mathf.Clamp01(audioSource.volume + 0.1f);
    }

    public void BajarVolumen()
    {
        audioSource.volume = Mathf.Clamp01(audioSource.volume - 0.1f);
    }

    public void Mutear()
    {
        audioSource.mute = !audioSource.mute;
    }
}