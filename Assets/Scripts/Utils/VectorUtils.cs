using UnityEngine;

public class VectorUtils {
    public static Vector3 CubicLerpVector(Vector3 from, Vector3 to, float t) {
        float clampedT = Mathf.Clamp(t, 0, 1);
        return Vector3.Lerp(from, to, 1 + Mathf.Pow(clampedT - 1, 3));
    }

    public static Vector3 SinLerpVector(Vector3 center, Vector3 amplitude, float t) {
        float clampedT = Mathf.Clamp(t, 0, 1);
        return Mathf.Sin(clampedT * 2 * Mathf.PI) * amplitude + center;
    }
}
