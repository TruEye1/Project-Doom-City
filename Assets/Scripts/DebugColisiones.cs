using UnityEngine;

public class DebugColisiones : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("¡CONTACTO! El objeto " + gameObject.name + " acaba de tocar a: " + collision.gameObject.name);
    }
}