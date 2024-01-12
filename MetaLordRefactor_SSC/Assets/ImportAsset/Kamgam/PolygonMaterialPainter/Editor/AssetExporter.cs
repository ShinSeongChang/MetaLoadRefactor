﻿using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kamgam.PolygonMaterialPainter
{
    public static class AssetExporter
    {
#if UNITY_EDITOR
        public static void SaveMeshAsAsset(Mesh mesh, string assetPath, bool logFilePaths)
        {
            if (mesh == null)
                return;

            assetPath = MakePathRelativeToProjectRoot(assetPath);

            string dirPath = System.IO.Path.GetDirectoryName(Application.dataPath + "/../" + assetPath);
            if (!System.IO.Directory.Exists(dirPath))
            {
                System.IO.Directory.CreateDirectory(dirPath);
            }

            // Check if the asset already exists.
            var existingMesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (existingMesh != null)
            {
                Undo.RegisterCompleteObjectUndo(existingMesh, "Create new mesh");
            }

            AssetDatabase.CreateAsset(mesh, assetPath);
            AssetDatabase.SaveAssets();
            // Important to force the reimport to avoid the "SkinnedMeshRenderer: Mesh has
            // been changed to one which is not compatibile with the expected mesh data size
            // and vertex stride." error.
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();

            if (logFilePaths)
                Logger.LogMessage($"Saved new mesh under <color=yellow>'{assetPath}'</color>.");
        }

        public static string MakePathRelativeToProjectRoot(string assetPath)
        {
            // Make path relative
            assetPath = assetPath.Replace("\\", "/");
            var dataPath = Application.dataPath.Replace("\\", "/");
            assetPath = assetPath.Replace(dataPath, "");

            // Ensure the path starts with "Assets/".
            if (!assetPath.StartsWith("Assets"))
            {
                if (assetPath.StartsWith("/"))
                {
                    assetPath = "Assets" + assetPath;
                }
                else
                {
                    assetPath = "Assets/" + assetPath;
                }
            }

            return assetPath;
        }
#endif

    }
}

