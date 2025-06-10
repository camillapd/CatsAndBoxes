using UnityEngine;

public class CameraController : MonoBehaviour
{
    public int tileHeight = 16;
    public float pixelsPerUnit = 32f;

    void Start()
    {
        // Mostrar exatamente 16 tiles na vertical
        Camera.main.orthographicSize = tileHeight / 2f;
    }
}

