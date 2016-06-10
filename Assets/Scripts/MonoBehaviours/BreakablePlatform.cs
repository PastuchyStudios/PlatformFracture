using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(Rigidbody))]
public class BreakablePlatform : ForceReceiver {

    public float fractureForce = 20;
    public float shatterForce = 40;
    [Range(0, 0.5f)]
    public float minSliceArea = 0.03f;
    [Range(0, 0.24f)]
    public float shatterTargetSliceArea = 0.15f;
    
    private Vector2[] _vertices = null;
    private float _area = 0;
    
    private Vector2[] vertices {
        get { return _vertices; }
        set {
            _vertices = value;
            _area = PolyU.GetArea(value);
        }
    }
    private float area { get { return _area; } }

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

    public override void receiveHit(AppliedForce force) {
      
        Vector3 localHitPoint = transform.InverseTransformPoint(force.point);
       
        if (force.magnitude >= shatterForce) {
            shatter(localHitPoint);
        }
        else if (force.magnitude >= fractureForce) {
            fracture(localHitPoint);
        }
    }

    private Vector3 getCenterOfMass() {
        Vector3 scaledCenterOfMass = GetComponent<Rigidbody>().centerOfMass;
        Vector3 centerOfMass = new Vector3(
                scaledCenterOfMass.x / transform.localScale.x,
                scaledCenterOfMass.y / transform.localScale.y,
                scaledCenterOfMass.z / transform.localScale.z
            );
        return centerOfMass;
    }

    public void fracture(Vector3 localHitPoint) {
        Vector3 centerOfMass = getCenterOfMass();

        Vector3 delta = localHitPoint - centerOfMass;
        Vector3 perpendicular = Vector3.Cross(delta, Vector3.up).normalized;

        Vector2 hitPoint2D = removeYComponent(localHitPoint);
        Vector2 perpendicular2D = removeYComponent(perpendicular);

        // Perpendicular vector is normalized (length 1).
        // We can multiply it by AABB diagonal length, then add and subtract
        // from hit point to get segment points guaranteed to be outside the AABB.
        Vector2 p1 = hitPoint2D + aabbDiagonal.magnitude * perpendicular2D;
        Vector2 p2 = hitPoint2D - aabbDiagonal.magnitude * perpendicular2D;

        sliceWithLine(gameObject, p1, p2, minSliceArea);
    }

    private void shatter(Vector3 localHitPoint) {
        if (area < 2 * minSliceArea) {
            return;
        }

        Vector2 hitPoint2D = removeYComponent(localHitPoint);
        Vector2 initialFractureVector = removeYComponent(localHitPoint - getCenterOfMass()).normalized;

        Vector2 p1 = hitPoint2D + aabbDiagonal.magnitude * initialFractureVector;
        Vector2 p2 = hitPoint2D - aabbDiagonal.magnitude * initialFractureVector;

        var splittableShards = sliceWithLine(gameObject, p1, p2);
        
        while (splittableShards.Count > 0) {
            var splittableArray = splittableShards.ToArray();
            foreach (GameObject splittableShard in splittableArray) {
                List<GameObject> newShards = shatterSplitShard(splittableShard, hitPoint2D);
                splittableShards.Remove(splittableShard);
                if (newShards.Count > 1) {
                    splittableShards.AddRange(newShards);
                }
            }
        }
    }

    private List<GameObject> shatterSplitShard(GameObject shardGameObject, Vector2 origin) {
        var shard = shardGameObject.GetComponent<BreakablePlatform>();
        if (shard.area < 2 * shatterTargetSliceArea) {
            return new List<GameObject>();
        }

        float minSplit = shatterTargetSliceArea / shard.area;
        float split = UnityEngine.Random.Range(minSplit, 1 - minSplit);

        var extremeVertices = getExtremeVertices(shard.vertices, origin);
        float angleBetweenVertices = Vector2.Angle(extremeVertices[0], extremeVertices[1]);
        float rotationAngle = angleBetweenVertices * split;
        Vector3 splitVector3D = Quaternion.Euler(0, rotationAngle, 0) * new Vector3(extremeVertices[0].x, 0, extremeVertices[0].y);
        Vector2 splitVector = removeYComponent(splitVector3D).normalized;

        Vector2 p1 = origin - splitVector * aabbDiagonal.magnitude;
        Vector2 p2 = origin + splitVector * aabbDiagonal.magnitude;

        return sliceWithLine(shardGameObject, p1, p2);
    }

    private Vector2[] getExtremeVertices(Vector2[] verticesArray, Vector2 origin) {
        int closestIndex = -1;
        for (int i = 1; i < verticesArray.Length; i++) {
            closestIndex = i;
        }

        int previousIndex, nextIndex;
        if (Vector2.Distance(origin, verticesArray[closestIndex]) < 0.0001) {
            // Just find and return neighbors
            previousIndex = (closestIndex - 1 + verticesArray.Length) % verticesArray.Length;
            nextIndex = (closestIndex + 1) % verticesArray.Length;
        }
        else {
            // Find the closeset edge and return end vertices
            ClosestEdgeU closestEdge = PolyU.ClosestEdge(verticesArray, origin);
            previousIndex = closestEdge.edgeIndex;
            nextIndex = (previousIndex + 1) % verticesArray.Length;
        }
        return new Vector2[] { verticesArray[previousIndex], verticesArray[nextIndex] };
    }

    private List<GameObject> sliceWithLine(GameObject platform, Vector2 p1, Vector2 p2, float minSliceArea = 0) {
        Vector2[] vertices = platform.GetComponent<BreakablePlatform>().vertices;

        var slices = PolyKU.Slice(vertices, p1, p2);
        foreach (Vector2[] slice in slices) {
            float sliceArea = PolyU.GetArea(slice);
            if (sliceArea < minSliceArea) {
                var singletonList = new List<GameObject>();
                singletonList.Add(gameObject);
                return singletonList;
            }
        }

        List<GameObject> shards = new List<GameObject>();
        foreach (Vector2[] slice in slices) {
            GameObject shard = Instantiate(platform) as GameObject;
            shard.name = "Platform Shard";
            shard.GetComponent<BreakablePlatform>().setShape(slice, lowY, highY);
            shards.Add(shard);
        }
        Destroy(platform);

        return shards;
    }

    private static Vector2 removeYComponent(Vector3 vector) {
        return new Vector2(vector.x, vector.z);
    }

    private void setShape(Vector2[] vertices, float lowY, float highY) {
        this.vertices = mergeCloseVertices(vertices);
        var mesh = Geometry.extrudePolygon(this.vertices, lowY, highY);
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    private Vector2[] mergeCloseVertices(Vector2[] vertices) {
        List<Vector2> result = new List<Vector2>(vertices);
        
        for (int i = 0; i < vertices.Length; i++) {
            var vertex1 = vertices[i];
            int offset = 1;
            var vertex2 = vertices[(i + offset) % vertices.Length];

            while (result.Count > 3 && Vector2.Distance(vertex1, vertex2) < 0.08) {
                result.Remove(vertex2);
                offset++;
                vertex2 = vertices[(i + offset) % vertices.Length];
            }
        }

        return result.ToArray();
    }
}
