using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldGenerator))]
public class TilemapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WorldGenerator tilemapGenerator = (WorldGenerator)target;

        if (GUILayout.Button("Generate"))
        {
            tilemapGenerator.Generate();
        }

        if (GUILayout.Button("Clear"))
        {
            tilemapGenerator.Clear();
        }
    }
}
