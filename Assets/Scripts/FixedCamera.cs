using UnityEngine;
[RequireComponent(typeof(Camera))]
public class FixedCamera : MonoBehaviour
{
    public int pixelsPerUnit = 32;
    public int tilesVertical = 14;

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = tilesVertical / 2f;
    }

    void LateUpdate()
    {
        // pixel perfect na movimentação da câmera
        float unitsPerPixel = 1f / pixelsPerUnit;
        Vector3 pos = cam.transform.position;

        pos.x = Mathf.Round(pos.x / unitsPerPixel) * unitsPerPixel;
        pos.y = Mathf.Round(pos.y / unitsPerPixel) * unitsPerPixel;

        cam.transform.position = pos;
    }
}
