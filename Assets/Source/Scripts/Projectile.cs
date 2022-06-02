using UnityEngine.Jobs;
using UnityEngine;
using Unity.Mathematics;

namespace flow____.Combat
{
    public class Projectile : MonoBehaviour
    {
        public struct ProjectileDataRuntime
        {
            public float3 _gravity;
        }

        public ProjectileDataRuntime _runtimeData;


        public struct ProjectileJob : IJobParallelForTransform
        {
            public void Execute(int _i, TransformAccess transform)
            {

            }
        }
    }
}
