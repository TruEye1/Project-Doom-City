using UnityEngine;

public class EventosCombate : MonoBehaviour
{
    private PlayerController player;
    private SaludJugador saludJugador;

    private void Awake()
    {
        player = GetComponentInParent<PlayerController>();
        saludJugador = GetComponentInParent<SaludJugador>();
    }

    public void AbrirHitbox(int id)
    {
        if (player != null)
        {
            player.AbrirHitbox(id);
        }
    }

    public void CerrarHitboxes()
    {
        if (player != null)
        {
            player.CerrarHitboxes();
        }
    }

    public void FinAtaque()
    {
        if (player != null)
        {
            player.FinAtaque();
        }
    }

    public void FinStunJugador()
    {
        if (player != null)
        {
            player.FinStunJugador();
        }
    }

    public void FinalizarMuerteJugador()
    {
        if (saludJugador != null)
        {
            saludJugador.FinalizarMuerteJugador();
        }
    }

    public void MostrarGameOver()
    {
        if (saludJugador != null)
        {
            saludJugador.MostrarGameOver();
        }
    }
}
