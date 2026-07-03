using UnityEngine;

public class EnemigoIA : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float velocidadX = 2.5f;
    public float velocidadY = 1.5f;

    [Header("Distancias de Combate")]
    public float distanciaAtaqueX = 2.2f;
    [Tooltip("El margen de tolerancia para evitar el temblor por colisiones.")]
    public float tolerancia = 0.15f;

    [Header("Alineación Vertical")]
    public float offsetY = 1.5f;

    private Transform jugador;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private bool isCurrentlyWalking = false;

    void Start()
    {
        // CORREGIDO: Ahora es GetComponent<Rigidbody2D>()
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject jugadorObj = GameObject.Find("PlayerBase");
        if (jugadorObj != null) jugador = jugadorObj.transform;
    }

    void FixedUpdate()
    {
        if (jugador == null) return;

        float targetY = jugador.position.y - offsetY;
        float diferenciaX = jugador.position.x - rb.position.x;
        float distanciaAbsX = Mathf.Abs(diferenciaX);
        float distanciaAbsY = Mathf.Abs(targetY - rb.position.y);

        // LÓGICA DE TOLERANCIA:
        bool esObjetivoAlcanzado = (distanciaAbsX <= (distanciaAtaqueX + tolerancia));

        bool debeCaminarX = isCurrentlyWalking ?
                            !esObjetivoAlcanzado :
                            (distanciaAbsX > (distanciaAtaqueX + tolerancia));

        bool debeCaminarY = distanciaAbsY > 0.2f;
        bool shouldBeWalking = debeCaminarX || debeCaminarY;

        if (shouldBeWalking != isCurrentlyWalking)
        {
            isCurrentlyWalking = shouldBeWalking;
            anim.SetBool("isWalking", isCurrentlyWalking);
        }

        if (shouldBeWalking)
        {
            float destinoX = jugador.position.x - (Mathf.Sign(diferenciaX) * distanciaAtaqueX);

            Vector2 nuevaPos = new Vector2(
                Mathf.MoveTowards(rb.position.x, destinoX, velocidadX * Time.fixedDeltaTime),
                Mathf.MoveTowards(rb.position.y, targetY, velocidadY * Time.fixedDeltaTime)
            );

            rb.MovePosition(nuevaPos);
            spriteRenderer.flipX = (diferenciaX < -0.1f);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}