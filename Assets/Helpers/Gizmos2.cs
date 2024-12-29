#if UNITY_EDITOR

using UnityEngine;

namespace Assets.Helpers
{

    public static class Gizmos2
    {

        /// <summary>
        /// BROKEN WHEN WIDTH > HEIGHT
        /// </summary>
        /// <param name="boxCollider"></param>
        /// <param name="lineSpacingMultiplier"></param>
        /// <param name="drawOutline"></param>
        public static void DrawLinesInBoxCollider(BoxCollider2D boxCollider, float lineSpacingMultiplier = 2, bool drawOutline = false)
        {
            // Get the bounds of the BoxCollider2D
            Bounds bounds = boxCollider.bounds;

            // Calculate the top-left and bottom-right corners from the bounds
            Vector2 topLeftCorner = new(bounds.min.x, bounds.max.y);
            Vector2 bottomRightCorner = new(bounds.max.x, bounds.min.y);
            Vector2 bottomLeft = new(topLeftCorner.x, bottomRightCorner.y);
            Vector2 topRight = new(bottomRightCorner.x, topLeftCorner.y);
            // Calculate the width and height of the rectangle
            float rectWidth = Mathf.Abs(topLeftCorner.x - bottomRightCorner.x);
            float rectHeight = Mathf.Abs(topLeftCorner.y - bottomRightCorner.y);
            float longerSide = Mathf.Max(rectWidth, rectHeight);
            float shorterSide = Mathf.Min(rectWidth, rectHeight);
            int numberOfLines = (int)((longerSide + shorterSide) * lineSpacingMultiplier);

            // Draw the outer rectangle (optional)
            if (drawOutline)
            {
                Gizmos.DrawLine(topLeftCorner, new Vector2(bottomRightCorner.x, topLeftCorner.y)); // Top side
                Gizmos.DrawLine(new Vector2(bottomRightCorner.x, topLeftCorner.y), bottomRightCorner); // Right side
                Gizmos.DrawLine(bottomRightCorner, new Vector2(topLeftCorner.x, bottomRightCorner.y)); // Bottom side
                Gizmos.DrawLine(new Vector2(topLeftCorner.x, bottomRightCorner.y), topLeftCorner); // Left side
            }

            // Calculate the spacing between each line
            float lineSpacing = (longerSide + shorterSide) / (numberOfLines + 1);

            // Draw parallel lines inside the rectangle

            for (int i = 1; i <= numberOfLines; i++)
            {

                float startX = topLeftCorner.x;
                float startY = topLeftCorner.y - (i * lineSpacing);

                float endX = startX + shorterSide;
                float endY = startY + shorterSide;

                float distOutOfBounds = Mathf.Max(0, bottomLeft.y - startY);
                startX += distOutOfBounds;
                startY = Mathf.Max(startY, bottomLeft.y);
                distOutOfBounds = Mathf.Min(0, topRight.y - endY);
                endX += distOutOfBounds;
                endY = Mathf.Min(endY, topRight.y);

                Gizmos.DrawLine(new Vector2(startX, startY), new Vector2(endX, endY));
            }

        }

        public static Mesh GetHDWireCircleMesh(Vector3 center, float radius, int baseVertexAmount = 20)
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
            return mesh;
        }
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
        public static void DrawQuad(Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight)
        {
            Mesh mesh = new();
            Vector3[] vertices = new Vector3[4] { topLeft, topRight, bottomLeft, bottomRight };
            Vector3[] normals = new Vector3[4] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            int[] indices = new int[] { 2, 1, 0, 1, 2, 3 };
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            Gizmos.DrawMesh(mesh);
        }
        public static void DrawRotatedRectangle(Vector2 center, Vector2 size, float degrees, Color? color = null)
        {
            float radians = degrees * Mathf.Deg2Rad;
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
        public static void DrawRectangle(float minX, float maxX, float minY, float maxY, Color? col = null)
        {
            Vector3 bottomLeft = new(minX, minY, 0);
            Vector3 bottomRight = new(maxX, minY, 0);
            Vector3 topRight = new(maxX, maxY, 0);
            Vector3 topLeft = new(minX, maxY, 0);
            Gizmos.color = col ?? Color.white;
            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);
        }
        public static void DrawSpheres(Vector2[] positions, float radius)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                Gizmos.DrawSphere(positions[i], radius);
            }
        }
        public static void DrawRectangle(Bounds bounds)
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            Vector3 bottomLeft = new(min.x, min.y, 0);
            Vector3 bottomRight = new(max.x, min.y, 0);
            Vector3 topLeft = new(min.x, max.y, 0);
            Vector3 topRight = new(max.x, max.y, 0);
            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);
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
#endif
