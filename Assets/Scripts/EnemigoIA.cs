using UnityEngine;

public class EnemigoIA : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float velocidadX = 2.5f;
    public float velocidadY = 1.5f;

    [Header("Distancias de Combate")]
    public float distanciaAtaqueX = 2.2f;
    public float tolerancia = 0.15f;

    [Header("Alineación Vertical")]
    public float offsetY = 1.5f;

    [Header("Sistema de Combate")]
    public int vidaMaxima = 100;
    public float tiempoEntreAtaques = 1.5f;

    [Header("Hitbox de Ataque")]
    public GameObject hitboxAtaque;

    private int vidaActual;
    private float proximoAtaqueTiempo = 0f;
    private bool estaMuerto = false;

    private Transform jugador;
    private Rigidbody2D rb;
    private Animator anim;
    private bool isCurrentlyWalking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        vidaActual = vidaMaxima;

        GameObject jugadorObj = GameObject.Find("PlayerBase");
        if (jugadorObj != null) jugador = jugadorObj.transform;
    }

    void FixedUpdate()
    {
        if (jugador == null || estaMuerto) return;

        float targetY = jugador.position.y - offsetY;
        float diferenciaX = jugador.position.x - rb.position.x;
        float distanciaAbsX = Mathf.Abs(diferenciaX);
        float distanciaAbsY = Mathf.Abs(targetY - rb.position.y);

        bool esObjetivoAlcanzado = (distanciaAbsX <= (distanciaAtaqueX + tolerancia));

        if (esObjetivoAlcanzado && Time.time >= proximoAtaqueTiempo)
        {
            Atacar();
        }

        bool debeCaminarX = isCurrentlyWalking ? !esObjetivoAlcanzado : (distanciaAbsX > (distanciaAtaqueX + tolerancia));
        bool debeCaminarY = distanciaAbsY > 0.2f;
        bool shouldBeWalking = (debeCaminarX || debeCaminarY) && !esObjetivoAlcanzado;

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

            if (diferenciaX > 0.1f) transform.localScale = new Vector3(1, 1, 1);
            else if (diferenciaX < -0.1f) transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void Atacar()
    {
        if (hitboxAtaque != null) hitboxAtaque.SetActive(false);
        int tipoAtaque = Random.Range(1, 3);
        anim.SetTrigger(tipoAtaque == 1 ? "Attack1" : "Attack2");
        proximoAtaqueTiempo = Time.time + tiempoEntreAtaques;
    }

    public void RecibirDano(int dano, bool esKnockout)
    {
        // Si es knockout, seguimos usando el trigger que ya funciona
        if (esKnockout)
        {
            anim.SetTrigger("Knockout");
        }
        else
        {
            // Generamos un número aleatorio entre 0 y 2
            // (0 = TakeDamage, 1 = Hit_1, 2 = Hit_2)
            int numeroAleatorio = Random.Range(0, 3);

            // Enviamos el número al Animator
            anim.SetInteger("HitIndex", numeroAleatorio);

            // Disparamos un trigger "dummy" o simplemente el parámetro nuevo
            // Si tus transiciones dependen del Integer, esto será suficiente.
            // Opcional: puedes mantener un trigger "TakeDamage" para asegurar la transición
            anim.SetTrigger("TakeDamage");
        }
    }

    private void Morir()
    {
        estaMuerto = true;
        anim.SetBool("isDead", true);
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
    }

    public void ActivarHitbox() { if (hitboxAtaque != null) hitboxAtaque.SetActive(true); }
    public void DesactivarHitbox() { if (hitboxAtaque != null) hitboxAtaque.SetActive(false); }
}