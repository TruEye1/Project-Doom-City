using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
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
    private float lastClickTime;
    private bool isAttacking = false;

    [Header("Referencias de Hitboxes")]
    public GameObject hitboxAtk1;
    public GameObject hitboxAtk2;
    public GameObject hitboxAtk3;
    public GameObject hitboxAtk4;

    private Rigidbody2D rb;
    private Animator anim;
    private PlayerControls controls;
    private Vector2 moveInput;

    void Awake()
    {
        // Inicialización de Input System y suscripción de eventos
        controls = new PlayerControls();

        controls.Player.Jump.performed += ctx => RealizarSalto();
        controls.Player.Run.performed += ctx => isRunning = true;
        controls.Player.Run.canceled += ctx => isRunning = false;
        controls.Player.Attack.performed += ctx => IntentarAtacar();
    }

    void OnEnable() { controls.Enable(); }
    void OnDisable() { controls.Disable(); }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Obtener referencias del objeto visual (hijo)
        anim = GetComponentInChildren<Animator>();

        // Auto-asignación de transform visual si no está definido en el Inspector
        if (spriteTransform == null && anim != null)
        {
            spriteTransform = anim.transform;
        }
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        // Reinicio del temporizador de combo (Input Buffer)
        if (Time.time > lastClickTime + comboWindow && !isAttacking)
        {
            comboStep = 0;
            if (anim != null) anim.SetInteger("ComboStep", 0);
        }

        // Control de movimiento y State Lock
        if (isAttacking || isJumping)
        {
            moveInput = Vector2.zero;

            // Forzar detención de animaciones de movimiento al atacar
            if (anim != null)
            {
                anim.SetFloat("Speed", 0);
                anim.SetBool("IsRunning", false);
            }
        }
        else
        {
            moveInput = controls.Player.Move.ReadValue<Vector2>();
        }

        // Orientación del Sprite (Flip)
        if (moveInput.x > 0)
        {
            spriteTransform.localScale = new Vector3(1, 1, 1);
        }
        else if (moveInput.x < 0)
        {
            spriteTransform.localScale = new Vector3(-1, 1, 1);
        }

        // Sincronización de variables del Animator
        if (anim != null)
        {
            if (!isJumping && !isAttacking)
            {
                anim.SetFloat("Speed", moveInput.sqrMagnitude);
                anim.SetBool("IsRunning", isRunning && moveInput.sqrMagnitude > 0);
            }
            anim.SetBool("IsJumping", isJumping);
        }

        // Simulación de gravedad en eje Z (falso 3D)
        if (isJumping)
        {
            currentJumpVelocity -= gravity * Time.deltaTime;
            spriteTransform.localPosition += new Vector3(0, currentJumpVelocity * Time.deltaTime, 0);

            // Detección de suelo
            if (spriteTransform.localPosition.y <= 0)
            {
                spriteTransform.localPosition = Vector3.zero;
                isJumping = false;
                currentJumpVelocity = 0;
            }
        }
    }

    void FixedUpdate()
    {
        // Bloqueo físico durante el estado de ataque
        if (isAttacking) return;

        float currentSpeed = isRunning ? runSpeed : speed;
        rb.MovePosition(rb.position + moveInput.normalized * currentSpeed * Time.fixedDeltaTime);
    }

    private void RealizarSalto()
    {
        // Bloqueo de evento temporal: Ignorar el input si el juego está en pausa
        if (Time.timeScale == 0f) return;

        if (!isJumping && !isAttacking)
        {
            isJumping = true;
            currentJumpVelocity = jumpPower;
        }
    }

    // --- LÓGICA DE COMBATE ---

    private void IntentarAtacar()
    {
        // Bloqueo de evento temporal: Ignorar el input si el juego está en pausa
        if (Time.timeScale == 0f) return;

        // Validación de estado para iniciar secuencia de ataque
        if (!isJumping && !isAttacking)
        {
            StartCoroutine(EjecutarAtaque());
        }
    }

    private IEnumerator EjecutarAtaque()
    {
        isAttacking = true;
        comboStep++;

        // Ciclar el combo al superar el último golpe (Finisher)
        if (comboStep > 4) comboStep = 1;

        lastClickTime = Time.time;

        if (anim != null)
        {
            anim.SetInteger("ComboStep", comboStep);
            anim.SetTrigger("Attack");
        }

        // Duración del State Lock ajustada para animaciones a velocidad 0.8x
        // Valores originales (1.0x): 0.6f y 0.35f -> Nuevos (0.8x): 0.75f y 0.45f
        float pauseTime = (comboStep == 4) ? 0.75f : 0.45f;

        yield return new WaitForSeconds(pauseTime);

        // Fin de la secuencia de ataque, restaurar control
        isAttacking = false;
    }

    // --- ANIMATION EVENTS ---

    public void AbrirHitbox(int id)
    {
        // Limpiar estado de hitboxes para evitar colisiones superpuestas
        CerrarHitboxes();

        if (id == 1 && hitboxAtk1 != null) hitboxAtk1.SetActive(true);
        if (id == 2 && hitboxAtk2 != null) hitboxAtk2.SetActive(true);
        if (id == 3 && hitboxAtk3 != null) hitboxAtk3.SetActive(true);
        if (id == 4 && hitboxAtk4 != null) hitboxAtk4.SetActive(true);
    }

    public void CerrarHitboxes()
    {
        if (hitboxAtk1 != null) hitboxAtk1.SetActive(false);
        if (hitboxAtk2 != null) hitboxAtk2.SetActive(false);
        if (hitboxAtk3 != null) hitboxAtk3.SetActive(false);
        if (hitboxAtk4 != null) hitboxAtk4.SetActive(false);
    }
}