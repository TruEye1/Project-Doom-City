using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HealthPickup : MonoBehaviour
{
    private static readonly int HashPickup = Animator.StringToHash("Pickup");

    [SerializeField] private int cantidadCuracion = 25;
    [SerializeField] private bool destruirAlRecoger = true;
    [SerializeField] private bool respawn = false;

    private Collider2D triggerCollider;
    private AudioSource audioSource;
    private Animator animator;
    private bool consumido;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (consumido)
        {
            return;
        }

        SaludJugador jugador = collision.GetComponentInParent<SaludJugador>();
        if (jugador == null || jugador.EstaMuerto)
        {
            return;
        }

        if (!jugador.Curar(cantidadCuracion))
        {
            return;
        }

        Consumir();
    }

    private void Consumir()
    {
        consumido = true;

        if (audioSource != null)
        {
            audioSource.Play();
        }

        if (animator != null)
        {
            animator.SetTrigger(HashPickup);
        }

        if (destruirAlRecoger && !respawn)
        {
            float destroyDelay = audioSource != null && audioSource.clip != null ? audioSource.clip.length : 0f;
            Destroy(gameObject, destroyDelay);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
