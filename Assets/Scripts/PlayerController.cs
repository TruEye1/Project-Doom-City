using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private enum PlayerState
    {
        Normal,
        Attacking,
        Stunned,
        Dead
    }

    [Header("Configuracion de Movimiento")]
    public float speed = 5f;
    public float runSpeed = 10f;

    [Header("Salto 2.5D")]
    public Transform spriteTransform;
    public float jumpPower = 7f;
    public float gravity = 15f;
    private float currentJumpVelocity;
    private bool isJumping = false;
    private bool isRunning = false;

    [Header("Sistema de Combos")]
    public int comboStep = 0;
    public float comboWindow = 0.8f;
    [SerializeField] private float attackFallbackSeconds = 0.9f;
    [SerializeField] private float stunFallbackSeconds = 0.5f;
    [SerializeField] private float deathGroundOffsetY = -0.75f;
    [SerializeField] private float deathSettleSeconds = 0.55f;
    private float lastClickTime;
    private bool isAttacking = false;

    [Header("Referencias de Hitboxes")]
    public GameObject hitboxAtk1;
    public GameObject hitboxAtk2;
    public GameObject hitboxAtk3;
    public GameObject hitboxAtk4;

    private static readonly int HashComboStep = Animator.StringToHash("ComboStep");
    private static readonly int HashAttack = Animator.StringToHash("Attack");
    private static readonly int HashSpeed = Animator.StringToHash("Speed");
    private static readonly int HashIsRunning = Animator.StringToHash("IsRunning");
    private static readonly int HashIsJumping = Animator.StringToHash("IsJumping");
    private static readonly int HashTakeDamage1 = Animator.StringToHash("TakeDamage1");
    private static readonly int HashTakeDamage2 = Animator.StringToHash("TakeDamage2");
    private static readonly int StateDeath = Animator.StringToHash("Chris_Death");

    private Rigidbody2D rb;
    private Animator anim;
    private PlayerControls controls;
    private Vector2 moveInput;
    private Coroutine attackFallbackCoroutine;
    private Coroutine stunFallbackCoroutine;
    private Coroutine deathSettleCoroutine;
    private Vector3 baseSpriteLocalPosition;
    private float stunLockedUntil;
    private PlayerState state = PlayerState.Normal;

    public bool EstaMuerto => state == PlayerState.Dead;
    public bool EstaStuneado => state == PlayerState.Stunned;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Jump.performed += _ => RealizarSalto();
        controls.Player.Run.performed += _ => isRunning = true;
        controls.Player.Run.canceled += _ => isRunning = false;
        controls.Player.Attack.performed += _ => IntentarAtacar();
    }

    private void OnEnable()
    {
        if (state != PlayerState.Dead)
        {
            controls?.Enable();
        }
    }

    private void OnDisable()
    {
        CerrarHitboxes();
        DetenerFallbackAtaque();
        DetenerFallbackStun();
        DetenerAjusteMuerte();
        controls?.Disable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        if (spriteTransform == null && anim != null)
        {
            spriteTransform = anim.transform;
        }

        if (spriteTransform != null)
        {
            baseSpriteLocalPosition = spriteTransform.localPosition;
        }

        CerrarHitboxes();
        ConfigurarGolpeFinal();
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            return;
        }

        if (state == PlayerState.Dead)
        {
            moveInput = Vector2.zero;
            return;
        }

        if (Time.time > lastClickTime + comboWindow && !isAttacking)
        {
            comboStep = 0;
            if (anim != null)
            {
                anim.SetInteger(HashComboStep, 0);
            }
        }

        if (isAttacking || isJumping || state == PlayerState.Stunned)
        {
            moveInput = Vector2.zero;
            if (anim != null)
            {
                anim.SetFloat(HashSpeed, 0f);
                anim.SetBool(HashIsRunning, false);
            }
        }
        else
        {
            moveInput = controls.Player.Move.ReadValue<Vector2>();
        }

        if (spriteTransform != null)
        {
            if (moveInput.x > 0f)
            {
                spriteTransform.localScale = new Vector3(1f, 1f, 1f);
            }
            else if (moveInput.x < 0f)
            {
                spriteTransform.localScale = new Vector3(-1f, 1f, 1f);
            }
        }

        ActualizarAnimacionMovimiento();
        ActualizarSalto();
    }

    private void FixedUpdate()
    {
        if (isAttacking || state == PlayerState.Stunned || state == PlayerState.Dead || rb == null)
        {
            return;
        }

        float currentSpeed = isRunning ? runSpeed : speed;
        Vector2 targetPosition = rb.position + moveInput.normalized * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(StageBounds2D.ClampRigidbodyTarget(rb, targetPosition));
    }

    private void RealizarSalto()
    {
        if (Time.timeScale == 0f || isJumping || isAttacking || state != PlayerState.Normal)
        {
            return;
        }

        isJumping = true;
        currentJumpVelocity = jumpPower;
    }

    private void IntentarAtacar()
    {
        if (Time.timeScale == 0f || isJumping || isAttacking || state != PlayerState.Normal)
        {
            return;
        }

        IniciarAtaque();
    }

    private void IniciarAtaque()
    {
        isAttacking = true;
        state = PlayerState.Attacking;
        CerrarHitboxes();

        comboStep++;
        if (comboStep > 4)
        {
            comboStep = 1;
        }

        lastClickTime = Time.time;

        if (anim != null)
        {
            anim.SetInteger(HashComboStep, comboStep);
            anim.SetTrigger(HashAttack);
        }

        DetenerFallbackAtaque();
        attackFallbackCoroutine = StartCoroutine(FallbackFinAtaque());
    }

    public void FinAtaque()
    {
        if (!isAttacking)
        {
            return;
        }

        CerrarHitboxes();
        isAttacking = false;
        DetenerFallbackAtaque();

        if (state == PlayerState.Attacking)
        {
            state = PlayerState.Normal;
        }
    }

    public void AbrirHitbox(int id)
    {
        if (!isAttacking || state == PlayerState.Stunned || state == PlayerState.Dead)
        {
            return;
        }

        CerrarHitboxes();

        if (id == 1 && hitboxAtk1 != null)
        {
            ConfigurarHitboxGolpeFinal(hitboxAtk1, false);
            hitboxAtk1.SetActive(true);
        }
        else if (id == 2 && hitboxAtk2 != null)
        {
            ConfigurarHitboxGolpeFinal(hitboxAtk2, false);
            hitboxAtk2.SetActive(true);
        }
        else if (id == 3 && hitboxAtk3 != null)
        {
            ConfigurarHitboxGolpeFinal(hitboxAtk3, false);
            hitboxAtk3.SetActive(true);
        }
        else if (id == 4 && hitboxAtk4 != null)
        {
            ConfigurarHitboxGolpeFinal(hitboxAtk4, true);
            hitboxAtk4.SetActive(true);
        }
    }

    public void CerrarHitboxes()
    {
        if (hitboxAtk1 != null)
        {
            ConfigurarHitboxGolpeFinal(hitboxAtk1, false);
            hitboxAtk1.SetActive(false);
        }

        if (hitboxAtk2 != null)
        {
            ConfigurarHitboxGolpeFinal(hitboxAtk2, false);
            hitboxAtk2.SetActive(false);
        }

        if (hitboxAtk3 != null)
        {
            ConfigurarHitboxGolpeFinal(hitboxAtk3, false);
            hitboxAtk3.SetActive(false);
        }

        if (hitboxAtk4 != null)
        {
            ConfigurarHitboxGolpeFinal(hitboxAtk4, false);
            hitboxAtk4.SetActive(false);
        }
    }

    public void EntrarStun(float duracion)
    {
        if (state == PlayerState.Dead)
        {
            return;
        }

        CerrarHitboxes();
        DetenerFallbackAtaque();
        isAttacking = false;
        isJumping = false;
        currentJumpVelocity = 0f;
        comboStep = 0;
        moveInput = Vector2.zero;
        state = PlayerState.Stunned;
        stunLockedUntil = Time.time + Mathf.Max(0.01f, duracion);

        if (spriteTransform != null)
        {
            spriteTransform.localPosition = baseSpriteLocalPosition;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (anim != null)
        {
            anim.SetInteger(HashComboStep, 0);
            anim.SetFloat(HashSpeed, 0f);
            anim.SetBool(HashIsRunning, false);
            anim.SetBool(HashIsJumping, false);
            anim.ResetTrigger(HashAttack);
        }

        DetenerFallbackStun();
        stunFallbackCoroutine = StartCoroutine(FallbackFinStun(Mathf.Max(duracion, stunFallbackSeconds)));
    }

    public void FinStunJugador()
    {
        if (state != PlayerState.Stunned || Time.time < stunLockedUntil)
        {
            return;
        }

        DetenerFallbackStun();
        state = PlayerState.Normal;
    }

    public void Morir()
    {
        if (state == PlayerState.Dead)
        {
            return;
        }

        state = PlayerState.Dead;
        isAttacking = false;
        isJumping = false;
        isRunning = false;
        currentJumpVelocity = 0f;
        comboStep = 0;
        moveInput = Vector2.zero;
        CerrarHitboxes();
        DetenerFallbackAtaque();
        DetenerFallbackStun();
        controls?.Disable();

        if (spriteTransform != null)
        {
            spriteTransform.localPosition = baseSpriteLocalPosition;
            deathSettleCoroutine = StartCoroutine(AjustarSpriteDuranteMuerte());
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (anim != null)
        {
            anim.SetInteger(HashComboStep, 0);
            anim.SetFloat(HashSpeed, 0f);
            anim.SetBool(HashIsRunning, false);
            anim.SetBool(HashIsJumping, false);
            anim.ResetTrigger(HashAttack);
            anim.ResetTrigger(HashTakeDamage1);
            anim.ResetTrigger(HashTakeDamage2);
            anim.Play(StateDeath, 0, 0f);
        }
    }

    private void ConfigurarGolpeFinal()
    {
        ConfigurarHitboxGolpeFinal(hitboxAtk1, false);
        ConfigurarHitboxGolpeFinal(hitboxAtk2, false);
        ConfigurarHitboxGolpeFinal(hitboxAtk3, false);
        ConfigurarHitboxGolpeFinal(hitboxAtk4, false);
    }

    private void ConfigurarHitboxGolpeFinal(GameObject hitbox, bool value)
    {
        if (hitbox == null)
        {
            return;
        }

        AtaqueJugador scriptAtk = hitbox.GetComponent<AtaqueJugador>();
        if (scriptAtk != null)
        {
            scriptAtk.esGolpeFinal = value;
        }
    }

    private void ActualizarAnimacionMovimiento()
    {
        if (anim == null)
        {
            return;
        }

        if (state == PlayerState.Dead)
        {
            return;
        }

        if (!isJumping && !isAttacking && state != PlayerState.Stunned)
        {
            anim.SetFloat(HashSpeed, moveInput.sqrMagnitude);
            anim.SetBool(HashIsRunning, isRunning && moveInput.sqrMagnitude > 0f);
        }

        anim.SetBool(HashIsJumping, isJumping);
    }

    private void ActualizarSalto()
    {
        if (!isJumping || spriteTransform == null)
        {
            return;
        }

        currentJumpVelocity -= gravity * Time.deltaTime;
        spriteTransform.localPosition += new Vector3(0f, currentJumpVelocity * Time.deltaTime, 0f);

        if (spriteTransform.localPosition.y <= baseSpriteLocalPosition.y)
        {
            spriteTransform.localPosition = baseSpriteLocalPosition;
            isJumping = false;
            currentJumpVelocity = 0f;
        }
    }

    private IEnumerator AjustarSpriteDuranteMuerte()
    {
        if (spriteTransform == null)
        {
            yield break;
        }

        Vector3 inicio = baseSpriteLocalPosition;
        Vector3 destino = baseSpriteLocalPosition + new Vector3(0f, deathGroundOffsetY, 0f);
        float duracion = Mathf.Max(0.02f, deathSettleSeconds);
        float tiempo = 0f;

        while (tiempo < duracion && state == PlayerState.Dead)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.Clamp01(tiempo / duracion);
            float suavizado = 1f - (1f - t) * (1f - t);
            spriteTransform.localPosition = new Vector3(
                inicio.x + (destino.x - inicio.x) * suavizado,
                inicio.y + (destino.y - inicio.y) * suavizado,
                inicio.z + (destino.z - inicio.z) * suavizado
            );
            yield return null;
        }

        if (state == PlayerState.Dead && spriteTransform != null)
        {
            spriteTransform.localPosition = destino;
        }

        deathSettleCoroutine = null;
    }

    private IEnumerator FallbackFinAtaque()
    {
        yield return new WaitForSeconds(attackFallbackSeconds);

        if (isAttacking && state == PlayerState.Attacking)
        {
            Debug.LogWarning($"{name}: falta Animation Event FinAtaque(). Se libera el ataque por fallback.");
            attackFallbackCoroutine = null;
            FinAtaque();
        }
    }

    private IEnumerator FallbackFinStun(float duracion)
    {
        yield return new WaitForSeconds(duracion);

        if (state == PlayerState.Stunned)
        {
            stunFallbackCoroutine = null;
            FinStunJugador();
        }
    }

    private void DetenerFallbackAtaque()
    {
        if (attackFallbackCoroutine != null)
        {
            StopCoroutine(attackFallbackCoroutine);
            attackFallbackCoroutine = null;
        }
    }

    private void DetenerFallbackStun()
    {
        if (stunFallbackCoroutine != null)
        {
            StopCoroutine(stunFallbackCoroutine);
            stunFallbackCoroutine = null;
        }
    }

    private void DetenerAjusteMuerte()
    {
        if (deathSettleCoroutine != null)
        {
            StopCoroutine(deathSettleCoroutine);
            deathSettleCoroutine = null;
        }
    }
}
