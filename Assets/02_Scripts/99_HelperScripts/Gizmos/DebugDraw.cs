using UnityEngine;

namespace PxP.Draw
{
    /// <summary>
    /// Static class for drawing temporary debug shapes using UnityEngine.Debug.DrawLine.
    /// </summary>
    public static class DebugDraw
    {
        private static DrawingHelper.DrawLineDelegate GetDrawLineDelegate(UnityEngine.Color color, float duration)
        {
            return (start, end) => UnityEngine.Debug.DrawLine(start, end, color, duration);
        }

        // --- 2D Primitives ---

        /// <summary>
        /// Draws a wireframe circle.
        /// </summary>
        public static void Circle(Vector3 center, Vector3 normal, float radius, Color color, float duration = 0f, int segments = DrawingHelper.DEFAULT_SEGMENTS)
        {
            DrawingHelper.DrawCircle(center, normal, radius, segments, GetDrawLineDelegate(color, duration));
        }

        /// <summary>
        /// Draws a wireframe arc.
        /// </summary>
        public static void Arc(Vector3 center, Vector3 axis1, Vector3 axis2, float radius, float startAngleDeg, float endAngleDeg, Color color, float duration = 0f, int segments = DrawingHelper.DEFAULT_SEGMENTS)
        {
            DrawingHelper.DrawArc(center, axis1, axis2, radius, startAngleDeg, endAngleDeg, segments, GetDrawLineDelegate(color, duration));
        }

        /// <summary>
        /// Draws the wireframe of a rectangle.
        /// </summary>
        public static void Rectangle(Vector3 center, Quaternion rotation, Vector2 size, Color color, float duration = 0f)
        {
            DrawingHelper.DrawRectangle(center, rotation, size, GetDrawLineDelegate(color, duration));
        }

        // --- 3D Primitives ---

        /// <summary>
        /// Draws a wireframe sphere.
        /// </summary>
        public static void Sphere(Vector3 center, float radius, Color color, float duration = 0f, int segments = DrawingHelper.DEFAULT_SEGMENTS)
        {
            DrawingHelper.DrawSphere(center, radius, segments, GetDrawLineDelegate(color, duration));
        }

        /// <summary>
        /// Draws the wireframe of an Axis-Aligned Bounding Box (AABB).
        /// </summary>
        public static void Bounds(Bounds bounds, Color color, float duration = 0f)
        {
            DrawingHelper.DrawBounds(bounds, GetDrawLineDelegate(color, duration));
        }

        /// <summary>
        /// Draws the wireframe of a box defined by its center, rotation, and size.
        /// </summary>
        public static void Box(Vector3 center, Quaternion rotation, Vector3 size, Color color, float duration = 0f)
        {
            DrawingHelper.DrawBox(center, rotation, size, GetDrawLineDelegate(color, duration));
        }

        /// <summary>
        /// Draws the wireframe of a cylinder.
        /// </summary>
        public static void Cylinder(Vector3 start, Vector3 end, float radius, Color color, float duration = 0f, int segments = DrawingHelper.DEFAULT_SEGMENTS)
        {
            DrawingHelper.DrawCylinder(start, end, radius, segments, GetDrawLineDelegate(color, duration));
        }

        /// <summary>
        /// Draws the wireframe of a capsule.
        /// </summary>
        public static void Capsule(Vector3 start, Vector3 end, float radius, Color color, float duration = 0f, int segments = DrawingHelper.DEFAULT_SEGMENTS)
        {
            DrawingHelper.DrawCapsule(start, end, radius, segments, GetDrawLineDelegate(color, duration));
        }

        public static void Capsule(Ray ray, float radius, float distance, Color color, float duration = 0f, int segments = DrawingHelper.DEFAULT_SEGMENTS)
        {
            DrawingHelper.DrawCapsule(ray, radius, distance, segments, GetDrawLineDelegate(color, duration));
        }

        /// <summary>
        /// Draws the wireframe of a cone.
        /// </summary>
        public static void Cone(Vector3 apex, Vector3 baseCenter, float baseRadius, Color color, float duration = 0f, int segments = DrawingHelper.DEFAULT_SEGMENTS)
        {
            DrawingHelper.DrawCone(apex, baseCenter, baseRadius, segments, GetDrawLineDelegate(color, duration));
        }

        /// <summary>
        /// Draws the wireframe of a camera's view frustum.
        /// </summary>
        public static void Frustum(Camera camera, Color color, float duration = 0f)
        {
            DrawingHelper.DrawFrustum(camera, GetDrawLineDelegate(color, duration));
        }

        // --- Utility Shapes ---

        /// <summary>
        /// Draws a line segment.
        /// </summary>
        public static void Line(Vector3 start, Vector3 end, Color color, float duration = 0f)
        {
            DrawingHelper.DrawLine(start, end, GetDrawLineDelegate(color, duration));
        }

        /// <summary>
        /// Draws a ray (line with an arrow head).
        /// </summary>
        public static void Ray(Vector3 start, Vector3 direction, float length, float headSize, Color color, float duration = 0f)
        {
            DrawingHelper.DrawRay(start, direction, length, headSize, GetDrawLineDelegate(color, duration));
        }

        /// <summary>
        /// Draws an arrow (line with a cone head).
        /// </summary>
        public static void Arrow(Vector3 start, Vector3 end, float headSize, Color color, float duration = 0f)
        {
            DrawingHelper.DrawArrow(start, end, headSize, GetDrawLineDelegate(color, duration));
        }

        /// <summary>
        /// Draws a coordinate system (X-Red, Y-Green, Z-Blue).
        /// </summary>
        public static void CoordinateSystem(Vector3 origin, Quaternion rotation, float size, float duration = 0f)
        {
            // X-axis (Red)
            DrawingHelper.DrawRay(origin, rotation * Vector3.right, size, size * 0.1f, GetDrawLineDelegate(Color.red, duration));
            // Y-axis (Green)
            DrawingHelper.DrawRay(origin, rotation * Vector3.up, size, size * 0.1f, GetDrawLineDelegate(Color.green, duration));
            // Z-axis (Blue)
            DrawingHelper.DrawRay(origin, rotation * Vector3.forward, size, size * 0.1f, GetDrawLineDelegate(Color.blue, duration));
        }
    }
}
