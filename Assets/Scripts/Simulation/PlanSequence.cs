using System;
using System.Collections.Generic;
using UnityEngine;

public class PlanSequence : SimulationPlan {
    public override List<Step> getSteps() {
        List<Step> steps = new List<Step>();
        foreach (Transform child in transform) {
            steps.AddRange(child.GetComponent<SimulationPlan>().getSteps());
        }
        return steps;
    }
}