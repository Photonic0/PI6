using UnityEngine;

namespace Assets.Helpers
{
    public static class Gizmos2
    {
        public static void DrawHDWireCircle(Vector3 center, float radius, int baseVertexAmount = 20)
        {
            Mesh mesh = new();
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

        public static void DrawRotatedRectangle(Vector2 center, Vector2 size, float angle, Color? color = null)
        {
            float radians = angle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            Vector2 halfSize = size * 0.5f;
            Vector2[] corners = new Vector2[4];
            corners[0] = new Vector2(-halfSize.x, -halfSize.y); // Bottom-left
            corners[1] = new Vector2(halfSize.x, -halfSize.y);  // Bottom-right
            corners[2] = new Vector2(halfSize.x, halfSize.y);   // Top-right
            corners[3] = new Vector2(-halfSize.x, halfSize.y);  // Top-left
                                                                // Rotate each corner manually using 2D rotation formula
            for (int i = 0; i < corners.Length; i++)
            {
                float x = corners[i].x;
                float y = corners[i].y;
                // Apply the 2D rotation matrix:
                // x' = x * cos(angle) - y * sin(angle)
                // y' = x * sin(angle) + y * cos(angle)
                float rotatedX = x * cos - y * sin;
                float rotatedY = x * sin + y * cos;
                // Translate the rotated corner back to the center position
                corners[i] = new Vector2(rotatedX, rotatedY) + center;
            }

            // Draw the rectangle using Gizmos
            Color prevColor = default;
            if (color != null)
            {
                prevColor = Gizmos.color;
                Gizmos.color = color.Value;
            }
            Gizmos.DrawLine(corners[0], corners[1]); // Bottom edge
            Gizmos.DrawLine(corners[1], corners[2]); // Right edge
            Gizmos.DrawLine(corners[2], corners[3]); // Top edge
            Gizmos.DrawLine(corners[3], corners[0]); // Left edge
            if (color != null)
            {
                Gizmos.color = prevColor;
            }
        }
        public static void DrawSpheres(Vector2[] positions, float radius)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                Gizmos.DrawSphere(positions[i], radius);
            }
        }
        public static void DrawSpheres(Vector3[] positions, float radius)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                Gizmos.DrawSphere(positions[i], radius);
            }
        }
    }
}
