using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Breakable : ForceReceiver {

    public string breakableTag;

    private Vector2[] vertices = null;
    private Rect aabb;
    private Vector2 aabbDiagonal;

    private float highY;
    private float lowY;

    void Start() {
        var mesh = GetComponent<MeshFilter>().mesh;

        GetComponent<MeshCollider>().sharedMesh = mesh;
        setLowHighY(mesh);

        if (vertices == null) {
            setPlatform2dVertices(mesh);
        }

        calculateAABB();
    }

    private void setLowHighY(Mesh mesh) {
        float y1 = mesh.vertices[0].y;
        lowY = highY = y1;
        foreach (Vector3 vertex in mesh.vertices) {
            if (vertex.y != y1) {
                if (y1 < vertex.y) {
                    highY = vertex.y;
                }
                else {
                    lowY = vertex.y;
                }
                break;
            }
        }
    }

    private void setPlatform2dVertices(Mesh mesh) {
        //var vertices = new List<Vector2>();
        //int vertexIndex = 0;

        //foreach (Vector3 vertex3 in mesh.vertices) {
        //    if (vertex3.y < highY) {
        //        continue;
        //    }
        //    Vector2 vertex2 = new Vector2(vertex3.x, vertex3.z);
        //    if (!vertices.Contains(vertex2)) {
        //        vertices.Add(vertex2);
        //    }
        //    vertexIndex++;
        //}

        //this.vertices = vertices.ToArray();
        vertices = new Vector2[] {
            new Vector2(0.5f, 0.5f),
            new Vector2(-0.5f, 0.5f),
            new Vector2(-0.5f, -0.5f),
            new Vector2(0.5f, -0.5f)
        };
    }

    private void calculateAABB() {
        aabb = PolyU.GetAABB(this.vertices);
        aabbDiagonal = new Vector2(aabb.width, aabb.height);
    }

    public override void receiveHit(PlatformHit hit) {
        Vector3 localHitPoint = transform.InverseTransformPoint(hit.point);
        //localHitPoint = new Vector3(-0.4f, 0.5f, -0.4f);

        Vector3 scaledCenterOfMass = GetComponent<Rigidbody>().centerOfMass;
        Vector3 centerOfMass = new Vector3(
                scaledCenterOfMass.x / transform.localScale.x,
                scaledCenterOfMass.y / transform.localScale.y,
                scaledCenterOfMass.z / transform.localScale.z
            );

        Vector3 delta = localHitPoint - centerOfMass;
        Vector3 perpendicular = Vector3.Cross(delta, Vector3.up).normalized;

        // Perpendicular vector is normalized (length 1).
        // We can multiply it by AABB diagonal length, then add and subtract
        // from hit point to get segment points guaranteed to be outside the AABB.
        Vector2 p1 = flatten(localHitPoint + aabbDiagonal.magnitude * perpendicular);
        Vector2 p2 = flatten(localHitPoint + -aabbDiagonal.magnitude * perpendicular);

        var slices = PolyKU.Slice(vertices, p1, p2);
        Transform template = transform;
        while (!template.CompareTag(breakableTag)) {
            template = template.parent;
        }
        foreach (Vector2[] slice in slices) {
            GameObject sliceObject = Instantiate(template.gameObject) as GameObject;
            sliceObject.name = "Platform Slice";
            Transform geometry = sliceObject.transform.FindChild("Geometry");
            geometry.GetComponent<Breakable>().setShape(slice, lowY, highY);
        }
        Destroy(template.gameObject);
    }

    private static Vector2 flatten(Vector3 vector) {
        return new Vector2(vector.x, vector.z);
    }

    public void setShape(Vector2[] vertices, float lowY, float highY) {
        var mesh = Geometry.extrudePolygon(vertices, lowY, highY);
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        this.vertices = vertices;
    }
}
