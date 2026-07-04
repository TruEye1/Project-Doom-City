using System;
using System.Collections;
using UnityEngine;

public class SaludJugador : MonoBehaviour
{
    public int vidaMaxima = 100;
    [SerializeField] private float duracionInvulnerabilidad = 0.4f;
    [SerializeField] private float duracionStunAlRecibirDano = 0.5f;
    [SerializeField] private float retrasoMenuDerrota = 1.6f;

    public int VidaActual => vidaActual;
    public int VidaMaxima => vidaMaxima;
    public float PorcentajeVida => vidaMaxima <= 0 ? 0f : (float)vidaActual / vidaMaxima;
    public bool EstaMuerto { get; private set; }

    public event Action<int, int> OnVidaCambiada;
    public event Action OnMuerto;

    private static readonly int HashTakeDamage1 = Animator.StringToHash("TakeDamage1");
    private static readonly int HashTakeDamage2 = Animator.StringToHash("TakeDamage2");
    private static readonly int StateDeath = Animator.StringToHash("Chris_Death");

    private int vidaActual;
    private float invulnerableHasta;
    private Animator anim;
    private PlayerController playerController;
    private Coroutine gameOverCoroutine;
    private bool gameOverMostrado;

    private void Awake()
    {
        vidaActual = Mathf.Max(1, vidaMaxima);
        anim = GetComponentInChildren<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        EmitirVidaCambiada();
    }

    private void OnDisable()
    {
        if (gameOverCoroutine != null)
        {
            StopCoroutine(gameOverCoroutine);
            gameOverCoroutine = null;
        }
    }

    public bool RecibirDano(int cantidadDano)
    {
        if (EstaMuerto || Time.time < invulnerableHasta)
        {
            return false;
        }

        int danoAplicado = Mathf.Max(0, cantidadDano);
        if (danoAplicado <= 0)
        {
            return false;
        }

        invulnerableHasta = Time.time + duracionInvulnerabilidad;
        vidaActual = Mathf.Clamp(vidaActual - danoAplicado, 0, vidaMaxima);
        EmitirVidaCambiada();

        if (vidaActual <= 0)
        {
            Morir();
            return true;
        }

        ReproducirDano();

        playerController = playerController != null ? playerController : GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.EntrarStun(duracionStunAlRecibirDano);
        }

        return true;
    }

    public bool Curar(int cantidad)
    {
        if (EstaMuerto || cantidad <= 0 || vidaActual >= vidaMaxima)
        {
            return false;
        }

        int vidaAnterior = vidaActual;
        vidaActual = Mathf.Clamp(vidaActual + cantidad, 0, vidaMaxima);

        if (vidaActual == vidaAnterior)
        {
            return false;
        }

        EmitirVidaCambiada();
        return true;
    }

    private void ReproducirDano()
    {
        if (anim == null)
        {
            return;
        }

        anim.SetTrigger(UnityEngine.Random.Range(0, 2) == 0 ? HashTakeDamage1 : HashTakeDamage2);
    }

    private void Morir()
    {
        if (EstaMuerto)
        {
            return;
        }

        EstaMuerto = true;
        playerController = playerController != null ? playerController : GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.Morir();
        }
        else if (anim != null)
        {
            anim.ResetTrigger(HashTakeDamage1);
            anim.ResetTrigger(HashTakeDamage2);
            anim.Play(StateDeath, 0, 0f);
        }

        OnMuerto?.Invoke();

        if (gameOverCoroutine != null)
        {
            StopCoroutine(gameOverCoroutine);
        }

        gameOverCoroutine = StartCoroutine(MostrarGameOverTrasDelay());
    }

    public void FinalizarMuerteJugador()
    {
        if (!EstaMuerto)
        {
            return;
        }
    }

    public void MostrarGameOver()
    {
        if (!EstaMuerto || gameOverMostrado)
        {
            return;
        }

        gameOverMostrado = true;
        PauseManager pauseManager = FindFirstObjectByType<PauseManager>();
        GameOverMenuView.Show(pauseManager);
    }

    private IEnumerator MostrarGameOverTrasDelay()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, retrasoMenuDerrota));
        gameOverCoroutine = null;
        MostrarGameOver();
    }

    private void EmitirVidaCambiada()
    {
        OnVidaCambiada?.Invoke(vidaActual, vidaMaxima);
    }
}
