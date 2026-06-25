using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CargadorAsincrono : MonoBehaviour
{
    public string escenaA_Cargar = "SampleScene";
    public Slider barraProgreso;
    public TextMeshProUGUI textoPorcentaje;

    void Start()
    {
        StartCoroutine(CargarEscenaAsincrona());
    }

    private IEnumerator CargarEscenaAsincrona()
    {
        AsyncOperation operacion = SceneManager.LoadSceneAsync(escenaA_Cargar);
        operacion.allowSceneActivation = false;

        float progresoVisual = 0f;

        while (progresoVisual < 1f)
        {
            float progresoDestino = Mathf.Clamp01(operacion.progress / 0.9f);

            progresoVisual = Mathf.MoveTowards(progresoVisual, progresoDestino, Time.deltaTime * 0.3f);

            barraProgreso.value = progresoVisual;
            textoPorcentaje.text = (progresoVisual * 100f).ToString("F0") + "%";

            if (Mathf.Approximately(progresoVisual, 1f) && operacion.progress >= 0.9f)
            {
                operacion.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}