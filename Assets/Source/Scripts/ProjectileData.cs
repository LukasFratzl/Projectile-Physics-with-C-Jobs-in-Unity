using Unity.Mathematics;
using UnityEngine;

namespace flow____.Combat
{
    [CreateAssetMenu(fileName = "Projectile Data Object", menuName = "Advanced Projectile/Projectile Data Object")]
    public class ProjectileData : ScriptableObject
    {
        public float muzzleVelocity_m_per_second = 910f; // M4A1 -> 910 m/s
        // public float projectileRadius = 0.05f;
        // public float projectileMass = 0.2f;
        // //Drag coefficient (Tesla Model S has the drag coefficient 0.24) // 0.5f
        // public float projectile_Coefficent_Drag = 0.5f;
        // //Lift coefficient // 0.5f
        // public float projectile_Coefficent_Lift = 0.5f;
        public float3 gravity = new float3(0f, -9.81f, 0f);
        public float3 windSpeedVector = float3.zero;
        [Range(0f, 10f)] public float airDensity = 1.225f;
        //[Range(100f, 10000f)] public float distanceToAutoRemoveProjectileFromInitialPosition = 1000f;
        public LayerMask hitLayer;
    }
}
