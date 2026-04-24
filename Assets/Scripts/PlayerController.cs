using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float speed = 5f;

    [Header("Configuración de Salto (Eje Z Falso)")]
    public Transform spriteTransform; // Referencia al objeto hijo que contiene el Sprite y el Animator
    public float jumpPower = 7f;
    public float gravity = 15f;

    private float currentJumpVelocity;
    private bool isJumping = false;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 movement;

    private PlayerControls controls;

    void Awake()
    {
        // Inicializar el Action Map del New Input System
        controls = new PlayerControls();

        // Suscribir el método RealizarSalto al evento "performed" del input Jump
        controls.Player.Jump.performed += ctx => RealizarSalto();
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Start()
    {
        // El Rigidbody2D maneja las colisiones de la base (sombra)
        rb = GetComponent<Rigidbody2D>();

        // El Animator se obtiene del objeto hijo para separar la lógica visual de la física
        if (spriteTransform != null)
        {
            anim = spriteTransform.GetComponent<Animator>();
        }
    }

    void Update()
    {
        // ¡El Guardia de Seguridad! 
        // Si el tiempo está congelado (pausa), aborta y no leas el teclado.
        if (Time.timeScale == 0f) return;

        // 1. Leer el vector de dirección desde el Input System
        movement = controls.Player.Move.ReadValue<Vector2>();

        // 2. Gestionar la orientación visual del sprite (Flip)
        if (movement.x > 0)
        {
            spriteTransform.localScale = new Vector3(1, 1, 1);
        }
        else if (movement.x < 0)
        {
            spriteTransform.localScale = new Vector3(-1, 1, 1);
        }

        // 3. Sincronizar estados con el Animator
        if (anim != null)
        {
            // Solo actualiza la animación de caminar si el personaje está en el suelo
            if (!isJumping)
            {
                anim.SetFloat("Speed", movement.sqrMagnitude);
            }
            anim.SetBool("IsJumping", isJumping);
        }

        // 4. Lógica de físicas para el salto en perspectiva 2.5D
        // Se desplaza únicamente el Transform local del hijo en el eje Y para simular altura
        if (isJumping)
        {
            currentJumpVelocity -= gravity * Time.deltaTime;
            spriteTransform.localPosition += new Vector3(0, currentJumpVelocity * Time.deltaTime, 0);

            // Detectar aterrizaje cuando la posición local Y del sprite vuelve a la base (0)
            if (spriteTransform.localPosition.y <= 0)
            {
                spriteTransform.localPosition = Vector3.zero; // Forzar el anclaje exacto al suelo
                isJumping = false;
                currentJumpVelocity = 0;
            }
        }
    }

    void FixedUpdate()
    {
        // Movimiento físico independiente de los frames para evitar jittering
        rb.MovePosition(rb.position + movement.normalized * speed * Time.fixedDeltaTime);
    }

    private void RealizarSalto()
    {
        // Método invocado por el Action Map para iniciar la parábola del salto
        if (!isJumping)
        {
            isJumping = true;
            currentJumpVelocity = jumpPower;
        }
    }
}