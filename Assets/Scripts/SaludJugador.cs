using UnityEngine;

public class SaludJugador : MonoBehaviour
{
    public int vidaMaxima = 100;
    private int vidaActual;

    private bool estaMuerto = false;
    private Animator anim;

    void Start()
    {
        vidaActual = vidaMaxima;
        // Busca el Animator en el objeto actual o en sus hijos
        anim = GetComponentInChildren<Animator>();
    }

    public void RecibirDano(int cantidadDano)
    {
        if (estaMuerto) return;

        vidaActual -= cantidadDano;

        // --- LÓGICA DE ANIMACIÓN ALEATORIA ---
        if (anim != null)
        {
            // Random.Range(1, 3) devuelve un 1 o un 2
            int tipoDano = Random.Range(1, 3);

            if (tipoDano == 1)
            {
                anim.SetTrigger("TakeDamage1");
            }
            else
            {
                anim.SetTrigger("TakeDamage2");
            }
        }

        Debug.Log($"¡Chris recibió {cantidadDano} de daño! Vida restante: {vidaActual}");

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    private void Morir()
    {
        estaMuerto = true;

        // Si tienes animación de muerte para Chris, puedes activarla aquí
        // anim.SetBool("isDead", true);

        Debug.Log("¡Chris ha caído! Game Over.");
    }
}