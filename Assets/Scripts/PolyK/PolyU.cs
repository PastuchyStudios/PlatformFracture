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

    public static uint[] Triangulate(Vector2[] vertices) {
        var flat = PolyKonvert.toValues(vertices);
        var rawOutput = PolyK.Triangulate(flat);
        return Array.ConvertAll(rawOutput, item => (uint) item);
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
