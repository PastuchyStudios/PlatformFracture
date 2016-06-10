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
        if (vertices.Length < 3) {
            return 0;
        }
        int lastIndex = vertices.Length - 1;

        float sum = 0;
        for (int i = 0; i < lastIndex; i++) {
            sum += (vertices[i + 1].x - vertices[i].x) * (vertices[i].y + vertices[i + 1].y);
        }
        sum += (vertices[0].x - vertices[lastIndex].x) * (vertices[lastIndex].y + vertices[0].y);
        return -sum * 0.5f;
    }

    public static Rect GetAABB(params Vector2[] vertices) {
        float minX = Mathf.Infinity, minY = Mathf.Infinity, maxX = 0, maxY = 0;
        foreach (var vertex in vertices) {
            if (vertex.x < minX) {
                minX = vertex.x;
            }
            else if (vertex.x > maxX) {
                maxX = vertex.x;
            }
            if (vertex.y < minY) {
                minY = vertex.y;
            }
            else if (vertex.y > maxY) {
                maxY = vertex.y;
            }
        }
        return new Rect(minX, minY, maxX - minX, maxY - minY);
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
        var deltaA = a1 - a2;
        var deltaB = b1 - b2;

        float det = deltaA.x * deltaB.y - deltaA.y * deltaB.x;
        if (det == 0) {
            return null;
        }

        float detA = (a1.x * a2.y - a1.y * a2.x);
        float detB = (b1.x * b2.y - b1.y * b2.x);

        Vector2 intersection = new Vector2(
            (detA * deltaB.x - deltaA.x * detB) / det,
            (detA * deltaB.y - deltaA.y * detB) / det);

        if (GetAABB(a1, a2).Contains(intersection) && GetAABB(b1, b2).Contains(intersection)) {
            return intersection;
        }
        else {
            return null;
        }
    }

    private static Vector2? RayLineIntersection(Vector2 origin, Vector2 head, Vector2 lineEnd1, Vector2 lineEnd2) {
        var deltaRay = origin - head;
        var deltaLine = lineEnd1 - lineEnd2;

        float det = deltaRay.x * deltaLine.y - deltaRay.y * deltaLine.x;
        if (det == 0) {
            return null;
        }

        float detRay = (origin.x * head.y - origin.y * head.x);
        float detLine = (lineEnd1.x * lineEnd2.y - lineEnd1.y * lineEnd2.x);

        Vector2 intersection = new Vector2(
            (detRay * deltaLine.x - deltaRay.x * detLine) / det,
            (detRay * deltaLine.y - deltaRay.y * detLine) / det);

        if (!GetAABB(lineEnd1, lineEnd2).Contains(intersection)) {
            return null;
        }
        if ((deltaRay.y > 0 && intersection.y > origin.y) || (deltaRay.y < 0 && intersection.y < origin.y)) {
            return null;
        }
        if ((deltaRay.x > 0 && intersection.x > origin.x) || (deltaRay.x < 0 && intersection.x < origin.x)) {
            return null;
        }
        return intersection;
    }

    public static bool ContainsPoint(Vector2[] vertices, Vector2 point) {
        var flat = PolyKonvert.toValues(vertices);
        return PolyK.ContainsPoint(flat, point.x, point.y);
    }

    public static RaycastHitU Raycast(Vector2[] vertices, Vector2 origin, Vector2 direction) {
        Vector2 head = origin + direction;

        RaycastHitU result = null;

        for (uint i = 0; i < vertices.Length; i++) {
            var vertex1 = vertices[i % vertices.Length];
            var vertex2 = vertices[(i + 1) % vertices.Length];
            Vector2? intersection = RayLineIntersection(origin, head, vertex1, vertex2);
            if (intersection.HasValue) {
                RaycastHitU newResult = new RaycastHitU(origin, direction, vertex1, vertex2, intersection.Value, i);
                if (result == null || newResult.distance < result.distance) {
                    result = newResult;
                }
            }
        }

        return result;
    }

    public static ClosestEdgeU ClosestEdge(Vector2[] vertices, Vector2 point) {
        if (vertices.Length < 1) {
            return null;
        }

        var verticesList = new List<Vector2>(vertices);
        verticesList.Add(vertices[0]);

        float minDistanceSquared = Mathf.Infinity;
        int minIndex = -1;
        Vector2 minA = Vector2.zero, minB = Vector2.zero;
        Vector2 closestPoint = Vector2.zero;    // will always be overwritten

        for (int i = 0; i < vertices.Length; i++) {
            var a = verticesList[i];
            var b = verticesList[i + 1];
            Vector2 pointOnSegment = closestPointOnSegment(a, b, point);
            float distanceSqr = distanceSquared(point, pointOnSegment);
            if (distanceSqr < minDistanceSquared) {
                minDistanceSquared = distanceSqr;
                minIndex = i;
                minA = a;
                minB = b;
                closestPoint = pointOnSegment;
            }
        }

        return new ClosestEdgeU(minIndex, minA, minB, minDistanceSquared, closestPoint);
    }

    private static float distanceSquared(Vector2 k, Vector2 l) {
        return sqr(k.x - l.x) + sqr(k.y - l.y);
    }

    private static float sqr(float x) {
        return x * x;
    }

    private static Vector2 closestPointOnSegment(Vector2 a, Vector2 b, Vector2 point) {
        // Based on http://forum.unity3d.com/threads/math-problem.8114/#post-59715
        Vector3 vVector1 = new Vector3(point.x - a.x, 0, point.y - a.y);
        Vector3 vVector2 = new Vector3(b.x - a.x, 0, b.y - a.y).normalized;

        float d = Vector2.Distance(a, b);
        float t = Vector3.Dot(vVector2, vVector1);

        if (t <= 0) {
            return a;
        }
        if (t >= d) {
            return b;
        }

        var vVector3 = vVector2 * t;
        return new Vector2(a.x + vVector3.x, a.y + vVector3.z);
    }
}
