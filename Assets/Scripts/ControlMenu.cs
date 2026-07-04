using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControlMenu : MonoBehaviour
{
    private const string VolumePrefsKey = "MasterVolume";

    public AudioSource audioSource;
    public GameObject panelConfiguracion;
    [SerializeField] private Slider sliderVolumen;

    private float currentVolume = 1f;

    private void Awake()
    {
        ResolverReferencias();
        currentVolume = PlayerPrefs.GetFloat(VolumePrefsKey, 1f);
        AplicarVolumen(currentVolume, false);
        RepararCallbacksBotones();

        if (panelConfiguracion != null)
        {
            panelConfiguracion.SetActive(false);
        }
    }

    public void Jugar()
    {
        GuardarVolumen();
        SceneManager.LoadScene("PantallaCarga");
    }

    public void Salir()
    {
        Application.Quit();
    }

    public void AbrirConfiguracion()
    {
        if (panelConfiguracion != null)
        {
            panelConfiguracion.SetActive(true);
        }
    }

    public void CerrarConfiguracion()
    {
        GuardarVolumen();

        if (panelConfiguracion != null)
        {
            panelConfiguracion.SetActive(false);
        }
    }

    public void SubirVolumen()
    {
        SetVolume(currentVolume + 0.1f);
    }

    public void BajarVolumen()
    {
        SetVolume(currentVolume - 0.1f);
    }

    public void Mutear()
    {
        SetVolume(currentVolume > 0.001f ? 0f : 1f);
    }

    public void SetVolume(float value)
    {
        AplicarVolumen(value, true);
    }

    private void ResolverReferencias()
    {
        if (audioSource == null)
        {
            audioSource = FindFirstObjectByType<AudioSource>();
        }

        if (sliderVolumen == null && panelConfiguracion != null)
        {
            sliderVolumen = panelConfiguracion.GetComponentInChildren<Slider>(true);
        }
    }

    private void RepararCallbacksBotones()
    {
        RepararBoton("Boton_Jugar", Jugar);
        RepararBoton("Boton_Configuracion", AbrirConfiguracion);
        RepararBoton("Boton_Salir", Salir);
        RepararBoton("Boton_Volver", CerrarConfiguracion);
        RepararBoton("Boton_Mas", SubirVolumen);
        RepararBoton("Boton_Menos", BajarVolumen);
        RepararBoton("Boton_Mute", Mutear);
    }

    private void RepararBoton(string objectName, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = GameObject.Find(objectName);
        if (buttonObject == null)
        {
            return;
        }

        Button button = buttonObject.GetComponent<Button>();
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private void AplicarVolumen(float value, bool persistir)
    {
        currentVolume = Mathf.Clamp01(value);
        AudioListener.volume = currentVolume;

        if (audioSource != null)
        {
            audioSource.volume = currentVolume;
            audioSource.mute = currentVolume <= 0.001f;
        }

        if (sliderVolumen != null && !Mathf.Approximately(sliderVolumen.value, currentVolume))
        {
            sliderVolumen.value = currentVolume;
        }

        if (persistir)
        {
            GuardarVolumen();
        }
    }

    private void GuardarVolumen()
    {
        PlayerPrefs.SetFloat(VolumePrefsKey, currentVolume);
        PlayerPrefs.Save();
    }
}
