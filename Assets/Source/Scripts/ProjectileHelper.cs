using Unity.Mathematics;
using UnityEngine;

namespace flow____.Combat
{
    public class ProjectileHelper
    {
        public static readonly float3 Forward = new float3(0, 0, 1);
        public static readonly float3 Up = new float3(0, 1, 0);
        public static readonly float3 Right = new float3(1, 0, 0);
    }

    public static class VectorUtilities
    {
        public static float3 ToFloat3(this Vector3 to)
        {
            return to;
        }
        public static Vector3 ToVector3(this float3 to)
        {
            return to;
        }

        public static float ToMagnitude(this float3 to)
        {
            return to.ToVector3().magnitude;
        }
    }
}
