using System;
using UnityEngine;

namespace PxP.Draw
{
    /// <summary>
    /// Contains core geometric logic for drawing shapes, agnostic to whether it's
    /// a Debug.DrawLine or Gizmos.DrawLine call.
    /// </summary>
    public static class DrawingHelper
    {
        public const int DEFAULT_SEGMENTS = 64;
        public const int SIDE_LINES = 4; // Number of side lines for cylinder/capsule/cone

        /// <summary>
        /// Delegate for the line drawing function.
        /// </summary>
        /// <param name="start">The start point of the line segment.</param>
        /// <param name="end">The end point of the line segment.</param>
        public delegate void DrawLineDelegate(Vector3 start, Vector3 end);

        /// <summary>
        /// Calculates a robust orthonormal basis (axis1, axis2) for the plane
        /// perpendicular to the given normal vector.
        /// </summary>
        private static void GetOrthonormalBasis(Vector3 normal, out Vector3 axis1, out Vector3 axis2)
        {
            // Choose a helper vector not parallel to the normal
            Vector3 helper =
                Mathf.Abs(Vector3.Dot(normal, Vector3.up)) < 0.99f ?
                Vector3.up : Vector3.right;

            // Build an orthonormal basis
            axis1 = Vector3.Cross(normal, helper).normalized;
            axis2 = Vector3.Cross(normal, axis1);
        }

        #region --- 2D Primitives ---

        /// <summary>
        /// Draws a wireframe circle.
        /// </summary>
        public static void DrawCircle(Vector3 center, Vector3 normal, float radius, int segments, DrawLineDelegate drawLine)
        {
            normal = normal.normalized;
            GetOrthonormalBasis(normal, out Vector3 axis1, out Vector3 axis2);
            DrawArc(center, axis1, axis2, radius, 0f, 360f, segments, drawLine);
        }

        /// <summary>
        /// Draws a wireframe arc.
        /// </summary>
        public static void DrawArc(Vector3 center, Vector3 axis1, Vector3 axis2,
            float radius, float startAngleDeg, float endAngleDeg, int segments, DrawLineDelegate drawLine)
        {
            axis1 = axis1.normalized;
            axis2 = axis2.normalized;

            float startRad = startAngleDeg * Mathf.Deg2Rad;
            float endRad = endAngleDeg * Mathf.Deg2Rad;

            float angleRange = endRad - startRad;
            int steps = Mathf.Max(1, segments);

            Vector3 GetPoint(float angle)
            {
                return center + (Mathf.Cos(angle) * axis1 + Mathf.Sin(angle) * axis2) * radius;
            }

            Vector3 prevPoint = GetPoint(startRad);

            for (int i = 1; i <= steps; i++)
            {
                float t = (float)i / steps;
                float angle = startRad + angleRange * t;

                Vector3 nextPoint = GetPoint(angle);

                drawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }
        }

        /// <summary>
        /// Draws the wireframe of a rectangle defined by its center, rotation, and size.
        /// </summary>
        public static void DrawRectangle(Vector3 center, Quaternion rotation, Vector2 size, DrawLineDelegate drawLine)
        {
            Vector3 halfSize = new Vector3(size.x / 2f, size.y / 2f, 0f);

            Vector3 p1 = center + rotation * new Vector3(-halfSize.x, halfSize.y, 0f);
            Vector3 p2 = center + rotation * new Vector3(halfSize.x, halfSize.y, 0f);
            Vector3 p3 = center + rotation * new Vector3(halfSize.x, -halfSize.y, 0f);
            Vector3 p4 = center + rotation * new Vector3(-halfSize.x, -halfSize.y, 0f);

            drawLine(p1, p2);
            drawLine(p2, p3);
            drawLine(p3, p4);
            drawLine(p4, p1);
        }

        #endregion

        #region --- 3D Primitives ---

        /// <summary>
        /// Draws a wireframe sphere using three perpendicular circles.
        /// </summary>
        public static void DrawSphere(Vector3 center, float radius, int segments, DrawLineDelegate drawLine)
        {
            // XY plane
            DrawCircle(center, Vector3.forward, radius, segments, drawLine);
            // XZ plane
            DrawCircle(center, Vector3.up, radius, segments, drawLine);
            // YZ plane
            DrawCircle(center, Vector3.right, radius, segments, drawLine);
        }

        /// <summary>
        /// Draws the wireframe of an Axis-Aligned Bounding Box (AABB).
        /// </summary>
        public static void DrawBounds(Bounds bounds, DrawLineDelegate drawLine)
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            // Define the 8 corners
            Vector3 p1 = new Vector3(min.x, min.y, min.z);
            Vector3 p2 = new Vector3(max.x, min.y, min.z);
            Vector3 p3 = new Vector3(max.x, max.y, min.z);
            Vector3 p4 = new Vector3(min.x, max.y, min.z);

            Vector3 p5 = new Vector3(min.x, min.y, max.z);
            Vector3 p6 = new Vector3(max.x, min.y, max.z);
            Vector3 p7 = new Vector3(max.x, max.y, max.z);
            Vector3 p8 = new Vector3(min.x, max.y, max.z);

            // Draw the 12 edges
            // Bottom face
            drawLine(p1, p2);
            drawLine(p2, p3);
            drawLine(p3, p4);
            drawLine(p4, p1);

            // Top face
            drawLine(p5, p6);
            drawLine(p6, p7);
            drawLine(p7, p8);
            drawLine(p8, p5);

            // Vertical edges
            drawLine(p1, p5);
            drawLine(p2, p6);
            drawLine(p3, p7);
            drawLine(p4, p8);
        }

        /// <summary>
        /// Draws the wireframe of a box defined by its center, rotation, and size.
        /// </summary>
        public static void DrawBox(Vector3 center, Quaternion rotation, Vector3 size, DrawLineDelegate drawLine)
        {
            Vector3 halfSize = size / 2f;

            // Local corners
            Vector3[] localCorners = new Vector3[8];
            localCorners[0] = new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
            localCorners[1] = new Vector3( halfSize.x, -halfSize.y, -halfSize.z);
            localCorners[2] = new Vector3( halfSize.x,  halfSize.y, -halfSize.z);
            localCorners[3] = new Vector3(-halfSize.x,  halfSize.y, -halfSize.z);
            localCorners[4] = new Vector3(-halfSize.x, -halfSize.y,  halfSize.z);
            localCorners[5] = new Vector3( halfSize.x, -halfSize.y,  halfSize.z);
            localCorners[6] = new Vector3( halfSize.x,  halfSize.y,  halfSize.z);
            localCorners[7] = new Vector3(-halfSize.x,  halfSize.y,  halfSize.z);

            // World corners
            Vector3[] worldCorners = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                worldCorners[i] = center + rotation * localCorners[i];
            }

            // Edges
            int[] indices = new int[]
            {
                0, 1, 1, 2, 2, 3, 3, 0, // Bottom face
                4, 5, 5, 6, 6, 7, 7, 4, // Top face
                0, 4, 1, 5, 2, 6, 3, 7  // Vertical edges
            };

            for (int i = 0; i < indices.Length; i += 2)
            {
                drawLine(worldCorners[indices[i]], worldCorners[indices[i + 1]]);
            }
        }

        /// <summary>
        /// Draws the wireframe of a cylinder.
        /// </summary>
        public static void DrawCylinder(Vector3 start, Vector3 end, float radius, int segments, DrawLineDelegate drawLine)
        {
            Vector3 axis = end - start;
            float length = axis.magnitude;
            if (length < 0.0001f)
            {
                DrawSphere(start, radius, segments, drawLine);
                return;
            }

            Vector3 normal = axis.normalized;
            GetOrthonormalBasis(normal, out Vector3 axis1, out Vector3 axis2);

            // Draw the two end caps
            DrawCircle(start, normal, radius, segments, drawLine);
            DrawCircle(end, normal, radius, segments, drawLine);

            // Draw the side lines
            for (int i = 0; i < SIDE_LINES; i++)
            {
                float angle = i * 2f * Mathf.PI / SIDE_LINES;
                Vector3 offset = (Mathf.Cos(angle) * axis1 + Mathf.Sin(angle) * axis2) * radius;
                drawLine(start + offset, end + offset);
            }
        }

        /// <summary>
        /// Draws the wireframe of a capsule.
        /// </summary>
        public static void DrawCapsule(Vector3 start, Vector3 end, float radius, int segments, DrawLineDelegate drawLine)
        {
            Vector3 axis = end - start;
            float length = axis.magnitude;
            Vector3 normal = axis.normalized;
            
            GetOrthonormalBasis(normal, out Vector3 axis1, out Vector3 axis2);

            // Draw the two hemispheres
            DrawHemisphere(end, normal, radius, segments, drawLine);
            DrawHemisphere(start, -normal, radius, segments, drawLine);

            // Draw the side lines connecting the two hemispheres
            for (int i = 0; i < SIDE_LINES; i++)
            {
                float angle = i * 2f * Mathf.PI / SIDE_LINES;
                Vector3 offset = (Mathf.Cos(angle) * axis1 + Mathf.Sin(angle) * axis2) * radius;
                drawLine(start + offset, end + offset);
            }
        }

        /// <summary>
        /// Draws the wireframe of a capsule.
        /// </summary>
        public static void DrawCapsule(Ray ray, float radius, float distance, int segments, DrawLineDelegate drawLine)
        {
            Vector3 start = ray.origin;
            Vector3 end = ray.origin + ray.direction.normalized * distance;

            Vector3 normal = ray.direction.normalized;

            GetOrthonormalBasis(normal, out Vector3 axis1, out Vector3 axis2);

            // Draw the two hemispheres
            DrawHemisphere(end, normal, radius, segments, drawLine);
            DrawHemisphere(start, -normal, radius, segments, drawLine);

            // Draw the side lines connecting the two hemispheres
            for (int i = 0; i < SIDE_LINES; i++)
            {
                float angle = i * 2f * Mathf.PI / SIDE_LINES;
                Vector3 offset =
                    (Mathf.Cos(angle) * axis1 + Mathf.Sin(angle) * axis2) * radius;

                drawLine(start + offset, end + offset);
            }
        }

        /// <summary>
        /// Draws a wireframe hemisphere.
        /// </summary>
        /// <param name="center">Center of the hemisphere.</param>
        /// <param name="up">The direction vector pointing out of the flat side.</param>
        public static void DrawHemisphere(Vector3 center, Vector3 up, float radius, int segments, DrawLineDelegate drawLine)
        {
            up = up.normalized;
            GetOrthonormalBasis(up, out Vector3 axis1, out Vector3 axis2);

            // Draw the base circle (flat side)
            DrawCircle(center, up, radius, segments, drawLine);

            // Draw two perpendicular semi-circles to form the dome
            // Semi-circle 1 (along axis1)
            DrawArc(center, axis1, up, radius, 0f, 180f, segments / 2, drawLine);
            // Semi-circle 2 (along axis2)
            DrawArc(center, axis2, up, radius, 0f, 180f, segments / 2, drawLine);
        }

        /// <summary>
        /// Draws the wireframe of a cone.
        /// </summary>
        public static void DrawCone(Vector3 apex, Vector3 baseCenter, float baseRadius, int segments, DrawLineDelegate drawLine)
        {
            Vector3 axis = apex - baseCenter;
            Vector3 normal = axis.normalized;
            
            GetOrthonormalBasis(normal, out Vector3 axis1, out Vector3 axis2);

            // Draw the base circle
            DrawCircle(baseCenter, normal, baseRadius, segments, drawLine);

            // Draw side lines from the apex to the base
            for (int i = 0; i < SIDE_LINES; i++)
            {
                float angle = i * 2f * Mathf.PI / SIDE_LINES;
                Vector3 basePoint = baseCenter + (Mathf.Cos(angle) * axis1 + Mathf.Sin(angle) * axis2) * baseRadius;
                drawLine(apex, basePoint);
            }
        }

        /// <summary>
        /// Draws the wireframe of a camera's view frustum.
        /// </summary>
        public static void DrawFrustum(Camera camera, DrawLineDelegate drawLine)
        {
            Vector3[] corners = new Vector3[8];
            
            // Get corners in local space (relative to the camera's transform)
            // The array is populated with the 4 near corners first, then the 4 far corners.
            // Near corners: 0=bottom-left, 1=top-left, 2=top-right, 3=bottom-right
            // Far corners: 4=bottom-left, 5=top-left, 6=top-right, 7=bottom-right
            camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, corners);
            camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, corners);

            // Convert local corners to world corners using TransformPoint
            // This is the most robust way to account for camera position and rotation.
            for (int i = 0; i < 8; i++)
            {
                corners[i] = camera.transform.TransformPoint(corners[i]);
            }

            // Near plane (0, 1, 2, 3)
            drawLine(corners[0], corners[1]);
            drawLine(corners[1], corners[2]);
            drawLine(corners[2], corners[3]);
            drawLine(corners[3], corners[0]);

            // Far plane (4, 5, 6, 7)
            drawLine(corners[4], corners[5]);
            drawLine(corners[5], corners[6]);
            drawLine(corners[6], corners[7]);
            drawLine(corners[7], corners[4]);

            // Connecting lines
            drawLine(corners[0], corners[4]);
            drawLine(corners[1], corners[5]);
            drawLine(corners[2], corners[6]);
            drawLine(corners[3], corners[7]);
        }

        #endregion

        #region --- Utility Shapes ---

        /// <summary>
        /// Draws a line segment.
        /// </summary>
        public static void DrawLine(Vector3 start, Vector3 end, DrawLineDelegate drawLine)
        {
            drawLine(start, end);
        }

        /// <summary>
        /// Draws a ray (line with an arrow head).
        /// </summary>
        public static void DrawRay(Vector3 start, Vector3 direction, float length, float headSize, DrawLineDelegate drawLine)
        {
            Vector3 end = start + direction.normalized * length;
            DrawArrow(start, end, headSize, drawLine);
        }

        /// <summary>
        /// Draws an arrow (line with a cone head).
        /// </summary>
        public static void DrawArrow(Vector3 start, Vector3 end, float headSize, DrawLineDelegate drawLine)
        {
            drawLine(start, end);

            Vector3 direction = (end - start).normalized;
            if (direction == Vector3.zero) return;

            GetOrthonormalBasis(direction, out Vector3 right, out Vector3 up);

            Vector3 baseCenter = end - direction * headSize;

            // Draw cone base (a small circle)
            DrawCircle(baseCenter, direction, headSize * 0.3f, 8, drawLine);

            // Draw four lines from the apex (end) to the base circle
            for (int i = 0; i < 4; i++)
            {
                float angle = i * Mathf.PI / 2f;
                Vector3 basePoint = baseCenter + (Mathf.Cos(angle) * right + Mathf.Sin(angle) * up) * headSize * 0.3f;
                drawLine(end, basePoint);
            }
        }

        /// <summary>
        /// Draws a coordinate system (X-Red, Y-Green, Z-Blue).
        /// </summary>
        public static void DrawCoordinateSystem(Vector3 origin, Quaternion rotation, float size, DrawLineDelegate drawLine)
        {
            Vector3 xEnd = origin + rotation * Vector3.right * size;
            Vector3 yEnd = origin + rotation * Vector3.up * size;
            Vector3 zEnd = origin + rotation * Vector3.forward * size;

            // Note: Color is handled by the caller (DebugDraw/GizmoDraw)
            // The caller must call this three times with different colors.
            DrawArrow(origin, xEnd, size * 0.1f, drawLine);
            DrawArrow(origin, yEnd, size * 0.1f, drawLine);
            DrawArrow(origin, zEnd, size * 0.1f, drawLine);
        }

        #endregion
    }
}
