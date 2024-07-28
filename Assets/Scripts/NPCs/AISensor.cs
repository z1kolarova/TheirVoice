using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AISensor : MonoBehaviour
{
    const float MIN_DISTANCE = 4f;
    const float MAX_DISTANCE = 9f;
    public float distance;
    public float horizontalAngle = 30;
    public float height = 1.5f;
    public Color meshColor = Color.magenta;
    public int scanFrequency = 30;
    public LayerMask layers;
    public List<GameObject> objects = new List<GameObject>();

    Collider[] colliders = new Collider[50];
    Mesh mesh;
    int count;
    float scanInterval;
    float scanTimer;

    // Start is called before the first frame update
    void Start()
    {
        scanInterval = 1.0f / scanFrequency;
        distance = MIN_DISTANCE + (float)RngUtils.Rng.NextDouble() * (MAX_DISTANCE - MIN_DISTANCE);
    }

    // Update is called once per frame
    void Update()
    {
        scanTimer -= Time.deltaTime;
        if (scanTimer < 0)
        {
            scanTimer += scanInterval;
            Scan();
        }
    }

    private void Scan()
    {
        count = Physics.OverlapSphereNonAlloc(transform.position, distance, colliders, layers, QueryTriggerInteraction.Collide);
        objects.Clear();
        for (int i = 0; i < count; i++)
        {
            GameObject obj = colliders[i].gameObject;
            if (IsInSight(obj))
            {
                objects.Add(obj);
            }
        }
    }

    public bool IsInSight(GameObject obj)
    {
        Vector3 origin = transform.position;
        Vector3 destination = obj.transform.position;
        Vector3 direction = destination - origin;

        if (direction.y < -height || direction.y > height)
        {
            return false;
        }
        
        direction.y = 0;
        float deltaAngle = Vector3.Angle(direction, transform.forward);
        if (deltaAngle > horizontalAngle)
        {
            return false;
        }
        return true;
    }

    Mesh CreateWedgeMesh()
    { 
        Mesh mesh = new Mesh();

        int segments = 10;
        // each segment has 2 triangles on far side, 1 on top and 1 on bottom
        // the whole mesh then has 2 triangles on the left side and 2 on the right side
        int numTriangles = (segments * 4) + 2 + 2;
        int numVertices = numTriangles * 3;

        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[numVertices];

        Vector3 bottomCenter = Vector3.down * height;
        Vector3 bottomLeft = bottomCenter + Quaternion.Euler(0, -horizontalAngle, 0) * Vector3.forward * distance;
        Vector3 bottomRight = bottomCenter + Quaternion.Euler(0, horizontalAngle, 0) * Vector3.forward * distance;

        Vector3 topCenter = Vector3.up * height;
        Vector3 topLeft = bottomLeft + Vector3.up * 2 * height;
        Vector3 topRight = bottomRight + Vector3.up * 2* height;

        int vert = 0;

        // left side
        vertices[vert++] = bottomCenter;
        vertices[vert++] = bottomLeft;
        vertices[vert++] = topLeft;

        vertices[vert++] = topLeft;
        vertices[vert++] = topCenter;
        vertices[vert++] = bottomCenter;

        // right side
        vertices[vert++] = bottomCenter;
        vertices[vert++] = topCenter;
        vertices[vert++] = topRight;

        vertices[vert++] = topRight;
        vertices[vert++] = bottomRight;
        vertices[vert++] = bottomCenter;

        float currentAngle = -horizontalAngle;
        float deltaAngle = (horizontalAngle * 2) / segments;

        for (int i = 0; i < segments; i++)
        {
            bottomLeft = bottomCenter + Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * distance;
            bottomRight = bottomCenter + Quaternion.Euler(0, currentAngle + deltaAngle, 0) * Vector3.forward * distance;

            topLeft = bottomLeft + Vector3.up * 2 * height;
            topRight = bottomRight + Vector3.up * 2 * height;

            // far side
            vertices[vert++] = bottomLeft;
            vertices[vert++] = bottomRight;
            vertices[vert++] = topRight;

            vertices[vert++] = topRight;
            vertices[vert++] = topLeft;
            vertices[vert++] = bottomLeft;

            // top
            vertices[vert++] = topCenter;
            vertices[vert++] = topLeft;
            vertices[vert++] = topRight;

            // bottom
            vertices[vert++] = bottomCenter;
            vertices[vert++] = bottomRight;
            vertices[vert++] = bottomLeft;

            currentAngle += deltaAngle;
        }

        for (int i = 0; i < numVertices; i++)
        {
            triangles[i] = i;
        }
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    private void OnValidate()
    {
        scanInterval = 1.0f / scanFrequency;
        mesh = CreateWedgeMesh();
    }

    //private void OnDrawGizmos()
    //{
    //    if (mesh)
    //    {
    //        Gizmos.color = meshColor;
    //        Gizmos.DrawMesh(mesh, transform.position, transform.rotation);
    //    }
    //    /*
    //    Gizmos.DrawWireSphere(transform.position, distance);
    //    for (int i = 0; i < count; i++)
    //    {
    //        Gizmos.DrawSphere(colliders[i].transform.position, 1f);
    //    }*/

    //    Gizmos.color = Color.yellow;
    //    foreach (var obj in objects)
    //    {
    //        Gizmos.DrawSphere(obj.transform.position, 3f);
    //    }
    //}
}
