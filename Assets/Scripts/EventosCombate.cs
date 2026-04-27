using UnityEngine;

public class EventosCombate : MonoBehaviour
{
    private PlayerController player;

    void Start()
    {
        // Al iniciar, busca a su "padre" (PlayerBase) y se conecta a su cerebro
        player = GetComponentInParent<PlayerController>();
    }

    // Estas son las funciones que la ventana Animation AHORA SÍ verá
    public void AbrirHitbox(int id)
    {
        if (player != null) player.AbrirHitbox(id);
    }

    public void CerrarHitboxes()
    {
        if (player != null) player.CerrarHitboxes();
    }
}