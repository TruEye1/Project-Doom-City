using UnityEngine;

public class PickupRotatorOrBob : MonoBehaviour
{
    [SerializeField] private float amplitud = 0.08f;
    [SerializeField] private float velocidad = 2f;

    private Vector3 posicionInicial;

    private void Awake()
    {
        posicionInicial = transform.position;
    }

    private void Update()
    {
        float y = Mathf.Sin(Time.time * velocidad) * amplitud;
        transform.position = posicionInicial + new Vector3(0f, y, 0f);
    }
}
