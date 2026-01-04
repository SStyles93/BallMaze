using UnityEngine;

namespace PxP.Draw
{
    /// <summary>
    /// Static class for drawing persistent editor shapes using Gizmos.DrawLine.
    /// This should be called from OnDrawGizmos or OnDrawGizmosSelected.
    /// </summary>
    public static class GizmoDraw
    {
        private static DrawingHelper.DrawLineDelegate GetDrawLineDelegate()
        {
            // Gizmos.DrawLine uses the currently set Gizmos.color
            return (start, end) => Gizmos.DrawLine(start, end);
        }

        // --- 2D Primitives ---

        /// <summary>
        /// Draws a wireframe circle.
        /// </summary>
        public static void Circle(Vector3 center, Vector3 normal, float radius, Color color, int segments = DrawingHelper.DEFAULT_SEGMENTS)
        {
            Gizmos.color = color;
            DrawingHelper.DrawCircle(center, normal, radius, segments, GetDrawLineDelegate());
        }

        /// <summary>
        /// Draws a wireframe arc.
        /// </summary>
        public static void Arc(Vector3 center, Vector3 axis1, Vector3 axis2, float radius, float startAngleDeg, float endAngleDeg, Color color, int segments = DrawingHelper.DEFAULT_SEGMENTS)
        {
            Gizmos.color = color;
            DrawingHelper.DrawArc(center, axis1, axis2, radius, startAngleDeg, endAngleDeg, segments, GetDrawLineDelegate());
        }

        /// <summary>
        /// Draws the wireframe of a rectangle.
        /// </summary>
        public static void Rectangle(Vector3 center, Quaternion rotation, Vector2 size, Color color)
        {
            Gizmos.color = color;
            DrawingHelper.DrawRectangle(center, rotation, size, GetDrawLineDelegate());
        }

        // --- 3D Primitives ---

        /// <summary>
        /// Draws a wireframe sphere.
        /// </summary>
        public static void Sphere(Vector3 center, float radius, Color color, int segments = DrawingHelper.DEFAULT_SEGMENTS)
        {
            Gizmos.color = color;
            DrawingHelper.DrawSphere(center, radius, segments, GetDrawLineDelegate());
        }

        /// <summary>
        /// Draws the wireframe of an Axis-Aligned Bounding Box (AABB).
        /// </summary>
        public static void Bounds(Bounds bounds, Color color)
        {
            Gizmos.color = color;
            DrawingHelper.DrawBounds(bounds, GetDrawLineDelegate());
        }

        /// <summary>
        /// Draws the wireframe of a box defined by its center, rotation, and size.
        /// </summary>
        public static void Box(Vector3 center, Quaternion rotation, Vector3 size, Color color)
        {
            Gizmos.color = color;
            DrawingHelper.DrawBox(center, rotation, size, GetDrawLineDelegate());
        }

        /// <summary>
        /// Draws the wireframe of a cylinder.
        /// </summary>
        public static void Cylinder(Vector3 start, Vector3 end, float radius, Color color, int segments = DrawingHelper.DEFAULT_SEGMENTS)
        {
            Gizmos.color = color;
            DrawingHelper.DrawCylinder(start, end, radius, segments, GetDrawLineDelegate());
        }

        /// <summary>
        /// Draws the wireframe of a capsule.
        /// </summary>
        public static void Capsule(Vector3 start, Vector3 end, float radius, Color color, int segments = DrawingHelper.DEFAULT_SEGMENTS)
        {
            Gizmos.color = color;
            DrawingHelper.DrawCapsule(start, end, radius, segments, GetDrawLineDelegate());
        }

        /// <summary>
        /// Draws the wireframe of a cone.
        /// </summary>
        public static void Cone(Vector3 apex, Vector3 baseCenter, float baseRadius, Color color, int segments = DrawingHelper.DEFAULT_SEGMENTS)
        {
            Gizmos.color = color;
            DrawingHelper.DrawCone(apex, baseCenter, baseRadius, segments, GetDrawLineDelegate());
        }

        /// <summary>
        /// Draws the wireframe of a camera's view frustum.
        /// </summary>
        public static void Frustum(Camera camera, Color color)
        {
            Gizmos.color = color;
            DrawingHelper.DrawFrustum(camera, GetDrawLineDelegate());
        }

        // --- Utility Shapes ---

        /// <summary>
        /// Draws a line segment.
        /// </summary>
        public static void Line(Vector3 start, Vector3 end, Color color)
        {
            Gizmos.color = color;
            DrawingHelper.DrawLine(start, end, GetDrawLineDelegate());
        }

        /// <summary>
        /// Draws a ray (line with an arrow head).
        /// </summary>
        public static void Ray(Vector3 start, Vector3 direction, float length, float headSize, Color color)
        {
            Gizmos.color = color;
            DrawingHelper.DrawRay(start, direction, length, headSize, GetDrawLineDelegate());
        }

        /// <summary>
        /// Draws an arrow (line with a cone head).
        /// </summary>
        public static void Arrow(Vector3 start, Vector3 end, float headSize, Color color)
        {
            Gizmos.color = color;
            DrawingHelper.DrawArrow(start, end, headSize, GetDrawLineDelegate());
        }

        /// <summary>
        /// Draws a coordinate system (X-Red, Y-Green, Z-Blue).
        /// </summary>
        public static void CoordinateSystem(Vector3 origin, Quaternion rotation, float size)
        {
            // X-axis (Red)
            Gizmos.color = Color.red;
            DrawingHelper.DrawRay(origin, rotation * Vector3.right, size, size * 0.1f, GetDrawLineDelegate());
            // Y-axis (Green)
            Gizmos.color = Color.green;
            DrawingHelper.DrawRay(origin, rotation * Vector3.up, size, size * 0.1f, GetDrawLineDelegate());
            // Z-axis (Blue)
            Gizmos.color = Color.blue;
            DrawingHelper.DrawRay(origin, rotation * Vector3.forward, size, size * 0.1f, GetDrawLineDelegate());
        }
    }
}
