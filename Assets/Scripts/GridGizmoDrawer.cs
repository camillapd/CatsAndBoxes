using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridGizmoDrawer : MonoBehaviour
{
    public int width = 16;   // largura em tiles
    public int height = 14;  // altura em tiles
    public Color gizmoColor = new Color(0f, 1f, 1f, 0.3f); // ciano transparente

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        // A posição do GameObject (Grid)
        Vector3 pos = transform.position;

        // Desenha um retângulo preenchido no plano XY com o tamanho da grid
        Vector3 size = new Vector3(width, height, 0);

        // Centro do retângulo no meio da grid
        Vector3 center = pos + size / 2f;

        // Desenha caixa sem preenchimento
        Handles.color = gizmoColor;
        Handles.DrawSolidRectangleWithOutline(new Rect(pos.x, pos.y, width, height), gizmoColor, Color.cyan);

        // Opcional: desenhar uma cruz no centro
        float crossSize = 0.3f;
        Handles.color = Color.cyan;
        Handles.DrawLine(center + Vector3.left * crossSize, center + Vector3.right * crossSize);
        Handles.DrawLine(center + Vector3.up * crossSize, center + Vector3.down * crossSize);
    }
#endif
}
