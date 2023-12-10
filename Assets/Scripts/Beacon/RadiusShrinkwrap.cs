using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RadiusShrinkwrap : MonoBehaviour
{
    [SerializeField] LayerMask mask;
    [SerializeField] ParticleSystem pSystem;
    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        //https://forum.unity.com/threads/how-do-i-duplicate-a-mesh-asset.35639/
        Mesh newMesh = new Mesh
        {
            vertices = mesh.vertices,
            triangles = mesh.triangles,
            uv = mesh.uv,
            normals = mesh.normals,
            colors = mesh.colors,
            tangents = mesh.tangents
        };

        Vector3[] vertices = mesh.vertices;
        List<float> offsets = new();
        for(int i = 0; i < vertices.Length; i++)
        {
            //The local position of the vertex rotated and scaled to match world coordinates
            Vector3 transformedPos = transform.localToWorldMatrix * vertices[i];
            //The position of the vertex in world coordinates
            Vector3 globalVertPos = transformedPos + transform.position;

            if(Physics.Raycast(globalVertPos, Vector3.down, out RaycastHit hitInfo, 100f, mask))
            {
                //By how much we have to move the vertex in it's local space
                Vector3 offset = transform.worldToLocalMatrix * new Vector3(0, transformedPos.y - hitInfo.distance, 0);
                offsets.Add(transformedPos.y - hitInfo.distance);
                vertices[i] += offset;
            }   
        }

        newMesh.vertices = vertices;
        newMesh.RecalculateBounds();
        GetComponent<MeshFilter>().mesh = newMesh;

        //Cursed
        Vector3 newPos = pSystem.gameObject.transform.position;
        newPos.y += offsets.Average();
        pSystem.gameObject.transform.position = newPos;
    }
}
