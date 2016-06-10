using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Simulator : MonoBehaviour {
    public BreakablePlatform platformPrefab;
    public string platformTag;
    public int stepIntervalMilliseconds = 500;
    public SimulationPlan simulationPlan;
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
        plan = simulationPlan.getSteps();
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
}
