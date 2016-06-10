using UnityEngine;

public class RaycastHitU {
    public Vector2 point { get; private set; }
    public float distance { get; private set; }
    public uint edgeIndex { get; private set; }
    public Vector2 normal { get; private set; }
    public Vector2 reflection { get; private set; }
    
    public RaycastHitU(Vector2 origin, Vector2 direction, Vector2 lineEnd1, Vector2 lineEnd2, Vector2 hitPoint, uint edgeIndex) {
        float distance = Vector2.Distance(origin, hitPoint);
        float lineLength = Vector2.Distance(lineEnd1, lineEnd2);

        Vector2 normal = new Vector2(
            -(lineEnd2.y - lineEnd1.y) / lineLength,
            (lineEnd2.x - lineEnd1.x) / lineLength);

        float ddot = 2 * (direction.x * normal.x + direction.y * normal.y);
        Vector2 reflection = new Vector2(
            -ddot * normal.x + direction.x,
            -ddot * normal.y + direction.y);

        assignFields(hitPoint, distance, edgeIndex, normal, reflection);
    }

    private void assignFields(Vector2 point, float distance, uint edgeIndex, Vector2 normal, Vector2 reflection) {
        this.point = point;
        this.distance = distance;
        this.edgeIndex = edgeIndex;
        this.normal = normal;
        this.reflection = reflection;
    }
}
