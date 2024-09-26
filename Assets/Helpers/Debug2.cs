using UnityEngine;

namespace Assets.Helpers
{
    public static class Debug2
    {
        public static void DrawHDWireCircle(Vector3 center, float radius, int baseVertexAmount = 20)
        {
            Mesh mesh = new Mesh();
            baseVertexAmount = (int)(baseVertexAmount * radius);
            Vector3[] vertices = new Vector3[baseVertexAmount];
            Vector3[] normals = new Vector3[baseVertexAmount];
            for (int i = 0; i < baseVertexAmount; i++)
            {
                float rotation = Helper.Remap(i, 0, baseVertexAmount - 2, 0, Mathf.PI * 2f, false);
                Vector3 offset = rotation.PolarVector(radius);
                vertices[i] = offset + center;
                normals[i] = new Vector3(offset.y, offset.x, 0);

            }
            mesh.vertices = vertices;
            mesh.normals = normals;
            baseVertexAmount--;
            baseVertexAmount *= 2;
            int[] indices = new int[baseVertexAmount];
            for (int i = 0; i < baseVertexAmount; i++)
            {
                indices[i] = i / 2;
                indices[i] = i / 2 + 1;
            }
            mesh.SetIndices(indices, MeshTopology.LineStrip, 0);
            Gizmos.DrawWireMesh(mesh);
        }
    }
}
