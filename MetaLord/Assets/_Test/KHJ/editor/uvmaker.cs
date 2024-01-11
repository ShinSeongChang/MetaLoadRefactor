using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class uvmaker : EditorWindow
{
    [MenuItem("Custom/LoadAndSetUV")]
    static void LoadAndSetUV()
    {
        // Load the mesh asset
        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/_Test/KHJ/Models/Node-Mesh.mesh");

        if (mesh != null)
        {
            // Modify UVs (example: simple UV mapping)
            Vector2[] uv = new Vector2[mesh.vertices.Length];
            for (int i = 0; i < uv.Length; i++)
            {
                uv[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].z);
            }
            mesh.uv = uv;


            // Set UV1 (lightmap or additional texture mapping)
            Vector2[] uv1 = new Vector2[mesh.vertices.Length];
            for (int i = 0; i < uv1.Length; i++)
            {
                // Your UV1 mapping logic goes here
                uv1[i] = new Vector2(mesh.vertices[i].y, mesh.vertices[i].x);
            }
            mesh.uv2 = uv1;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        else
        {
            Debug.LogError("Mesh not found at the specified path.");
        }
    }
}