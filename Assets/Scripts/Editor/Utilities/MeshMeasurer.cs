using UnityEngine;
using UnityEditor;

namespace Commar
{
    public class MeshMeasurer : EditorWindow
    {
        private GameObject _meshObject;

        private int _selectedTab = 0;
        private string[] _tabLabels = new string[] { "Mesh Size" };

        [MenuItem("Commar/Mesh Measurer")]
        public static void ShowWindow()
        {
            GetWindow<MeshMeasurer>("Mesh Measurer");
        }

        private void OnGUI()
        {
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabLabels);

            GUILayout.Space(10);
            _meshObject = (GameObject)EditorGUILayout.ObjectField("GameObjct containin mesh", _meshObject, typeof(GameObject), true);
            GUILayout.Space(15);

            switch (_selectedTab)
            {
                case 0:
                    LogMeshBounds();
                    break;
            }
        }

        private void LogMeshBounds()
        {
            if (!GUILayout.Button("Log Mesh Size"))
                return;

            if (!CheckMeshObject(out MeshFilter meshFilter))
                return;

            Bounds bounds = meshFilter.sharedMesh.bounds;
            Vector3 meshSize = bounds.size;
            meshSize.x *= _meshObject.transform.localScale.x;
            meshSize.y *= _meshObject.transform.localScale.y;
            meshSize.z *= _meshObject.transform.localScale.z;
            Debug.Log($"{_meshObject.name}'s size{meshSize}");
        }

        private bool CheckMeshObject(out MeshFilter meshFilter)
        {
            if (_meshObject == null)
            {
                Debug.LogError($"GameObject is not assigned!!!");
                meshFilter = null;
                return false;
            }

            meshFilter = _meshObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                Debug.LogError($"{_meshObject.name} does not have MeshFilter or a valid Mesh!!!");
                return false;
            }

            return true;
        }
    }
}