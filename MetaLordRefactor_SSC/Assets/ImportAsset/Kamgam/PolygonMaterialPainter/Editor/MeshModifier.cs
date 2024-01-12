using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;
using System.Text.RegularExpressions;

namespace Kamgam.PolygonMaterialPainter
{
    public class MeshModifier
    {
        const string FILE_ENDING = ".pmp";
        /// <summary>
        /// The file extensions first part is used to decide whether to
        /// simply update the mesh inside the asset or to create a new
        /// mesh asset file.
        /// </summary>
        const string FILE_EXTENSION = FILE_ENDING + ".asset";

        public Component Component;
        public GameObject GameObject;
        public Mesh NewMesh;
        public SkinnedMeshRenderer SkinnedMeshRenderer;
        public MeshRenderer MeshRenderer;
        public MeshFilter MeshFilter;
        public bool DeleteNewMeshAfterResetToOriginal;
        public int LastAssignedSubMeshIndex;
        public Material LastAssignedMaterial;
        public bool IsSkinned => SkinnedMeshRenderer != null;

        protected bool _hasOriginalMesh;
        public bool HasOriginalMesh => _hasOriginalMesh;

        protected Mesh _originalMesh;
        protected Mesh _originalMeshCopy;
        protected Material[] _originalMaterials;

        protected bool _newMeshExistsAsAsset;

        public static bool IsNameOfEditedMesh(string nameOrPath)
        {
            if (nameOrPath == null)
                return false;

            return nameOrPath.EndsWith(FILE_EXTENSION) || nameOrPath.EndsWith(FILE_ENDING);
        }

        /// <summary>
        /// New Mesh Modifier operating on the first sharedMesh it finds on the given component (MeshRenderer, MeshFilter or SkinnedMeshRenderer).
        /// </summary>
        /// <param name="comp">Either a MeshRenderer, MeshFilter or SkinnedMeshRenderer</param>
        public MeshModifier(Component comp)
        {
            Component = comp;
            MeshFilter = comp as MeshFilter;
            MeshRenderer = comp as MeshRenderer;

            if (MeshFilter != null && MeshRenderer == null)
                MeshRenderer = MeshFilter.gameObject.GetComponent<MeshRenderer>();

            if (MeshFilter == null && MeshRenderer != null)
                MeshFilter = MeshRenderer.gameObject.GetComponent<MeshFilter>();

            SkinnedMeshRenderer = comp as SkinnedMeshRenderer;

            if ((MeshFilter == null || MeshRenderer == null) && SkinnedMeshRenderer == null)
            {
                throw new System.ArgumentException(comp + " does not contain any meshes.", "go");
            }

            if ((MeshFilter != null && MeshFilter.sharedMesh == null) || (SkinnedMeshRenderer != null && SkinnedMeshRenderer.sharedMesh == null))
            {
                throw new System.ArgumentException(comp + " does not contain any meshes.", "go");
            }

            Mesh currentMesh;
            if (IsSkinned)
            {
                _originalMeshCopy = new Mesh();
                copyMesh(SkinnedMeshRenderer.sharedMesh, _originalMeshCopy);
                _originalMaterials = SkinnedMeshRenderer.sharedMaterials;

                currentMesh = SkinnedMeshRenderer.sharedMesh;
            }
            else
            {
                _originalMeshCopy = new Mesh();
                copyMesh(MeshFilter.sharedMesh, _originalMeshCopy);
                _originalMaterials = MeshRenderer.sharedMaterials;

                currentMesh = MeshFilter.sharedMesh;
            }

            if (_originalMaterials.Length > 0)
            {
                LastAssignedMaterial = _originalMaterials[0];
                LastAssignedSubMeshIndex = 0;
            }

            _originalMesh = currentMesh;
            _hasOriginalMesh = false;
            var currentPath = AssetDatabase.GetAssetPath(currentMesh);
            // The current mesh aleady is an edited mesh. Try to find the original for it by removing the file extension.
            if (currentPath.EndsWith(FILE_EXTENSION))
            {
                Mesh newOriginalMesh = LoadOriginalFromNewMeshPath(currentPath);
                if (newOriginalMesh != null)
                {
                    _originalMesh = newOriginalMesh;
                    var newOriginalMaterials = new Material[_originalMesh.subMeshCount];
                    // Sadly we do no longer have the original material array here.
                    // We try to copy the current one but it may not match the original ones.
                    for (int i = 0; i < newOriginalMaterials.Length; i++)
                    {
                        if (i < _originalMaterials.Length)
                        {
                            newOriginalMaterials[i] = _originalMaterials[i];
                        }
                    }
                    _originalMaterials = newOriginalMaterials;
                    _hasOriginalMesh = true;
                }
            }
            else
            {
                _hasOriginalMesh = true;
            }

            initNewMesh();
        }

        void copyMesh(Mesh source, Mesh target)
        {
            target.indexFormat = source.vertexCount > 65536 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            target.vertices = source.vertices;
            target.normals = source.normals;
            target.tangents = source.tangents;
            target.colors = source.colors;
            target.uv = source.uv;
            target.uv2 = source.uv2;
            target.uv3 = source.uv3;
            target.uv4 = source.uv4;
            target.uv5 = source.uv5;
            target.uv6 = source.uv6;
            target.uv7 = source.uv7;
            target.uv8 = source.uv8;
            target.SetBoneWeights(source.GetBonesPerVertex(), source.GetAllBoneWeights());
            target.bindposes = source.bindposes;
            target.subMeshCount = source.subMeshCount;
            for (int m = 0; m < target.subMeshCount; m++)
            {
                target.SetTriangles(source.GetTriangles(m), m);
            }

            // Copy Blend Shape
            target.ClearBlendShapes();
            for (int s = 0; s < source.blendShapeCount; s++)
            {
                int frameCount = source.GetBlendShapeFrameCount(s);
                for (int f = 0; f < frameCount; f++)
                {
                    string name = source.GetBlendShapeName(s);
                    var weight = source.GetBlendShapeFrameWeight(s, f);

                    var deltaVertices = new Vector3[source.vertexCount];
                    var deltaNormals = new Vector3[source.vertexCount];
                    var deltaTangents = new Vector3[source.vertexCount];
                    source.GetBlendShapeFrameVertices(s, f, deltaVertices, deltaNormals, deltaTangents);
                    target.AddBlendShapeFrame(name, weight, deltaVertices, deltaNormals, deltaTangents);
                }
            }

            target.RecalculateBounds();
        }

        void initNewMesh()
        {
            var currentMesh = GetSharedMeshFromComponent();
            var currentPath = AssetDatabase.GetAssetPath(currentMesh);

            // NEW Mesh
            // If the current mesh already is an edited mesh then load it and use it as the NewMesh.
            if (currentPath.EndsWith(FILE_EXTENSION))
            {
                NewMesh = AssetDatabase.LoadAssetAtPath<Mesh>(currentPath);
                _newMeshExistsAsAsset = true;
            }
            else
            {
                // Is there already an edited mesh file?
                string filePath = GetNewMeshFilePath(currentMesh, null, makeRelative: true);
                NewMesh = null;
                if (filePath != null)
                {
                    NewMesh = AssetDatabase.LoadAssetAtPath<Mesh>(filePath);
                }
                // No? Then create a new one.
                if (NewMesh == null)
                {
                    NewMesh = new Mesh();
                    _newMeshExistsAsAsset = false;
                }
                else
                {
                    _newMeshExistsAsAsset = true;
                }
            }

            copyMesh(_originalMeshCopy, NewMesh);
        }

        public bool HasMesh()
        {
            return NewMesh != null;
        }

        public bool IsUsingEditedMesh()
        {
            var mesh = GetSharedMeshFromComponent();
            if (mesh == null)
                return false;

            if (string.IsNullOrEmpty(mesh.name))
                return false;

            return mesh.name.EndsWith(FILE_ENDING);
        }

        public Material[] GetSharedMaterialsFromComponent()
        {
            if (IsSkinned)
            {
                if (SkinnedMeshRenderer != null)
                {
                    return SkinnedMeshRenderer.sharedMaterials;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (MeshRenderer != null)
                {
                    return MeshRenderer.sharedMaterials;
                }
                else
                {
                    return null;
                }
            }
        }

        public Mesh GetSharedMeshFromComponent()
        {
            if (IsSkinned)
            {
                if (SkinnedMeshRenderer != null)
                {
                    return SkinnedMeshRenderer.sharedMesh;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (MeshFilter != null)
                {
                    return MeshFilter.sharedMesh;
                }
                else
                {
                    return null;
                }
            }
        }

        public void ResetAll()
        {
            if (_originalMesh == null)
            {
                return;
            }

            var currentMesh = GetSharedMeshFromComponent();
            EditorGUIUtility.PingObject(currentMesh);
            string currentPath = AssetDatabase.GetAssetPath(currentMesh);

            var origPath = AssetDatabase.GetAssetPath(_originalMesh);
            // If the original mesh already was an edited mesh then we will have to use the copied mesh.
            if (origPath.EndsWith(FILE_EXTENSION))
            {
                copyMesh(_originalMeshCopy, GetSharedMeshFromComponent());
                applyMeshAndMaterials(GetSharedMeshFromComponent(), _originalMaterials);
            }
            // Otherwise we can simply reset to the original mesh
            else
            {
                applyMeshAndMaterials(_originalMesh, _originalMaterials);

                // Delete
                if (DeleteNewMeshAfterResetToOriginal && !string.IsNullOrEmpty(currentPath) && currentPath.EndsWith(FILE_EXTENSION) && currentPath.StartsWith("Assets"))
                {
                    AssetDatabase.DeleteAsset(currentPath);
                    EditorGUIUtility.PingObject(_originalMesh);
                }
            }
        }

        public void ResetToOriginal()
        {
            if (_originalMesh == null)
            {
                Logger.LogWarning("We have no original mesh");
                return;
            }

            var currentMesh = GetSharedMeshFromComponent();
            EditorGUIUtility.PingObject(currentMesh);

            // Try to find the original asset first. If that fails then revert to ResetAll().
            var currentPath = AssetDatabase.GetAssetPath(currentMesh);
            if (!string.IsNullOrEmpty(currentPath))
            {
                if (IsNameOfEditedMesh(currentPath))
                {
                    var originalMesh = LoadOriginalFromNewMeshPath(currentPath);
                    if (originalMesh != null)
                    {
                        applyMeshAndMaterials(originalMesh, _originalMaterials);

                        // Delete
                        if (DeleteNewMeshAfterResetToOriginal && !string.IsNullOrEmpty(currentPath) && currentPath.EndsWith(FILE_EXTENSION) && currentPath.StartsWith("Assets"))
                        {
                            AssetDatabase.DeleteAsset(currentPath);
                            EditorGUIUtility.PingObject(_originalMesh);
                        }
                    }
                    else
                    {
                        ResetAll();
                    }
                }
            }
            else
            {
                ResetAll();
            }
        }

        protected List<SelectedTriangle> _tmpTris = new List<SelectedTriangle>();
        protected List<List<int>> _tmpNewTriangles = new List<List<int>>();
        protected List<Material> _tmpNewMaterials = new List<Material>();

        public void AssignMaterial(HashSet<SelectedTriangle> selectedTriangles, Material material,
            bool mergeSameMaterialMesh = true, bool applyChangesToSelectedObject = true)
        {
            if (GetSharedMeshFromComponent() == null)
                return;

            if (NewMesh == null)
            {
                initNewMesh();
            }

            var validTris = getSelectedTrisMatchingThisModifier(selectedTriangles);

            _tmpNewTriangles.Clear();
            _tmpNewMaterials.Clear();
            var sharedMesh = GetSharedMeshFromComponent();
            var sharedMaterials = GetSharedMaterialsFromComponent();
            int subMeshCount = sharedMesh.subMeshCount;

            // Init lists of current sub meshes and materials
            for (int m = 0; m < subMeshCount; m++)
            {
                _tmpNewTriangles.Add(new List<int>());
                
                if (sharedMaterials.Length > m)
                {
                    _tmpNewMaterials.Add(sharedMaterials[m]);
                }
                else
                {
                    _tmpNewMaterials.Add(null);
                }
            }

            // Add a single list for the new sub mesh.
            _tmpNewTriangles.Add(new List<int>());
            int newSubMeshIndex = _tmpNewTriangles.Count - 1;

            // Add material for the new sub mesh
            _tmpNewMaterials.Add(material);

            // flag material as duplicate
            bool containsMaterial = material != null && _tmpNewMaterials.Contains(material);

            // Fill lists with triangles according to selection 
            List<int> tmpTris = new List<int>();
            for (int m = 0; m < subMeshCount; m++)
            {
                tmpTris.Clear();
                sharedMesh.GetTriangles(tmpTris, m);

                int triCount = tmpTris.Count;
                for (int v = 0; v < triCount; v += 3)
                {
                    int v0 = tmpTris[v];
                    int v1 = tmpTris[v + 1];
                    int v2 = tmpTris[v + 2];
                    bool isPartOfSelection = false;
                    foreach (var selectedTri in validTris)
                    {
                        if (
                               (v0 == selectedTri.TriangleIndices[0] && v1 == selectedTri.TriangleIndices[1] && v2 == selectedTri.TriangleIndices[2])
                            || (v0 == selectedTri.TriangleIndices[2] && v1 == selectedTri.TriangleIndices[1] && v2 == selectedTri.TriangleIndices[0])
                            )
                        {
                            isPartOfSelection = true;
                            break;
                        }
                    }
                    if (isPartOfSelection)
                    {
                        _tmpNewTriangles[newSubMeshIndex].Add(v0);
                        _tmpNewTriangles[newSubMeshIndex].Add(v1);
                        _tmpNewTriangles[newSubMeshIndex].Add(v2);
                    }
                    else
                    {
                        _tmpNewTriangles[m].Add(v0);
                        _tmpNewTriangles[m].Add(v1);
                        _tmpNewTriangles[m].Add(v2);
                    }
                }
            }

            LastAssignedSubMeshIndex = newSubMeshIndex;
            LastAssignedMaterial = material;

            // Merge new sub mesh with existing sub mesh if they share the same material
            if (mergeSameMaterialMesh && containsMaterial)
            {
                var newSubMeshTris = _tmpNewTriangles[newSubMeshIndex];
                // Append new tris to the first sub mesh matching the material.
                for (int m = 0; m < subMeshCount; m++)
                {
                    if (_tmpNewMaterials[m] == material)
                    {
                        // Append tris to existing sub mesh with the same material
                        _tmpNewTriangles[m].AddRange(newSubMeshTris);
                        // Delete new sub mesh (not needed anymore)
                        _tmpNewTriangles.RemoveAt(newSubMeshIndex);
                        _tmpNewMaterials.RemoveAt(newSubMeshIndex);
                        // update last sub mesh index
                        LastAssignedSubMeshIndex = m;
                        break;
                    }
                }
            }

            // Clear empty sub meshes
            for (int m = _tmpNewTriangles.Count-1; m >= 0; m--)
            {
                if (_tmpNewTriangles[m].Count == 0)
                {
                    _tmpNewTriangles.RemoveAt(m);
                    _tmpNewMaterials.RemoveAt(m);
                    // update last sub mesh index
                    if (m <= LastAssignedSubMeshIndex)
                    {
                        LastAssignedSubMeshIndex--;
                    }
                }
            }

            // Apply tris to mesh
            subMeshCount = _tmpNewTriangles.Count;
            NewMesh.subMeshCount = subMeshCount;
            for (int m = 0; m < subMeshCount; m++)
            {
                NewMesh.SetTriangles(_tmpNewTriangles[m].ToArray(), m);
            }

            // If the NewMesh does not yet exist as an asset then create it.
            if (!_newMeshExistsAsAsset)
            {
                var originalMesh = GetSharedMeshFromComponent();
                var origPath = AssetDatabase.GetAssetPath(originalMesh);
                if (!origPath.EndsWith(FILE_EXTENSION))
                {
                    // Create asset path
                    string filePath = GetNewMeshFilePath(originalMesh, NewMesh, makeRelative: false);

                    // Export
                    NewMesh.name = System.IO.Path.GetFileNameWithoutExtension(filePath); // not really necessary, names is set automatically by AssetDatabase
                    AssetExporter.SaveMeshAsAsset(NewMesh, filePath, logFilePaths: false);

                    _newMeshExistsAsAsset = true;
                }
            }

            EditorGUIUtility.PingObject(NewMesh);

            if (applyChangesToSelectedObject)
            {
                applyMeshAndMaterials(NewMesh, _tmpNewMaterials.ToArray());
            }
        }

        public void AssignMaterialToIndex(Material material, int subMeshIndex)
        {
            if (material == null)
                return;

            if (IsSkinned)
            {
                if (SkinnedMeshRenderer != null && SkinnedMeshRenderer.sharedMaterials.Length > subMeshIndex)
                {
                    var materials = SkinnedMeshRenderer.sharedMaterials;
                    materials[subMeshIndex] = material;
                    SkinnedMeshRenderer.sharedMaterials = materials;
                }
            }
            else
            {
                if (MeshRenderer != null && MeshRenderer.sharedMaterials.Length > subMeshIndex)
                {
                    var materials = MeshRenderer.sharedMaterials;
                    materials[subMeshIndex] = material;
                    MeshRenderer.sharedMaterials = materials;
                }
            }

            LastAssignedMaterial = material;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="originalMesh"></param>
        /// <param name="newMesh"></param>
        /// <param name="makeRelative">If true then the path will be made relative to Assets/. If false then the path will remain as is (no change).</param>
        /// <returns></returns>
        public static string GetNewMeshFilePath(Mesh originalMesh, Mesh newMesh, bool makeRelative = false)
        {
            string absolutePath = null;

            string originalMeshPath = AssetDatabase.GetAssetPath(originalMesh);
            if (string.IsNullOrEmpty(originalMeshPath))
            {
                if(newMesh != null)
                {
                    originalMeshPath = "Assets/Mesh" + (newMesh.GetInstanceID() + newMesh.vertexCount).ToString();
                }
                else
                {
                    originalMeshPath = "Assets/Mesh" + Random.Range(1000, 99800).ToString();
                }

                absolutePath = System.IO.Path.GetDirectoryName(originalMeshPath) + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileName(originalMeshPath) + "." + FILE_EXTENSION;
            }
            else
            {
                var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(originalMeshPath);
                if(assets == null || assets.Length == 0)
                {
                    absolutePath = System.IO.Path.GetDirectoryName(originalMeshPath) + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileName(originalMeshPath) + "." + FILE_EXTENSION;
                }
                else
                {
                    // The asset file contains more than one asset. Therefore we need to find the our original mesh in there.
                    var originalMeshAsObject = originalMesh as Object;
                    foreach (var asset in assets)
                    {
                        if (asset == originalMeshAsObject)
                        {
                            string nameForPath = NameToValidPath(asset.name);
                            absolutePath = System.IO.Path.GetDirectoryName(originalMeshPath) + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileName(originalMeshPath) + "." + nameForPath + FILE_EXTENSION;
                            break;
                        }
                    }
                }
            }
            
            if(makeRelative)
            {
                string relativePath = makePathRelativeToAssets(absolutePath);
                return relativePath;
            }
            else
            {
                return absolutePath;
            }
        }

        public static string NameToValidPath(string name)
        {
            string pathName = Regex.Replace(name, "[^a-zA-Z0-9_-]", "");
            if (string.IsNullOrEmpty(pathName))
            {
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(name);
                return System.Convert.ToBase64String(plainTextBytes);
            }
            else
            {
                return pathName;
            }
        }

        public static Mesh LoadOriginalFromNewMeshPath(string path)
        {
            path = makePathRelativeToAssets(path);

            // Extract original path and asset name from path and then load the mesh.
            var pathWithoutExtension = path;
            if (path.EndsWith(FILE_EXTENSION))
            {
                pathWithoutExtension = path.Replace(FILE_EXTENSION, "");
            }
            if (path.EndsWith(FILE_ENDING))
            {
                pathWithoutExtension = path.Replace(FILE_ENDING, "");
            }
            var pathParts = pathWithoutExtension.Split('.');
            var name = pathParts[pathParts.Length - 1];

            var originalAssetPath = pathWithoutExtension;
            if (string.IsNullOrEmpty(name))
            {
                // Remove trailing dot
                originalAssetPath = originalAssetPath.Substring(0, originalAssetPath.Length-1);

                return AssetDatabase.LoadAssetAtPath<Mesh>(originalAssetPath);
            }
            else
            {
                // Remove dot and name
                originalAssetPath = pathWithoutExtension.Replace("." + name, "");

                var assets = AssetDatabase.LoadAllAssetsAtPath(originalAssetPath);
                foreach (var asset in assets)
                {
                    var assetName = NameToValidPath(asset.name);
                    if (assetName == name && asset is Mesh)
                    {
                        return asset as Mesh;
                    }
                }
            }

            return null;
        }

        static string makePathRelativeToAssets(string path)
        {
            // Make path relative
            var relativePath = path;
            relativePath = relativePath.Replace("\\", "/");
            var dataPath = Application.dataPath.Replace("\\", "/");
            relativePath = relativePath.Replace(dataPath, "");

            // Ensure the path starts with "Assets/".
            if (!relativePath.StartsWith("Assets"))
            {
                if (relativePath.StartsWith("/"))
                {
                    relativePath = "Assets" + relativePath;
                }
                else
                {
                    relativePath = "Assets/" + relativePath;
                }
            }

            return relativePath;
        }

        void applyMeshAndMaterials(Mesh mesh, Material[] materials)
        {
            // Assign mesh and materials
            if (IsSkinned)
            {
                if (SkinnedMeshRenderer != null)
                {
                    SkinnedMeshRenderer.sharedMaterials = materials;

                    SkinnedMeshRenderer.sharedMesh = mesh;
                    SkinnedMeshRenderer.sharedMesh.MarkModified();

                    EditorUtility.SetDirty(SkinnedMeshRenderer);
                    EditorUtility.SetDirty(SkinnedMeshRenderer.gameObject);
                }
            }
            else
            {
                if (MeshRenderer != null && MeshFilter != null)
                {
                    MeshRenderer.sharedMaterials = materials;

                    MeshFilter.sharedMesh = mesh;
                    MeshFilter.sharedMesh.MarkModified();

                    EditorUtility.SetDirty(MeshRenderer);
                    EditorUtility.SetDirty(MeshFilter);
                    EditorUtility.SetDirty(MeshRenderer.gameObject);
                }
            }
        }

        public void ResetSelected(HashSet<SelectedTriangle> selectedTriangles)
        {
            if (GetSharedMeshFromComponent() == null)
                return;

            if (NewMesh == null)
            {
                initNewMesh();
            }

            var validTris = getSelectedTrisMatchingThisModifier(selectedTriangles);

            var originalMesh = _originalMesh ?? _originalMeshCopy;
            var originalMaterials = _originalMaterials;
            var originalTris = new List<List<int>>();
            for (int m = 0; m < originalMesh.subMeshCount; m++)
            {
                var tris = new List<int>();
                originalMesh.GetTriangles(tris, m);
                originalTris.Add(tris);
            }

            _tmpNewTriangles.Clear();
            _tmpNewMaterials.Clear();

            var sharedMesh = GetSharedMeshFromComponent();
            var sharedMaterials = GetSharedMaterialsFromComponent();
            int subMeshCount = sharedMesh.subMeshCount;

            var newSubMeshes = new Dictionary<Material,List<int>>();

            // Fill lists with triangles according to selection 
            List<int> tmpTris = new List<int>();
            for (int m = 0; m < subMeshCount; m++)
            {
                _tmpNewTriangles.Add(new List<int>());

                if (sharedMaterials.Length > m)
                {
                    _tmpNewMaterials.Add(sharedMaterials[m]);
                }
                else
                {
                    _tmpNewMaterials.Add(null);
                }

                tmpTris.Clear();
                sharedMesh.GetTriangles(tmpTris, m);

                int triCount = tmpTris.Count;
                for (int v = 0; v < triCount; v += 3)
                {
                    int v0 = tmpTris[v];
                    int v1 = tmpTris[v + 1];
                    int v2 = tmpTris[v + 2];
                    bool isPartOfSelection = false;
                    foreach (var selectedTri in validTris)
                    {
                        if (
                               (v0 == selectedTri.TriangleIndices[0] && v1 == selectedTri.TriangleIndices[1] && v2 == selectedTri.TriangleIndices[2])
                            || (v0 == selectedTri.TriangleIndices[2] && v1 == selectedTri.TriangleIndices[1] && v2 == selectedTri.TriangleIndices[0])
                            )
                        {
                            isPartOfSelection = true;
                            break;
                        }
                    }
                    if (isPartOfSelection)
                    {
                        // find that tri + subMesh index in the original mesh
                        for (int sm = 0; sm < originalTris.Count; sm++)
                        {
                            int tris = originalTris[sm].Count;
                            for (int ov = 0; ov < tris; ov += 3)
                            {
                                int ov0 = originalTris[sm][ov];
                                int ov1 = originalTris[sm][ov + 1];
                                int ov2 = originalTris[sm][ov + 2];
                                if (
                                       (ov0 == v0 && ov1 == v1 && ov2 == v2)
                                    || (ov0 == v2 && ov1 == v1 && ov2 == v0)
                                    )
                                {
                                    var material = originalMaterials[sm];
                                    if (!newSubMeshes.ContainsKey(material))
                                    {
                                        newSubMeshes.Add(material, new List<int>());
                                    }
                                    newSubMeshes[material].Add(v0);
                                    newSubMeshes[material].Add(v1);
                                    newSubMeshes[material].Add(v2);
                                }
                            }
                        }
                    }
                    else
                    {
                        _tmpNewTriangles[m].Add(v0);
                        _tmpNewTriangles[m].Add(v1);
                        _tmpNewTriangles[m].Add(v2);
                    }
                }
            }

            foreach (var kv in newSubMeshes)
            {
                _tmpNewMaterials.Add(kv.Key);
                _tmpNewTriangles.Add(kv.Value);
            }

            // Merge sub meshes if they share the same material
            for (int m = _tmpNewTriangles.Count-1; m >= 0; m--)
            {
                // find first occurence of the material
                for (int i = 0; i < _tmpNewMaterials.Count; i++)
                {
                    if (_tmpNewMaterials[i] == _tmpNewMaterials[m] && i != m)
                    {
                        // Append new tris to the first sub mesh matching the material.
                        _tmpNewTriangles[i].AddRange(_tmpNewTriangles[m]);
                        _tmpNewTriangles.RemoveAt(m);
                        _tmpNewMaterials.RemoveAt(m);
                        break;
                    }
                }
            }

            // Clear empty sub meshes
            for (int m = _tmpNewTriangles.Count - 1; m >= 0; m--)
            {
                if (_tmpNewTriangles[m].Count == 0)
                {
                    _tmpNewTriangles.RemoveAt(m);
                    _tmpNewMaterials.RemoveAt(m);
                }
            }

            // Apply tris to mesh
            subMeshCount = _tmpNewTriangles.Count;
            NewMesh.subMeshCount = subMeshCount;
            for (int m = 0; m < subMeshCount; m++)
            {
                NewMesh.SetTriangles(_tmpNewTriangles[m].ToArray(), m);
            }

            // If the NewMesh does not yet exist as an asset then create it.
            if (!_newMeshExistsAsAsset)
            {
                var currentMesh = GetSharedMeshFromComponent();
                var currentPath = AssetDatabase.GetAssetPath(currentMesh);
                if (!currentPath.EndsWith(FILE_EXTENSION))
                {
                    string filePath = GetNewMeshFilePath(currentMesh, NewMesh, makeRelative: false);
                    AssetExporter.SaveMeshAsAsset(NewMesh, filePath, logFilePaths: false);

                    _newMeshExistsAsAsset = true;

                    EditorGUIUtility.PingObject(NewMesh);
                }
            }

            applyMeshAndMaterials(NewMesh, _tmpNewMaterials.ToArray());
        }

        List<SelectedTriangle> getSelectedTrisMatchingThisModifier(HashSet<SelectedTriangle> selectedTriangles)
        {
            _tmpTris.Clear();
            foreach (var tri in selectedTriangles)
            {
                if (tri.Component == null)
                    continue;

                if (tri.Component == MeshFilter || tri.Component == SkinnedMeshRenderer || tri.Component == MeshRenderer)
                {
                    _tmpTris.Add(tri);
                }
            }

            return _tmpTris;
        }

        bool fileExists(string filePathInsideAssetsDir)
        {
            string absoluteFilePath = Application.dataPath + System.IO.Path.DirectorySeparatorChar + filePathInsideAssetsDir;
            return System.IO.File.Exists(absoluteFilePath);
        }

        void deleteFile(string filePathInsideAssetsDir)
        {
            string absoluteFilePath = Application.dataPath + System.IO.Path.DirectorySeparatorChar + filePathInsideAssetsDir;
            
            if (System.IO.File.Exists(absoluteFilePath))
            {
                System.IO.File.Delete(absoluteFilePath);
            }

            if (System.IO.File.Exists(absoluteFilePath + ".meta"))
            {
                System.IO.File.Delete(absoluteFilePath + ".meta");
            }
        }

        static T getOrCreateMeshData<T>(Mesh mesh, Dictionary<Mesh, T> data) where T : new()
        {
            if (!data.ContainsKey(mesh))
            {
                data.Add(mesh, new T());
            }
            return data[mesh];
        }
    }
}
