using UnityEngine;
using System.Collections;
using System;

public static class PolyKonvert {
    public static Vector2[] toVectors(float[] values) {
        if (values.Length % 2 != 0) {
            throw new ArgumentException("Odd number of coord values");
        }
        Vector2[] vectors = new Vector2[values.Length / 2];
        for (int i = 0; i < vectors.Length; i++) {
            vectors[i] = new Vector2(values[i * 2], values[i * 2 + 1]);
        }
        return vectors;
    }

    public static float[] toValues(Vector2[] vectors) {
        float[] values = new float[vectors.Length * 2];
        for (int i = 0; i < vectors.Length; i++) {
            Vector2 vector = vectors[i];
            values[i * 2] = vector.x;
            values[i * 2 + 1] = vector.y;
        }
        return values;
    }
}
