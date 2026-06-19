using UnityEngine;

public class CamaraFinalFight : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform personaje;        // Arrastra a Chris aquí

    [Header("Configuración de Altura")]
    public float alturaFijaY = 0f;     // La altura fija en la que se quedará la cámara

    [Header("Límites Horizontales (Eje X)")]
    public float limiteIzquierdo;
    public float limiteDerecho;

    [Header("Suavizado")]
    [Range(0f, 1f)]
    public float suavizado = 0.125f;   // Elasticidad de la cámara

    void LateUpdate()
    {
        if (personaje == null) return;

        // 1. Calculamos la posición X ideal (seguir al personaje)
        float xObjetivo = personaje.position.x;

        // 2. Restringimos la X para que no se pase de los bordes de la calle
        xObjetivo = Mathf.Clamp(xObjetivo, limiteIzquierdo, limiteDerecho);

        // 3. Suavizamos el movimiento horizontal
        float xSuave = Mathf.Lerp(transform.position.x, xObjetivo, suavizado);

        // 4. Aplicamos la nueva posición (X suavizada, Y fija, Z original de la cámara)
        transform.position = new Vector3(xSuave, alturaFijaY, transform.position.z);
    }
}