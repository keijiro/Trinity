using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Vacs
{
    [CustomEditor(typeof(VacsData))]
    public sealed class VacsDataEditor : Editor
    {
        #region Custom inspector

        public override void OnInspectorGUI()
        {
            var vcount = ((VacsData)target).vertexCount;
            EditorGUILayout.LabelField("Vertex Count", vcount.ToString("N0"));
        }

        #endregion

        #region Menu item functions

        static Object[] SelectedMeshes {
            get { return Selection.GetFiltered(typeof(Mesh), SelectionMode.Deep); }
        }

        [MenuItem("Assets/Vacs/Create From Mesh", true)]
        static bool ValidateCreateFromMesh()
        {
            return SelectedMeshes.Length > 0;
        }

        [MenuItem("Assets/Vacs/Create From Mesh")]
        static void CreateFromMesh()
        {
            var assets = new List<Object>();

            foreach (Mesh mesh in SelectedMeshes)
            {
                // Destination file path.
                var dirPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(mesh));
                var filename = (string.IsNullOrEmpty(mesh.name) ? "Vacs" : mesh.name + " Vacs") + ".asset";
                var assetPath = AssetDatabase.GenerateUniqueAssetPath(dirPath + "/" + filename);

                // Create a data asset.
                var asset = ScriptableObject.CreateInstance<VacsData>();
                asset.CreateFromMesh(mesh);
                AssetDatabase.CreateAsset(asset, assetPath);
                AssetDatabase.AddObjectToAsset(asset.templateMesh, asset);

                assets.Add(asset);
            }

            // Save the generated assets.
            AssetDatabase.SaveAssets();

            // Select the generated assets.
            EditorUtility.FocusProjectWindow();
            Selection.objects = assets.ToArray();
        }

        #endregion
    }
}
