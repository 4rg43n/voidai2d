using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Draws a configurable grid and a narrow line on top, compatible with URP/HDRP (SRP) and Built-in.
/// Attach to any active GameObject. Renders in both Scene and Game views.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class GridLineOverlay : MonoBehaviour
{
    [Header("Path Overlay")]
    public bool drawSquares = true;
    public List<TileCell> pathTiles = new();           // assign your A* path here
    public Color pathColor = new Color(1f, 0f, 0f, 0.35f);
    public float squaresZOffset = 0.001f;              // draws slightly above the grid



    [Header("Grid (in cells)")]
    [Min(1)] public int columns = 10;
    [Min(1)] public int rows = 10;

    [Header("Cell Size (world units)")]
    [Min(0.0001f)] public float cellWidth = 1f;
    [Min(0.0001f)] public float cellHeight = 1f;

    [Header("Grid Visuals")]
    public bool drawGrid = true;
    public Color gridColor = new Color(1f, 1f, 1f, 0.15f); // faint white lines

    [Header("Line Over Grid")]
    public Vector2 startCell = new Vector2(0, 0);
    public Vector2 endCell = new Vector2(5, 7);
    public bool useCellCenters = true;
    [Min(0.0001f)] public float lineThickness = 0.02f;
    public Color lineColor = Color.red;
    [Range(0f, 1f)] public float lineAlpha = 1f;

    [Header("Z Offset")]
    [Tooltip("Local Z offset for the GL overlay to avoid z-fighting with other geometry.")]
    public float zOffset = 0f;

    private static Material s_lineMat;

    public float GridWorldWidth => columns * cellWidth;
    public float GridWorldHeight => rows * cellHeight;

    // --- Lifecycle -----------------------------------------------------------

    private void OnValidate()
    {
        columns = Mathf.Max(1, columns);
        rows = Mathf.Max(1, rows);
        cellWidth = Mathf.Max(0.0001f, cellWidth);
        cellHeight = Mathf.Max(0.0001f, cellHeight);
        lineThickness = Mathf.Max(0.0001f, lineThickness);
    }

    private void OnEnable()
    {
        EnsureMaterial();
        RenderPipelineManager.endCameraRendering += OnEndCameraRenderingSRP;
    }

    private void OnDisable()
    {
        RenderPipelineManager.endCameraRendering -= OnEndCameraRenderingSRP;
    }

    // Built-in pipeline fallback
    private void OnPostRender()
    {
        if (GraphicsSettings.currentRenderPipeline != null) return; // SRP active, handled elsewhere
        if (!enabled || s_lineMat == null) return;
        var cam = Camera.current;
        if (cam == null) return;

        DrawForCamera(cam);
    }

    // SRP (URP/HDRP) path
    private void OnEndCameraRenderingSRP(ScriptableRenderContext ctx, Camera cam)
    {
        if (!enabled || s_lineMat == null) return;

        // Skip previews/reflections if you want; keep Scene/Game cameras
        if (cam.cameraType == CameraType.Preview || cam.cameraType == CameraType.Reflection) return;

        DrawForCamera(cam);
    }

    // --- Public helpers ------------------------------------------------------

    /// <summary>Convert a cell coordinate (x,y in cells) to local space position (XY plane).</summary>
    public Vector3 CellToLocal(Vector2 cell, bool toCenter = true)
    {
        float x = cell.x * cellWidth;
        float y = cell.y * cellHeight;
        if (toCenter)
        {
            x += cellWidth * 0.5f;
            y += cellHeight * 0.5f;
        }
        return new Vector3(x, y, zOffset);
    }

    public void SetLineCells(Vector2 start, Vector2 end, bool center = true)
    {
        startCell = start;
        endCell = end;
        useCellCenters = center;
    }

    // --- Drawing core --------------------------------------------------------

    private void DrawForCamera(Camera cam)
    {
        // Prepare GL matrices for this camera (SRP-safe)
        s_lineMat.SetPass(0);
        GL.PushMatrix();
        GL.LoadProjectionMatrix(cam.projectionMatrix);

        // ModelView = camera view * our local-to-world
#if UNITY_2021_2_OR_NEWER
        GL.modelview = cam.worldToCameraMatrix * transform.localToWorldMatrix;
#else
        // Older fallback: multiply into current matrix
        GL.MultMatrix(cam.worldToCameraMatrix * transform.localToWorldMatrix);
#endif

        if (drawGrid)
            DrawGridLines();


        if (drawSquares && pathTiles != null && pathTiles.Count > 0)
        {
            DrawSquares(pathTiles, cellWidth, cellHeight,
                new Color(pathColor.r, pathColor.g, pathColor.b,
                          Mathf.Clamp01(pathColor.a)));
        }


        //DrawThickLine(
        //    CellToLocal(startCell, useCellCenters),
        //    CellToLocal(endCell, useCellCenters),
        //    lineThickness,
        //    new Color(lineColor.r, lineColor.g, lineColor.b, Mathf.Clamp01(lineAlpha) * lineColor.a)
        //);

        GL.PopMatrix();
    }

    /// <summary>
    /// Fills a quad per cell. Each TileCell.WorldPosition is treated as the
    /// BOTTOM-LEFT corner in WORLD space. Width/Height are WORLD-units.
    /// Converts to this overlay's LOCAL space so existing GL matrices work.
    /// </summary>
    public void DrawSquares(IList<TileCell> cells, float width, float height, Color c)
    {
        if (cells == null || cells.Count == 0) return;

        // We’re already in the correct GL matrix scope (camera proj + modelview = cam * localToWorld)
        // So we must submit LOCAL-space vertices. Convert each world corner → local.
        GL.Begin(GL.TRIANGLES);
        GL.Color(c);

        for (int i = 0; i < cells.Count; i++)
        {
            var cell = cells[i];
            if (cell == null) continue;

            // Corners in WORLD space (bottom-left is provided by the TileCell)
            Vector3 blW = cell.WorldPosition;
            Vector3 brW = blW + new Vector3(width, 0f, 0f);
            Vector3 tlW = blW + new Vector3(0f, height, 0f);
            Vector3 trW = blW + new Vector3(width, height, 0f);

            // Convert to LOCAL space for this overlay
            Vector3 bl = transform.InverseTransformPoint(blW); bl.z = zOffset + squaresZOffset;
            Vector3 br = transform.InverseTransformPoint(brW); br.z = zOffset + squaresZOffset;
            Vector3 tl = transform.InverseTransformPoint(tlW); tl.z = zOffset + squaresZOffset;
            Vector3 tr = transform.InverseTransformPoint(trW); tr.z = zOffset + squaresZOffset;

            // Two triangles (bl, tl, tr) and (bl, tr, br)
            GL.Vertex(bl); GL.Vertex(tl); GL.Vertex(tr);
            GL.Vertex(bl); GL.Vertex(tr); GL.Vertex(br);
        }

        GL.End();
    }

    private void DrawGridLines()
    {
        var c = gridColor;
        GL.Begin(GL.LINES);
        GL.Color(c);

        float w = GridWorldWidth;
        float h = GridWorldHeight;

        // Vertical lines
        for (int i = 0; i <= columns; i++)
        {
            float x = i * cellWidth;
            GL.Vertex3(x, 0f, 0f);
            GL.Vertex3(x, h, 0f);
        }

        // Horizontal lines
        for (int j = 0; j <= rows; j++)
        {
            float y = j * cellHeight;
            GL.Vertex3(0f, y, 0f);
            GL.Vertex3(w, y, 0f);
        }

        GL.End();
    }

    private void DrawThickLine(Vector3 a, Vector3 b, float thickness, Color color)
    {
        Vector3 dir = b - a;
        dir.z = 0f;
        if (dir.sqrMagnitude < 1e-12f) return;

        Vector3 n = new Vector3(-dir.y, dir.x, 0f).normalized;
        Vector3 half = n * (thickness * 0.5f);

        Vector3 v0 = a - half;
        Vector3 v1 = a + half;
        Vector3 v2 = b + half;
        Vector3 v3 = b - half;

        GL.Begin(GL.TRIANGLES);
        GL.Color(color);

        // Triangle 1
        GL.Vertex(v0); GL.Vertex(v1); GL.Vertex(v2);
        // Triangle 2
        GL.Vertex(v0); GL.Vertex(v2); GL.Vertex(v3);

        GL.End();
    }

    private static void EnsureMaterial()
    {
#if UNITY_EDITOR
        if (s_lineMat != null && s_lineMat.shader == null) s_lineMat = null;
#endif
        if (s_lineMat != null) return;

        Shader shader = Shader.Find("Hidden/Internal-Colored");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        if (shader != null)
        {
            s_lineMat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            // Transparent overlay settings
            s_lineMat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            s_lineMat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            s_lineMat.SetInt("_Cull", (int)CullMode.Off);
            s_lineMat.SetInt("_ZWrite", 0);
            // Force draw on top when possible
            if (s_lineMat.HasProperty("_ZTest"))
                s_lineMat.SetInt("_ZTest", (int)CompareFunction.Always);
            s_lineMat.renderQueue = 5000; // overlay
        }
        else
        {
            Debug.LogWarning("GridLineOverlay: No suitable shader found for GL drawing.");
        }
    }
}
