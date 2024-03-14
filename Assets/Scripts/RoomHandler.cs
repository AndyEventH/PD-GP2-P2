using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct RoomWalls
{
    public bool hasDoor;
    public GameObject wallSide;
}

public class RoomHandler : MonoBehaviour
{
    public List<RoomWalls> walls;

    private void OnValidate()
    {
        UpdateWalls();
    }

    private void UpdateWalls()
    {
        if (walls == null)
        {
            walls = new List<RoomWalls>();
        }
        else
        {
            walls.Clear();
        }

        foreach (Transform child in transform)
        {
            bool hasDoor = false;

            if (child.childCount > 0)
            {
                hasDoor = true;
            }

            walls.Add(new RoomWalls { wallSide = child.gameObject, hasDoor = hasDoor });
        }
    }

    private LayerMask wallMask = 1 << 8;

    public void RoomCheck()
    {
        for (int i = 0; i < walls.Count; i++)
        {
            if (walls[i].hasDoor && CheckDoors(i))
            {
                return;
            }
        }
        DestroyImmediate(gameObject);
    }


    private void SetCollidersEnabled(GameObject obj, bool enabled)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = enabled;
        }
    }

    public float angleThreshold = 45.0f;
    bool CheckDoors(int ind)
    {
        Debug.DrawRay(walls[ind].wallSide.transform.position , walls[ind].wallSide.transform.forward * 0.4f, Color.cyan, 2);
        SetCollidersEnabled(gameObject, false);

        if (Physics.Raycast(walls[ind].wallSide.transform.position, walls[ind].wallSide.transform.forward, out RaycastHit hit, 0.4f, wallMask))
        {
            Vector3 wall = hit.transform.parent.position;
            Vector3 room = hit.transform.parent.transform.parent.position;
            Debug.Log($"Raycast from {transform.name} hit {hit.collider.gameObject.name} ({hit.transform.parent.name})");
            RoomHandler hitted = hit.transform.parent.transform.parent.gameObject.GetComponent<RoomHandler>();
            if (hitted != null)
            {
                float distance = Vector3.Distance(wall, room) * 2 + 0.1f;
                Vector3 direction = (wall - room).normalized;
                transform.position = room + (direction) * distance;
                float angle = Vector3.Angle(hit.transform.parent.transform.forward, hit.normal);

                SetCollidersEnabled(gameObject, true);
                if (Mathf.Abs(angle) < angleThreshold || Mathf.Abs(angle - 180) < angleThreshold)
                {
                    Debug.Log("Hit from front or back " + angle);
                    return true;
                }
                else
                {
                    Debug.Log("Ignored side hit " + angle);
                   
                        DestroyImmediate(gameObject);
                

                    
                    return false;
                }
            }
        }
        SetCollidersEnabled(gameObject, true);
        return false;
    }


}
