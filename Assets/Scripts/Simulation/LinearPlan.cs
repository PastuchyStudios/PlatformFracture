using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class LinearPlan : SimulationPlan {
    public Vector2 point1;
    public Vector2 point2;
    public int stepCount;
    public bool shatter;
    public int repeats = 1;
    public bool alternate = true;

    public override List<Step> getSteps() {
        List<Step> steps = new List<Step>();
        List<Step> once = getStepsOnce();
        for (int i = 0; i < repeats; i++) {
            steps.AddRange(once);
            if (alternate) {
                once.Reverse();
            }
        }
        return steps;
    }

    private List<Step> getStepsOnce() {
        List<Step> steps = new List<Step>(stepCount);
        for (int i = 0; i < stepCount; i++) {
            Vector2 point = Vector2.Lerp(point1, point2, i / (float) (stepCount - 1));
            steps.Add(new Step() { point = point, shatter = shatter });
        }
        return steps;
    }
}
