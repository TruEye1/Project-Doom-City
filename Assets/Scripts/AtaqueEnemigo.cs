using UnityEngine;

public class AtaqueEnemigo : MonoBehaviour
{
    [Tooltip("Cantidad de daño que hace este ataque")]
    public int danoAtaque = 15;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // EL RADAR: Esto imprimirá en consola el nombre y el Tag de CUALQUIER cosa que toque
        Debug.Log($"[RADAR] El puño tocó a: {collision.gameObject.name} | Su Tag es: {collision.tag}");

        if (collision.CompareTag("Player"))
        {
            SaludJugador jugador = collision.GetComponent<SaludJugador>();

            if (jugador != null)
            {
                jugador.RecibirDano(danoAtaque);
                // gameObject.SetActive(false); // Lo dejamos comentado por ahora para pruebas
            }
            else
            {
                Debug.LogWarning("⚠️ ERROR: Golpeé a Chris, pero no encuentro el script 'SaludJugador' en él.");
            }
        }
    }
}