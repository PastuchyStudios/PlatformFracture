using System.Collections.Generic;
using UnityEngine;

public abstract class SimulationPlan : MonoBehaviour {
    public abstract List<Step> getSteps();
}

