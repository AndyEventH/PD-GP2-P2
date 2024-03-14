using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RoomGenerator : EditorWindow
{
    [MenuItem("Generator/RoomSpawner")]
    public static void OpenGrid() => GetWindow<RoomGenerator>("Spawn room");

    public Transform room;

    SerializedObject so;
    GameObject[] prefabs;
    bool[] selectedPrefabs;
    int selTog = -1;
    int moves = 0;

    private void OnEnable()
    {
        so = new SerializedObject(this);

        string[] guids = AssetDatabase.FindAssets("t:prefab", new[] { "Assets/Rooms" });
        IEnumerable<string> paths = guids.Select(AssetDatabase.GUIDToAssetPath);
        prefabs = paths.Select(AssetDatabase.LoadAssetAtPath<GameObject>).ToArray();

        if (selectedPrefabs == null || selectedPrefabs.Length != prefabs.Length)
        {
            selectedPrefabs = new bool[prefabs.Length];
        }

        Selection.selectionChanged += Repaint;
        SceneView.duringSceneGui += UpdateSceneGUI;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= Repaint;
        SceneView.duringSceneGui -= UpdateSceneGUI;
    }

    private void OnGUI()
    {
        so.Update();

        GUIStyle style = GUI.skin.GetStyle("Label");
        style.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("Room Spawner", style);
        GUILayout.Space(10);
        GUILayout.Label("Place an object with SPACE once selected", style);
        if (GUI.Button(new Rect(1, position.height - 20, position.width - 2, 20), "Undo") && moves > 0)
        {
            moves--;
            Undo.PerformUndo();
        }

        so.ApplyModifiedProperties();
    }

    private Quaternion currentRotation = Quaternion.identity;


    private void UpdateSceneGUI(SceneView sceneView)
    {
        Handles.BeginGUI();
        Event guiEvent = Event.current;
        Rect prefabToggleRect = new Rect(10, 10, 200, 50);

        for (int prefabIndex = 0; prefabIndex < prefabs.Length; prefabIndex++)
        {
            GameObject currentPrefab = prefabs[prefabIndex];
            Texture currentPrefabIcon = AssetPreview.GetAssetPreview(currentPrefab);
            EditorGUI.BeginChangeCheck();

            selectedPrefabs[prefabIndex] = GUI.Toggle(prefabToggleRect, selectedPrefabs[prefabIndex], new GUIContent(currentPrefab.name, currentPrefabIcon));

            if(EditorGUI.EndChangeCheck())
{
                room = null;
                selTog = -1; 
                for (int selectionIndex = 0; selectionIndex < selectedPrefabs.Length; selectionIndex++)
                {
                    if (selectedPrefabs[selectionIndex])
                    {
                        if (prefabIndex != selTog)
                        {
                            selectedPrefabs[selectionIndex] = true;
                            if (selTog != -1) selectedPrefabs[selTog] = false;
                            selTog = selectionIndex;
                            room = prefabs[selectionIndex].transform;
                        }
                    }
                }
                SceneView.RepaintAll(); 
            }


            prefabToggleRect.y += prefabToggleRect.height + 2;
        }

        Handles.EndGUI();
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

        if (guiEvent.type == EventType.MouseMove)
        {
            sceneView.Repaint();
        }

        Ray rayFromMouse = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);

        if (Physics.Raycast(rayFromMouse, out RaycastHit hitInfo))
        {
            Vector3 spawnPosition = Vector3.zero;
            if (room != null)
            {
                if (guiEvent.type == EventType.KeyUp && guiEvent.keyCode == KeyCode.R)
                {
                    currentRotation *= Quaternion.Euler(0, 90f, 0);
                    sceneView.Repaint();
                }

                spawnPosition = hitInfo.point + Vector3.up;
                PreviewPrefab(spawnPosition, currentRotation);

                if (guiEvent.type == EventType.KeyUp && guiEvent.keyCode == KeyCode.Space)
                {
                    SpawnPrefab(spawnPosition, room);
                    sceneView.Repaint();
                }
            }

            Vector3 tangentVector = Vector3.Cross(hitInfo.normal, sceneView.camera.transform.up).normalized;
            Vector3 bitangentVector = Vector3.Cross(hitInfo.normal, tangentVector);
            DrawInteractionLines(hitInfo.point, tangentVector, bitangentVector, hitInfo.normal);
        }
    }

    private void DrawInteractionLines(Vector3 originPoint, Vector3 tangentDir, Vector3 bitangentDir, Vector3 normalDir)
    {
        Handles.color = Color.red;
        Handles.DrawAAPolyLine(6, originPoint, originPoint + tangentDir);
        Handles.color = Color.green;
        Handles.DrawAAPolyLine(6, originPoint, originPoint + bitangentDir);
        Handles.color = Color.blue;
        Handles.DrawAAPolyLine(6, originPoint, originPoint + normalDir);
        Handles.color = Color.white;
    }

    void PreviewPrefab(Vector3 position, Quaternion rotation)
    {
        Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, Vector3.one);

        MeshFilter[] meshFilters = room.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter meshFilter in meshFilters)
        {
            Mesh mesh = meshFilter.sharedMesh;
            Material material = meshFilter.GetComponent<Renderer>().sharedMaterial;
            material.SetPass(0);
            Graphics.DrawMeshNow(mesh, matrix * meshFilter.transform.localToWorldMatrix);
        }
    }


    void SpawnPrefab(Vector3 position, Transform prefab)
    {
        GameObject spawnedObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject);
        spawnedObject.transform.SetPositionAndRotation(position, currentRotation);
        Undo.RegisterCreatedObjectUndo(spawnedObject, "Spawn Prefab");
        moves++;

        spawnedObject.GetComponent<RoomHandler>().RoomCheck();
    }



}