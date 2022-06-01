using UnityEngine.Jobs;
using UnityEngine;

namespace flow____.Combat
{
    public class Projectile : MonoBehaviour
    {



        public struct ProjectileJob : IJobParallelForTransform
        {
            public void Execute(int _i, TransformAccess transform)
            {

            }
        }
    }
}
