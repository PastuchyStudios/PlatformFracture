using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Simulator : MonoBehaviour {
    public BreakablePlatform platformPrefab;
    public string platformTag;
    public int stepIntervalMilliseconds = 500;
    public Transform hammer;
    public float hammerMultiplier = 1;

    [Range(0.01f, 0.49f)]
    public float margin;
    public int stepsPerStage;

    private DateTime lastStep;
    private BreakablePlatform platform;

    private List<Step> plan = new List<Step>();

    void Start() {
        lastStep = DateTime.Now;
        generatePlan(margin, stepsPerStage);
    }

    void FixedUpdate() {
        TimeSpan interval = new TimeSpan(0, 0, 0, 0, stepIntervalMilliseconds);
        if (DateTime.Now - lastStep >= interval) {
            lastStep = DateTime.Now;
            if (plan.Count == 0) {
                return;
            }

            resetSimulation();
            simulateStep();
        }
    }

    private void resetSimulation() {
        var children = new List<Transform>(transform.childCount);
        foreach (Transform childTransform in transform) {
            children.Add(childTransform);
        }
        foreach (Transform childTransform in children) {
            DestroyImmediate(childTransform.gameObject);
        }

        GameObject newInstance = Instantiate(platformPrefab.gameObject) as GameObject;
        newInstance.transform.parent = transform;
        newInstance.tag = platformTag;

        platform = newInstance.GetComponent<BreakablePlatform>();
        platform.Start();
    }

    private void simulateStep() {
        Step step = plan[0];
        plan.RemoveAt(0);
        var shards = hit(step.point, step.shatter);
        Debug.Log("Hit " + step.point + " shatter " + step.shatter + " => " + shards.Count + " shards");
    }

    private List<GameObject> hit(Vector2 point, bool shatter) {
        if (point == Vector2.zero) {
            point = UnityEngine.Random.insideUnitCircle.normalized / 1000;
        }

        Vector3 point3 = new Vector3(point.x, 0, point.y);
        hammer.position = point3 * hammerMultiplier;
        hammer.gameObject.SetActive(true);

        List<GameObject> shards;
        if (shatter) {
            shards = platform.shatter(point3);
        } else {
            shards = platform.fracture(point3);
        }

        foreach (GameObject shard in shards) {
            shard.transform.parent = transform;
        }
        return shards;
    }

    private void generatePlan(float margin, int pointCount) {
        float minCoord = -0.5f + margin;
        float maxCoord = -minCoord;

        Vector2 minVector = new Vector2(-0.1f, minCoord);
        Vector2 maxVector = new Vector2(0.1f, maxCoord);
        addInterpolatedToPlan(minVector, maxVector, pointCount, false);

        minVector = new Vector2(minCoord * 1.2f, minCoord);
        maxVector = new Vector2(maxCoord * 1.2f, maxCoord);
        addInterpolatedToPlan(minVector, maxVector, pointCount, false);

        minVector = new Vector2(minCoord, 0);
        maxVector = new Vector2(maxCoord, 0);
        addInterpolatedToPlan(minVector, maxVector, pointCount, true);
    }

    private void addInterpolatedToPlan(Vector2 minVector, Vector2 maxVector, int pointCount, bool shatter) {
        for (int i = 0; i <= pointCount; i++) {
            Vector2 point = Vector2.Lerp(minVector, maxVector, i / (float) pointCount);
            plan.Add(new Step() { point = point, shatter = shatter });
        }
    }

    private struct Step {
        public Vector2 point { get; set; }
        public bool shatter { get; set; }
    }
}
