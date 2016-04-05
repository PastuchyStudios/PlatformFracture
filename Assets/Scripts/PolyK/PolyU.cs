using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public static class PolyU {
    public static bool IsSimple(Vector2[] vertices) {
        var flat = PolyKonvert.toValues(vertices);
        return PolyK.IsSimple(flat);
    }

    public static bool IsConvex(Vector2[] vertices) {
        var flat = PolyKonvert.toValues(vertices);
        return PolyK.IsConvex(flat);
    }

    public static float GetArea(Vector2[] vertices) {
        var flat = PolyKonvert.toValues(vertices);
        return PolyK.GetArea(flat);
    }

    public static Rect GetAABB(Vector2[] vertices) {
        var flat = PolyKonvert.toValues(vertices);
        return PolyK.GetAABB(flat);
    }

    public static int[] Triangulate(Vector2[] vertices) {
        var flat = PolyKonvert.toValues(vertices);
        var rawOutput = PolyK.Triangulate(flat);
        return Array.ConvertAll(rawOutput, item => (int) item);
    }

    public static ICollection<Vector2[]> Slice(Vector2[] vertices, Vector2 p1, Vector2 p2) {
        var flat = PolyKonvert.toValues(vertices);
        ICollection<Vector2[]> triangles = new HashSet<Vector2[]>();
        var rawOutput = PolyK.Slice(flat, p1.x, p1.y, p2.x, p2.y);
        foreach (float[] flatTriangle in rawOutput) {
            var converted = PolyKonvert.toVectors(flatTriangle);
            triangles.Add(converted);
        }
        return triangles;
    }

    private static Vector2? GetLineIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2) {
        float deltaAX = a1.x - a2.x;
        float deltaBX = b1.x - b2.x;
        float deltaAY = a1.y - a2.y;
        float deltaBY = b1.y - b2.y;

        var det = deltaAX * deltaBY - deltaAY * deltaBX;
        if (det == 0) {
            return null;
        }

        var detA = (a1.x * a2.y - a1.y * a2.x);
        var detB = (b1.x * b2.y - b1.y * b2.x);

        Vector2 intersection = new Vector2(
                (detA * deltaBX - deltaAX * detB) / det,
                (detA * deltaBY - deltaAY * detB) / det
            );

        if (InRect(intersection, a1, a2) && InRect(intersection, b1, b2)) {
            return intersection;
        }
        else {
            return null;
        }
    }

    private static bool InRect(Vector2 a, Vector2 b, Vector2 c) {
        var minX = Mathf.Min(b.x, c.x);
        var maxX = Mathf.Max(b.x, c.x);
        var minY = Mathf.Min(b.y, c.y);
        var maxY = Mathf.Max(b.y, c.y);

        if (minX == maxX) return (minY <= a.y && a.y <= maxY);
        if (minY == maxY) return (minX <= a.x && a.x <= maxX);

        return minX <= a.x + 1e-10
            && a.x - 1e-10 <= maxX
            && minY <= a.y + 1e-10
            && a.y - 1e-10 <= maxY;

    }

    public static bool ContainsPoint(Vector2[] vertices, Vector2 point) {
        var flat = PolyKonvert.toValues(vertices);
        return PolyK.ContainsPoint(flat, point.x, point.y);
    }

    public static PolyKRaycastHit Raycast(Vector2[] vertices, Vector2 origin, Vector2 direction) {
        var flat = PolyKonvert.toValues(vertices);
        return (PolyKRaycastHit) PolyK.Raycast(flat, origin.x, origin.y, direction.x, direction.y, null);
    }

    public static PolyKClosestEdge ClosestEdge(Vector2[] vertices, Vector2 point) {
        var flat = PolyKonvert.toValues(vertices);
        return (PolyKClosestEdge) PolyK.ClosestEdge(flat, point.x, point.y, null);
    }
}

//class FlaggedVector2 {
//    public float x { get; set; }
//    public float y { get; set; }
//    public bool flag { get; set; }

//    public FlaggedVector2(float x, float y, bool flag) {
//        this.x = x;
//        this.y = y;
//        this.flag = flag;
//    }

//    public Vector2 toVector2() {
//        return new Vector2(x, y);
//    }
//}