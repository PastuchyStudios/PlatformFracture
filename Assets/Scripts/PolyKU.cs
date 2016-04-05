using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public static class PolyKU {
    private static bool IsConvex(Vector2[] vertices) {
        if (vertices.Length <= 3) {
            return true;
        }

        var l = vertices.Length - 2;

        for (int i = 0; i < l; i++) {
            if (!Convex3(vertices[0], vertices[1], vertices[2])) {
                return false;
            }
        }

        if (!Convex3(vertices[l], vertices[l + 1], vertices[0])) {
            return false;
        }
        if (!Convex3(vertices[l + 1], vertices[0], vertices[1])) {
            return false;
        }

        return true;
    }

    public static ICollection<Vector2[]> Slice(Vector2[] vertices, Vector2 p1, Vector2 p2) {
        if (PolyU.ContainsPoint(vertices, p1) || PolyU.ContainsPoint(vertices, p2)) {
            return new List<Vector2[]>() { vertices };
        }

        var flaggableVertices = Array.ConvertAll(vertices, (vertex) => new FV2(vertex.x, vertex.y, false));
        var intersections = new List<FV2>();
        var points = new List<FV2>(flaggableVertices);

        for (int i = 0; i < points.Count; i++) {
            var thisVertex = points[i];
            var nextVertex = points[(i + 1) % points.Count];

            Vector2? intersection = GetLineIntersection(p1, p2, thisVertex.toVector2(), nextVertex.toVector2());
            if (!intersection.HasValue) {
                continue;
            }

            if (intersections.Count == 0
                || (Vector2.Distance(intersection.Value, intersections[0].toVector2()) > 1e-10)
                    && Vector2.Distance(intersection.Value, intersections[intersections.Count - 1].toVector2()) > 1e-10) {
                var flaggedIntersection = new FV2(intersection.Value, true);
                intersections.Add(flaggedIntersection);
                points.Insert(i + 1, flaggedIntersection);
                i++;
            }
        }

        if (intersections.Count < 2) {
            return new List<Vector2[]>() { vertices };
        }

        intersections.Sort((u, v) => Math.Sign(
                Vector2.Distance(p1, u.toVector2()) - Vector2.Distance(p1, v.toVector2())
            ));

        var polygons = new List<Vector2[]>();
        var direction = 0;

        while (intersections.Count > 0) {
            var x0 = intersections[0];
            var x1 = intersections[1];
            var index0 = points.IndexOf(x0);
            var index1 = points.IndexOf(x1);
            var solved = false;

            if (firstFlaggedIndex(points, index0) == index1) {
                solved = true;
            }
            else {
                x0 = intersections[1];
                x1 = intersections[0];
                index0 = points.IndexOf(x0);
                index1 = points.IndexOf(x1);
                if (firstFlaggedIndex(points, index0) == index1) {
                    solved = true;
                }
            }

            if (solved) {
                direction--;
                var polygon = GetWrappingSubrange(points, index0, index1);
                polygons.Add(Array.ConvertAll(polygon.ToArray(), (fv2) => fv2.toVector2()));
                points = GetWrappingSubrange(points, index1, index0);
                x0.flag = x1.flag = false;
                intersections.RemoveRange(0, 2);
                if (intersections.Count == 0) {
                    polygons.Add(Array.ConvertAll(points.ToArray(), (fv2) => fv2.toVector2()));
                }
            }
            else {
                direction++;
                intersections.Reverse();
            }

            if (direction > 1) {
                break;
            }
        }

        return polygons;
    }

    public static int[] Triangulate(Vector2[] vertices) {
        if (vertices.Length < 3) {
            return new int[0];
        }

        var triangles = new List<int>((vertices.Length - 2) * 3);
        var verticesLeft = new List<int>(vertices.Length);

        for (int i = 0; i < verticesLeft.Capacity; i++) {
            verticesLeft.Add(i);
        }

        var index = 0;
        var left = verticesLeft.Count;

        while (left > 3) {
            int i0 = verticesLeft[(index + 0) % left];
            int i1 = verticesLeft[(index + 1) % left];
            int i2 = verticesLeft[(index + 2) % left];

            Vector2 a = vertices[i0];
            Vector2 b = vertices[i1];
            Vector2 c = vertices[i2];

            bool earFound = false;

            if (Convex3(a, b, c)) {
                earFound = true;
                for (int j = 0; j < left; j++) {
                    int vertexIndex = verticesLeft[j];
                    if (vertexIndex == i0 || vertexIndex == i1 || vertexIndex == i2) {
                        continue;
                    }
                    if (PointInTriangle(vertices[vertexIndex], a, b, c)) {
                        earFound = false;
                        break;
                    }
                }
            }

            if (earFound) {
                triangles.Add(i0);
                triangles.Add(i1);
                triangles.Add(i2);
                verticesLeft.RemoveRange((index + 1) % left, 1);
                left--;
                index = 0;
            }
            else if (index++ > 3 * left) {
                break;
            }
        }

        triangles.Add(verticesLeft[0]);
        triangles.Add(verticesLeft[1]);
        triangles.Add(verticesLeft[2]);

        return triangles.ToArray();
    }

    private static bool Convex3(Vector2 p1, Vector2 p2, Vector3 p3) {
        return (p1.y - p2.y) * (p3.x - p2.x) + (p2.x - p1.x) * (p3.y - p2.y) >= 0;
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

    private static int firstFlaggedIndex(List<FV2> points, int startingIndex) {
        int index = startingIndex;
        while (true) {
            index = (index + 1) % points.Count;
            if (index == startingIndex) {
                throw new ArgumentException("No flagged points");
            }
            if (points[index].flag) {
                return index;
            }
        }
    }

    private static List<FV2> GetWrappingSubrange(List<FV2> points, int startIndex, int endIndex) {
        var subrange = new List<FV2>();
        if (endIndex < startIndex) {
            endIndex += points.Count;
        }
        for (int i = startIndex; i <= endIndex; i++) {
            subrange.Add(points[i % points.Count]);
        }
        return subrange;
    }

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c) {
        var v0 = c - a;
        var v1 = b - a;
        var v2 = p - a;

        var dot00 = Vector2.Dot(v0, v0);
        var dot01 = Vector2.Dot(v0, v1);
        var dot02 = Vector2.Dot(v0, v2);
        var dot11 = Vector2.Dot(v1, v1);
        var dot12 = Vector2.Dot(v1, v2);

        var invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
        var u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        var v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        return (u >= 0) && (v >= 0) && (u + v < 1);
    }
}

class FV2 {
    public float x { get; set; }
    public float y { get; set; }
    public bool flag { get; set; }

    public FV2(Vector2 vector, bool flag) : this(vector.x, vector.y, flag) { }

    public FV2(float x, float y, bool flag) {
        this.x = x;
        this.y = y;
        this.flag = flag;
    }

    public Vector2 toVector2() {
        return new Vector2(x, y);
    }
}