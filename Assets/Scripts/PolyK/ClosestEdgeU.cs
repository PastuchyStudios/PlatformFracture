using UnityEngine;

public class ClosestEdgeU {
    public int edgeIndex { get; private set; }
    public Vector2 a { get; private set; }
    public Vector2 b { get; private set; }
    public float distance { get; private set; }
    public Vector2 point { get; private set; }

    public ClosestEdgeU(int edgeIndex, Vector2 a, Vector2 b, float distance, Vector2 point) {
        this.edgeIndex = edgeIndex;
        this.a = a;
        this.b = b;
        this.distance = distance;
        this.point = point;
    }
}
