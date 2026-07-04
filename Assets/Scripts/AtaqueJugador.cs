using System.Collections.Generic;
using UnityEngine;

public class AtaqueJugador : MonoBehaviour
{
    public int danoAtaque = 10;
    public bool esGolpeFinal = false;
    [SerializeField] private AudioClip hitConfirmClip = null;
    [SerializeField] private bool debugCombate = false;

    private readonly HashSet<EnemigoIA> enemigosGolpeados = new HashSet<EnemigoIA>();

    private void OnEnable()
    {
        enemigosGolpeados.Clear();
    }

    private void OnDisable()
    {
        enemigosGolpeados.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemigoIA enemigo = collision.GetComponentInParent<EnemigoIA>();
        if (enemigo == null || !enemigo.PuedeRecibirDano)
        {
            return;
        }

        if (!enemigosGolpeados.Add(enemigo))
        {
            return;
        }

        bool golpeAplicado = enemigo.RecibirDano(danoAtaque, esGolpeFinal, new Vector2(transform.position.x, transform.position.y));
        if (!golpeAplicado)
        {
            return;
        }

        CombatAudioPlayer.PlayPlayerHit(enemigo.transform.position, hitConfirmClip);

        if (debugCombate)
        {
            Debug.Log($"Golpe jugador -> {enemigo.name} por {danoAtaque}");
        }
    }
}
