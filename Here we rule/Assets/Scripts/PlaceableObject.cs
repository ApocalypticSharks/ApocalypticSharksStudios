using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

public class PlaceableObject : MonoBehaviour
{
    public bool Placed { get; private set; }
    public Vector3Int Size { get; private set; }
    private Vector3[] Vertices;
    private void GetColliderVertexPositionLocal()
    { 
        BoxCollider collider = GetComponent<BoxCollider>();
        Vertices = new Vector3[4];
        Vertices[0] = collider.center + new Vector3(-collider.size.x, -collider.size.y, -collider.size.z) * .5f;
        Vertices[1] = collider.center + new Vector3(collider.size.x, -collider.size.y, -collider.size.z) * .5f;
        Vertices[2] = collider.center + new Vector3(collider.size.x, -collider.size.y, collider.size.z) * .5f;
        Vertices[3] = collider.center + new Vector3(-collider.size.x, -collider.size.y, collider.size.z) * .5f;
    }

    private void CalculateSizeInCells()
    {
        Vector3Int[] verticies = new Vector3Int[Vertices.Length];

        for (int i = 0; i < verticies.Length; i++)
        {
            Vector3 worldPosition = transform.TransformPoint(Vertices[i]);
            verticies[i] = BuildingSystem.instance.gridLayout.WorldToCell(worldPosition);
        }

        Size = new Vector3Int(Mathf.Abs((verticies[0] - verticies[1]).x), Mathf.Abs((verticies[0] - verticies[3]).y), 1);
    }

    public Vector3 GetStartPosition()
    {
        return transform.TransformPoint(Vertices[0]);
    }

    private void Start()
    {
        GetColliderVertexPositionLocal();
        CalculateSizeInCells();
    }

    public void Rotate()
    {
        transform.Rotate(new Vector3(0, 90,0));
        Size = new Vector3Int(Size.y, Size.x, 1);

        Vector3[] vertices = new Vector3[Vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = Vertices[(i+1) % Vertices.Length];
        }

        Vertices = vertices;
    }

    public virtual void Place()
    { 
        ObjectDrag drag = gameObject.GetComponent<ObjectDrag>();
        Destroy(drag);
        Placed = true;
        //Invoke events of placements
    }
}
