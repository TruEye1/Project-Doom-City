using System;
using System.Collections;
using UnityEngine;

public class EnemigoIA : MonoBehaviour
{
    private enum EnemyState
    {
        Idle,
        Chase,
        Attack,
        Hurt,
        Knockdown,
        Dead
    }

    [Header("Configuracion de Movimiento")]
    public float velocidadX = 2.5f;
    public float velocidadY = 1.5f;

    [Header("Distancias de Combate")]
    public float distanciaAtaqueX = 2.2f;
    public float tolerancia = 0.15f;
    [SerializeField] private float toleranciaVerticalAtaque = 0.35f;

    [Header("Alineacion Vertical")]
    public float offsetY = 1.5f;

    [Header("Sistema de Combate")]
    public int vidaMaxima = 100;
    public float tiempoEntreAtaques = 1.5f;
    [SerializeField] private float attackMaxLockTime = 1.2f;
    [SerializeField] private float hurtMaxLockTime = 0.8f;
    [SerializeField] private float knockdownMaxLockTime = 1.65f;
    [SerializeField] private float knockdownInvulnerabilityTime = 1.65f;
    [SerializeField] private float knockbackDistance = 1.15f;
    [SerializeField] private float knockbackDuration = 0.22f;
    [SerializeField] private float deathDisappearDelay = 0.85f;
    [SerializeField] private int blinkCount = 4;
    [SerializeField] private float blinkInterval = 0.12f;
    [SerializeField] private float retrasoContraataqueTrasDano = 0.35f;

    [Header("Hitbox de Ataque")]
    public GameObject hitboxAtaque;

    public int VidaActual => vidaActual;
    public int VidaMaxima => vidaMaxima;
    public float PorcentajeVida => vidaMaxima <= 0 ? 0f : (float)vidaActual / vidaMaxima;
    public bool EstaMuerto => state == EnemyState.Dead;
    public bool EstaNoqueado => state == EnemyState.Knockdown;
    public bool PuedeRecibirDano => state != EnemyState.Dead && state != EnemyState.Knockdown && !knockdownInvulnerable;

    public event Action<int, int> OnVidaCambiada;
    public event Action OnMuerto;

    private static readonly int HashIsWalking = Animator.StringToHash("isWalking");
    private static readonly int HashAttack1 = Animator.StringToHash("Attack1");
    private static readonly int HashAttack2 = Animator.StringToHash("Attack2");
    private static readonly int HashTakeDamage = Animator.StringToHash("TakeDamage");
    private static readonly int HashTipoDano = Animator.StringToHash("TipoDano");
    private static readonly int HashHitIndex = Animator.StringToHash("HitIndex");
    private static readonly int HashKnockout = Animator.StringToHash("Knockout");
    private static readonly int HashIsDead = Animator.StringToHash("isDead");
    private static readonly int StateIdle = Animator.StringToHash("Enemigo_Idle");
    private static readonly int StateHit1 = Animator.StringToHash("Pandillero_Hit_1");
    private static readonly int StateHit2 = Animator.StringToHash("Pandillero_Hit_2");
    private static readonly int StateKnockout = Animator.StringToHash("Pandillero_KO");
    private static readonly int StateMuerte = Animator.StringToHash("Pandillero_Muerte");

    private int vidaActual;
    private float proximoAtaqueTiempo;
    private float proximaBusquedaJugadorTiempo;
    private bool isCurrentlyWalking;
    private bool deathEventEmitted;
    private EnemyState state = EnemyState.Idle;

    private Transform jugador;
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D[] collidersPropios;
    private SpriteRenderer[] spriteRenderers;
    private Coroutine attackFallbackCoroutine;
    private Coroutine hurtFallbackCoroutine;
    private Coroutine knockdownFallbackCoroutine;
    private Coroutine knockdownInvulnerabilityCoroutine;
    private Coroutine knockbackCoroutine;
    private Coroutine deathFallbackCoroutine;
    private Coroutine deathBlinkCoroutine;
    private bool deathSequenceStarted;
    private bool knockdownInvulnerable;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        collidersPropios = GetComponents<Collider2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        vidaActual = Mathf.Max(1, vidaMaxima);
        CerrarHitboxAtaque();
    }

    private void Start()
    {
        BuscarJugador();
        EmitirVidaCambiada();
    }

    private void OnDisable()
    {
        CerrarHitboxAtaque();
        DetenerFallbacks();
    }

    private void FixedUpdate()
    {
        if (state == EnemyState.Dead || state == EnemyState.Hurt || state == EnemyState.Attack || state == EnemyState.Knockdown)
        {
            DetenerMovimiento();
            SetWalking(false);
            return;
        }

        if (jugador == null)
        {
            if (Time.time >= proximaBusquedaJugadorTiempo)
            {
                proximaBusquedaJugadorTiempo = Time.time + 0.5f;
                BuscarJugador();
            }

            CambiarEstado(EnemyState.Idle);
            DetenerMovimiento();
            SetWalking(false);
            return;
        }

        float targetY = jugador.position.y - offsetY;
        float diferenciaX = jugador.position.x - rb.position.x;
        float distanciaAbsX = Mathf.Abs(diferenciaX);
        float distanciaAbsY = Mathf.Abs(targetY - rb.position.y);
        bool enRangoX = distanciaAbsX <= distanciaAtaqueX + tolerancia;
        bool alineadoY = distanciaAbsY <= toleranciaVerticalAtaque;
        bool enRangoAtaque = enRangoX && alineadoY;

        ActualizarDireccion(diferenciaX);

        if (enRangoAtaque)
        {
            CambiarEstado(EnemyState.Idle);
            DetenerMovimiento();
            SetWalking(false);

            if (Time.time >= proximoAtaqueTiempo)
            {
                Atacar();
            }

            return;
        }

        CambiarEstado(EnemyState.Chase);
        SetWalking(true);

        float destinoX = enRangoX ? rb.position.x : jugador.position.x - Mathf.Sign(diferenciaX) * distanciaAtaqueX;
        Vector2 nuevaPos = new Vector2(
            Mathf.MoveTowards(rb.position.x, destinoX, velocidadX * Time.fixedDeltaTime),
            Mathf.MoveTowards(rb.position.y, targetY, velocidadY * Time.fixedDeltaTime)
        );
        rb.MovePosition(nuevaPos);
    }

    private void BuscarJugador()
    {
        GameObject jugadorObj = GameObject.Find("PlayerBase");
        if (jugadorObj != null)
        {
            jugador = jugadorObj.transform;
            return;
        }

        SaludJugador saludJugador = FindFirstObjectByType<SaludJugador>();
        jugador = saludJugador != null ? saludJugador.transform : null;
    }

    private void Atacar()
    {
        if (state == EnemyState.Dead || state == EnemyState.Hurt || state == EnemyState.Attack || state == EnemyState.Knockdown)
        {
            return;
        }

        if (Time.time < proximoAtaqueTiempo)
        {
            return;
        }

        CambiarEstado(EnemyState.Attack);
        CerrarHitboxAtaque();
        DetenerMovimiento();
        SetWalking(false);
        ResetearTriggersCombate();

        int triggerAtaque = UnityEngine.Random.Range(0, 2) == 0 ? HashAttack1 : HashAttack2;
        anim.SetTrigger(triggerAtaque);
        proximoAtaqueTiempo = Time.time + tiempoEntreAtaques;

        if (attackFallbackCoroutine != null)
        {
            StopCoroutine(attackFallbackCoroutine);
        }

        attackFallbackCoroutine = StartCoroutine(FallbackFinAtaque());
    }

    public bool RecibirDano(int dano, bool esGolpeFinal)
    {
        return RecibirDano(dano, esGolpeFinal, new Vector2(transform.position.x, transform.position.y));
    }

    public bool RecibirDano(int dano, bool esGolpeFinal, Vector2 origenGolpe)
    {
        if (state == EnemyState.Dead)
        {
            return false;
        }

        if (state == EnemyState.Knockdown || knockdownInvulnerable)
        {
            return false;
        }

        int danoAplicado = Mathf.Max(0, dano);
        if (danoAplicado <= 0)
        {
            return false;
        }

        vidaActual = Mathf.Clamp(vidaActual - danoAplicado, 0, vidaMaxima);
        EmitirVidaCambiada();

        CerrarHitboxAtaque();
        DetenerMovimiento();
        SetWalking(false);
        CancelarAtaqueActual();
        proximoAtaqueTiempo = Mathf.Max(proximoAtaqueTiempo, Time.time + retrasoContraataqueTrasDano);

        if (vidaActual <= 0)
        {
            Morir();
            return true;
        }

        if (esGolpeFinal)
        {
            IniciarKnockdown(origenGolpe);
            return true;
        }

        CambiarEstado(EnemyState.Hurt);
        ReproducirAnimacionDano();

        if (hurtFallbackCoroutine != null)
        {
            StopCoroutine(hurtFallbackCoroutine);
        }

        hurtFallbackCoroutine = StartCoroutine(FallbackFinGolpe());
        return true;
    }

    public void FinAtaque()
    {
        if (state != EnemyState.Attack)
        {
            return;
        }

        if (attackFallbackCoroutine != null)
        {
            StopCoroutine(attackFallbackCoroutine);
            attackFallbackCoroutine = null;
        }

        CerrarHitboxAtaque();
        DetenerMovimiento();
        SetWalking(false);
        state = EnemyState.Idle;
    }

    public void FinGolpe()
    {
        if (state != EnemyState.Hurt)
        {
            return;
        }

        if (hurtFallbackCoroutine != null)
        {
            StopCoroutine(hurtFallbackCoroutine);
            hurtFallbackCoroutine = null;
        }

        CerrarHitboxAtaque();
        DetenerMovimiento();
        SetWalking(false);
        proximoAtaqueTiempo = Mathf.Max(proximoAtaqueTiempo, Time.time + retrasoContraataqueTrasDano);
        state = EnemyState.Idle;
    }

    public void FinKnockdown()
    {
        if (state != EnemyState.Knockdown)
        {
            return;
        }

        if (knockdownFallbackCoroutine != null)
        {
            StopCoroutine(knockdownFallbackCoroutine);
            knockdownFallbackCoroutine = null;
        }

        CerrarHitboxAtaque();
        DetenerMovimiento();
        SetWalking(false);
        proximoAtaqueTiempo = Mathf.Max(proximoAtaqueTiempo, Time.time + retrasoContraataqueTrasDano);
        FinInvulnerabilidadKnockdown();
        FinKnockback();
        state = EnemyState.Idle;

        if (anim != null)
        {
            anim.Play(StateIdle, 0, 0f);
        }
    }

    public void Levantarse()
    {
        FinKnockdown();
    }

    public void FinalizarMuerte()
    {
        if (state != EnemyState.Dead || deathSequenceStarted)
        {
            return;
        }

        deathSequenceStarted = true;

        if (deathFallbackCoroutine != null)
        {
            StopCoroutine(deathFallbackCoroutine);
            deathFallbackCoroutine = null;
        }

        deathBlinkCoroutine = StartCoroutine(ParpadearYDesaparecer());
    }

    public void FinInvulnerabilidadKnockdown()
    {
        knockdownInvulnerable = false;

        if (knockdownInvulnerabilityCoroutine != null)
        {
            StopCoroutine(knockdownInvulnerabilityCoroutine);
            knockdownInvulnerabilityCoroutine = null;
        }
    }

    public void FinKnockback()
    {
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
            knockbackCoroutine = null;
        }

        DetenerMovimiento();
    }

    private void Morir()
    {
        if (state == EnemyState.Dead)
        {
            return;
        }

        state = EnemyState.Dead;
        vidaActual = 0;
        knockdownInvulnerable = false;
        EmitirVidaCambiada();
        DetenerFallbacks();
        CerrarHitboxAtaque();
        DetenerMovimiento();
        SetWalking(false);
        ResetearTriggersCombate();

        if (anim != null)
        {
            anim.SetBool(HashIsDead, true);
            anim.Play(StateMuerte, 0, 0f);
        }

        foreach (Collider2D col in collidersPropios)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }

        if (!deathEventEmitted)
        {
            deathEventEmitted = true;
            OnMuerto?.Invoke();
        }

        deathFallbackCoroutine = StartCoroutine(FallbackFinalizarMuerte());
    }

    public void ActivarHitbox()
    {
        if (state == EnemyState.Attack && hitboxAtaque != null)
        {
            hitboxAtaque.SetActive(true);
        }
    }

    public void DesactivarHitbox()
    {
        CerrarHitboxAtaque();
    }

    private void ReproducirAnimacionDano()
    {
        int tipoDano = UnityEngine.Random.Range(1, 3);
        anim.SetInteger(HashTipoDano, tipoDano);
        anim.SetInteger(HashHitIndex, tipoDano - 1);
        anim.ResetTrigger(HashTakeDamage);
        anim.Play(tipoDano == 1 ? StateHit1 : StateHit2, 0, 0f);
    }

    private void IniciarKnockdown(Vector2 origenGolpe)
    {
        if (state == EnemyState.Dead)
        {
            return;
        }

        state = EnemyState.Knockdown;
        knockdownInvulnerable = true;
        CerrarHitboxAtaque();
        DetenerMovimiento();
        SetWalking(false);
        ResetearTriggersCombate();
        IniciarInvulnerabilidadKnockdown();
        IniciarKnockback(origenGolpe);

        if (anim != null)
        {
            anim.SetBool(HashIsDead, false);
            anim.Play(StateKnockout, 0, 0f);
        }

        if (knockdownFallbackCoroutine != null)
        {
            StopCoroutine(knockdownFallbackCoroutine);
        }

        knockdownFallbackCoroutine = StartCoroutine(FallbackFinKnockdown());
    }

    private void IniciarInvulnerabilidadKnockdown()
    {
        if (knockdownInvulnerabilityCoroutine != null)
        {
            StopCoroutine(knockdownInvulnerabilityCoroutine);
        }

        knockdownInvulnerabilityCoroutine = StartCoroutine(FallbackFinInvulnerabilidadKnockdown());
    }

    private void IniciarKnockback(Vector2 origenGolpe)
    {
        if (rb == null || knockbackDistance <= 0f)
        {
            return;
        }

        float diferenciaX = rb.position.x - origenGolpe.x;
        float direccion = Mathf.Abs(diferenciaX) > 0.01f
            ? Mathf.Sign(diferenciaX)
            : (transform.localScale.x >= 0f ? -1f : 1f);

        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        knockbackCoroutine = StartCoroutine(AplicarKnockback(direccion));
    }

    private void CancelarAtaqueActual()
    {
        if (state == EnemyState.Attack && attackFallbackCoroutine != null)
        {
            StopCoroutine(attackFallbackCoroutine);
            attackFallbackCoroutine = null;
        }

        anim.ResetTrigger(HashAttack1);
        anim.ResetTrigger(HashAttack2);
    }

    private void ResetearTriggersCombate()
    {
        anim.ResetTrigger(HashAttack1);
        anim.ResetTrigger(HashAttack2);
        anim.ResetTrigger(HashTakeDamage);
        anim.ResetTrigger(HashKnockout);
    }

    private void CerrarHitboxAtaque()
    {
        if (hitboxAtaque != null)
        {
            hitboxAtaque.SetActive(false);
        }
    }

    private void DetenerMovimiento()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void SetWalking(bool value)
    {
        if (isCurrentlyWalking == value)
        {
            return;
        }

        isCurrentlyWalking = value;
        if (anim != null)
        {
            anim.SetBool(HashIsWalking, value);
        }
    }

    private void ActualizarDireccion(float diferenciaX)
    {
        if (diferenciaX > 0.1f)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (diferenciaX < -0.1f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }

    private void CambiarEstado(EnemyState nuevoEstado)
    {
        if (state == EnemyState.Dead)
        {
            return;
        }

        if (state == nuevoEstado)
        {
            return;
        }

        if (state == EnemyState.Hurt && nuevoEstado != EnemyState.Dead)
        {
            return;
        }

        if (state == EnemyState.Knockdown && nuevoEstado != EnemyState.Dead)
        {
            return;
        }

        if (state == EnemyState.Attack && nuevoEstado != EnemyState.Hurt && nuevoEstado != EnemyState.Dead)
        {
            return;
        }

        state = nuevoEstado;
    }

    private IEnumerator FallbackFinAtaque()
    {
        yield return new WaitForSeconds(attackMaxLockTime);

        if (state == EnemyState.Attack)
        {
            Debug.LogWarning($"{name}: falta Animation Event FinAtaque(). Se libera el ataque por fallback.");
            attackFallbackCoroutine = null;
            FinAtaque();
        }
    }

    private IEnumerator FallbackFinGolpe()
    {
        yield return new WaitForSeconds(hurtMaxLockTime);

        if (state == EnemyState.Hurt)
        {
            Debug.LogWarning($"{name}: falta Animation Event FinGolpe(). Se libera el dano por fallback.");
            hurtFallbackCoroutine = null;
            FinGolpe();
        }
    }

    private IEnumerator FallbackFinKnockdown()
    {
        yield return new WaitForSeconds(knockdownMaxLockTime);

        if (state == EnemyState.Knockdown)
        {
            Debug.LogWarning($"{name}: falta Animation Event FinKnockdown(). Se libera el derribo por fallback.");
            knockdownFallbackCoroutine = null;
            FinKnockdown();
        }
    }

    private IEnumerator FallbackFinInvulnerabilidadKnockdown()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, knockdownInvulnerabilityTime));
        knockdownInvulnerabilityCoroutine = null;
        FinInvulnerabilidadKnockdown();
    }

    private IEnumerator AplicarKnockback(float direccion)
    {
        float duracion = Mathf.Max(0.02f, knockbackDuration);
        Vector2 inicio = rb.position;
        Vector2 destino = new Vector2(inicio.x + direccion * knockbackDistance, inicio.y);
        float tiempo = 0f;

        while (tiempo < duracion && state == EnemyState.Knockdown)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.Clamp01(tiempo / duracion);
            float suavizado = 1f - (1f - t) * (1f - t);
            float x = inicio.x + (destino.x - inicio.x) * suavizado;
            rb.MovePosition(new Vector2(x, inicio.y));
            yield return null;
        }

        if (state == EnemyState.Knockdown)
        {
            rb.MovePosition(destino);
        }

        knockbackCoroutine = null;
    }

    private IEnumerator FallbackFinalizarMuerte()
    {
        yield return new WaitForSeconds(deathDisappearDelay);
        FinalizarMuerte();
    }

    private IEnumerator ParpadearYDesaparecer()
    {
        int safeBlinkCount = Mathf.Max(0, blinkCount);
        float safeInterval = Mathf.Max(0.02f, blinkInterval);

        for (int i = 0; i < safeBlinkCount; i++)
        {
            SetSpritesVisible(false);
            yield return new WaitForSeconds(safeInterval);
            SetSpritesVisible(true);
            yield return new WaitForSeconds(safeInterval);
        }

        gameObject.SetActive(false);
    }

    private void DetenerFallbacks()
    {
        if (attackFallbackCoroutine != null)
        {
            StopCoroutine(attackFallbackCoroutine);
            attackFallbackCoroutine = null;
        }

        if (hurtFallbackCoroutine != null)
        {
            StopCoroutine(hurtFallbackCoroutine);
            hurtFallbackCoroutine = null;
        }

        if (knockdownFallbackCoroutine != null)
        {
            StopCoroutine(knockdownFallbackCoroutine);
            knockdownFallbackCoroutine = null;
        }

        if (knockdownInvulnerabilityCoroutine != null)
        {
            StopCoroutine(knockdownInvulnerabilityCoroutine);
            knockdownInvulnerabilityCoroutine = null;
        }

        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
            knockbackCoroutine = null;
        }

        if (deathFallbackCoroutine != null)
        {
            StopCoroutine(deathFallbackCoroutine);
            deathFallbackCoroutine = null;
        }

        if (deathBlinkCoroutine != null)
        {
            StopCoroutine(deathBlinkCoroutine);
            deathBlinkCoroutine = null;
        }
    }

    private void SetSpritesVisible(bool visible)
    {
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = visible;
            }
        }
    }

    private void EmitirVidaCambiada()
    {
        OnVidaCambiada?.Invoke(vidaActual, vidaMaxima);
    }
}
