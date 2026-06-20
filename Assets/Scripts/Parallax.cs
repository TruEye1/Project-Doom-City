using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Transform cam;
    public float parallaxFactor;

    private float startPosX;

    void Start()
    {
        startPosX = transform.position.x;
    }

    void LateUpdate()
    {
        float distance = cam.position.x * parallaxFactor;
        transform.position = new Vector3(startPosX + distance, transform.position.y, transform.position.z);
    }
}