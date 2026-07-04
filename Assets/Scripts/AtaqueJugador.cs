using UnityEngine;

public class AtaqueJugador : MonoBehaviour
{
    public int danoAtaque = 10;
    public bool esGolpeFinal = false;
    private bool yaGolpeoEnEsteSwing = false; // Control de seguridad

    private void OnEnable()
    {
        yaGolpeoEnEsteSwing = false; // Reseteamos al activar el hitbox
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Evitar golpear múltiples veces
        if (yaGolpeoEnEsteSwing) return;

        // 2. Solo detectar el enemigo, ignorar sus propios hitboxes
        EnemigoIA enemigo = collision.GetComponent<EnemigoIA>();

        if (enemigo != null)
        {
            yaGolpeoEnEsteSwing = true; // Marcamos que ya golpeamos
            enemigo.RecibirDano(danoAtaque, esGolpeFinal);
            Debug.Log($"¡Golpe limpio a {collision.gameObject.name}!");
        }
    }
}