using System.Collections.Generic;
using UnityEngine;

public class AtaqueEnemigo : MonoBehaviour
{
    [Tooltip("Cantidad de dano que hace este ataque")]
    public int danoAtaque = 15;
    [SerializeField] private AudioClip hitConfirmClip = null;
    [SerializeField] private bool debugCombate = false;

    private readonly HashSet<SaludJugador> jugadoresGolpeados = new HashSet<SaludJugador>();

    private void OnEnable()
    {
        jugadoresGolpeados.Clear();
    }

    private void OnDisable()
    {
        jugadoresGolpeados.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        SaludJugador jugador = collision.GetComponentInParent<SaludJugador>();
        if (jugador == null || jugador.EstaMuerto)
        {
            return;
        }

        if (!jugadoresGolpeados.Add(jugador))
        {
            return;
        }

        bool golpeAplicado = jugador.RecibirDano(danoAtaque);
        if (!golpeAplicado)
        {
            return;
        }

        CombatAudioPlayer.PlayEnemyHit(jugador.transform.position, hitConfirmClip);

        if (debugCombate)
        {
            Debug.Log($"Golpe enemigo -> {jugador.name} por {danoAtaque}");
        }
    }
}
