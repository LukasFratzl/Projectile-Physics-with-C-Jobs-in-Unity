using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace flow____.Combat
{
    public partial class ProjectileManager : MonoBehaviour
    {
        protected TransformAccessArray _transformArray;
        protected List<Transform> _projectileTransformToCompute = new List<Transform>();

        void HandleAddtransformArray(Transform t)
        {
            if (_projectileTransformToCompute.Contains(t) == false)
            {
                _projectileTransformToCompute.Add(t);
                if (_transformArray.isCreated == false) _transformArray = new TransformAccessArray(0);
                _transformArray.Add(t);
                HandleAllocationMotion();
            }
        }

        void HandleRemoveTransformArray(Transform t)
        {
            int index = _projectileTransformToCompute.IndexOf(t);
            if (index != -1)
            {
                _projectileTransformToCompute.RemoveAt(index);
                _transformArray.RemoveAtSwapBack(index);
                if (_projectileTransformToCompute.Count == 0)
                {
                    if (_transformArray.isCreated) _transformArray.Dispose();
                }
                HandleDeallocationMotion(index);
            }
        }

        void HandleAllocationMotion()
        {
            if (_job.airDensity.IsCreated == false) _job.airDensity = new NativeList<float>(0, Allocator.Persistent);
            _job.airDensity.Add(default);

            if (_job.projectileInitialVelocity.IsCreated == false) _job.projectileInitialVelocity = new NativeList<float3>(0, Allocator.Persistent);
            _job.projectileInitialVelocity.Add(default);

            if (_job.projectileCurrentVelocity.IsCreated == false) _job.projectileCurrentVelocity = new NativeList<float3>(0, Allocator.Persistent);
            _job.projectileCurrentVelocity.Add(default);

            if (_job.gravity.IsCreated == false) _job.gravity = new NativeList<float3>(0, Allocator.Persistent);
            _job.gravity.Add(default);

            if (_job.isFirstFrameOnProjectilePass.IsCreated == false) _job.isFirstFrameOnProjectilePass = new NativeList<byte>(0, Allocator.Persistent);
            _job.isFirstFrameOnProjectilePass.Add(default);

            if (_job.muzzleVelocity.IsCreated == false) _job.muzzleVelocity = new NativeList<float>(0, Allocator.Persistent);
            _job.muzzleVelocity.Add(default);

            if (_job.projectileInitialPosition.IsCreated == false) _job.projectileInitialPosition = new NativeList<float3>(0, Allocator.Persistent);
            _job.projectileInitialPosition.Add(default);

            if (_job.projectileInitialRotation.IsCreated == false) _job.projectileInitialRotation = new NativeList<quaternion>(0, Allocator.Persistent);
            _job.projectileInitialRotation.Add(default);

            if (_job.projectile_Coefficent_Drag.IsCreated == false) _job.projectile_Coefficent_Drag = new NativeList<float>(0, Allocator.Persistent);
            _job.projectile_Coefficent_Drag.Add(default);

            if (_job.projectile_Coefficent_Lift.IsCreated == false) _job.projectile_Coefficent_Lift = new NativeList<float>(0, Allocator.Persistent);
            _job.projectile_Coefficent_Lift.Add(default);

            if (_job.projectileMass.IsCreated == false) _job.projectileMass = new NativeList<float>(0, Allocator.Persistent);
            _job.projectileMass.Add(default);

            if (_job.projectileRadius.IsCreated == false) _job.projectileRadius = new NativeList<float>(0, Allocator.Persistent);
            _job.projectileRadius.Add(default);

            if (_job.windSpeedVector.IsCreated == false) _job.windSpeedVector = new NativeList<float3>(0, Allocator.Persistent);
            _job.windSpeedVector.Add(default);

            if (_job.projectileTime.IsCreated == false) _job.projectileTime = new NativeList<float>(0, Allocator.Persistent);
            _job.projectileTime.Add(default);
        }

        void HandleDeallocationMotion(int index)
        {
            _job.airDensity.RemoveAt(index);
            _job.projectileInitialVelocity.RemoveAt(index);
            _job.gravity.RemoveAt(index);
            _job.isFirstFrameOnProjectilePass.RemoveAt(index);
            _job.muzzleVelocity.RemoveAt(index);
            _job.projectileInitialPosition.RemoveAt(index);
            _job.projectileInitialRotation.RemoveAt(index);
            _job.projectile_Coefficent_Drag.RemoveAt(index);
            _job.projectile_Coefficent_Lift.RemoveAt(index);
            _job.projectileMass.RemoveAt(index);
            _job.projectileRadius.RemoveAt(index);
            _job.windSpeedVector.RemoveAt(index);
            _job.projectileTime.RemoveAt(index);
        }

        void HandleAssignValuesMotion(ProjectileData _data, int _i)
        {
            _job.airDensity[_i] = _data.airDensity;
            _job.gravity[_i] = _data.gravity;
            _job.muzzleVelocity[_i] = _data.muzzleVelocity_m_per_second;
            _job.projectile_Coefficent_Drag[_i] = _data.projectile_Coefficent_Drag;
            _job.projectile_Coefficent_Lift[_i] = _data.projectile_Coefficent_Lift;
            _job.projectileMass[_i] = _data.projectileMass;
            _job.projectileRadius[_i] = _data.projectileRadius;
            _job.windSpeedVector[_i] = _data.windSpeedVector;
        }

        void OnDisposeMotion()
        {
            if (_transformArray.isCreated) _transformArray.Dispose();

            if (_job.airDensity.IsCreated) _job.airDensity.Dispose();
            if (_job.projectileInitialVelocity.IsCreated) _job.projectileInitialVelocity.Dispose();
            if (_job.projectileCurrentVelocity.IsCreated) _job.projectileCurrentVelocity.Dispose();
            if (_job.gravity.IsCreated) _job.gravity.Dispose();
            if (_job.isFirstFrameOnProjectilePass.IsCreated) _job.isFirstFrameOnProjectilePass.Dispose();
            if (_job.muzzleVelocity.IsCreated) _job.muzzleVelocity.Dispose();
            if (_job.projectileInitialPosition.IsCreated) _job.projectileInitialPosition.Dispose();
            if (_job.projectileInitialRotation.IsCreated) _job.projectileInitialRotation.Dispose();
            if (_job.projectile_Coefficent_Drag.IsCreated) _job.projectile_Coefficent_Drag.Dispose();
            if (_job.projectile_Coefficent_Lift.IsCreated) _job.projectile_Coefficent_Lift.Dispose();
            if (_job.projectileMass.IsCreated) _job.projectileMass.Dispose();
            if (_job.projectileRadius.IsCreated) _job.projectileRadius.Dispose();
            if (_job.windSpeedVector.IsCreated) _job.windSpeedVector.Dispose();
            if (_job.projectileTime.IsCreated) _job.projectileTime.Dispose();
        }

        public partial struct ProjectileJob : IJobParallelForTransform
        {
            [ReadOnly] public NativeList<float> muzzleVelocity;
            [ReadOnly] public NativeList<float> projectileRadius;
            [ReadOnly] public NativeList<float> projectileMass;
            //Drag coefficient (Tesla Model S has the drag coefficient 0.24) // 0.5f
            [ReadOnly] public NativeList<float> projectile_Coefficent_Drag;
            //Lift coefficient // 0.5f
            [ReadOnly] public NativeList<float> projectile_Coefficent_Lift;
            [ReadOnly] public NativeList<float3> gravity;
            [ReadOnly] public NativeList<float3> windSpeedVector;
            [ReadOnly] public NativeList<float> airDensity;
            [NativeDisableParallelForRestriction] public NativeList<float3> projectileInitialPosition;
            [NativeDisableParallelForRestriction] public NativeList<float3> projectileInitialVelocity;
            [NativeDisableParallelForRestriction] public NativeList<float3> projectileCurrentVelocity;
            [NativeDisableParallelForRestriction] public NativeList<byte> isFirstFrameOnProjectilePass;
            [NativeDisableParallelForRestriction] public NativeList<float> projectileTime;
            [NativeDisableParallelForRestriction] public NativeList<quaternion> projectileInitialRotation;



            void ProjectileMove(int _i, TransformAccess _transform)
            {
                if (isFirstFrameOnProjectilePass[_i] == 0)
                {
                    isFirstFrameOnProjectilePass[_i] = 1;

                    projectileInitialVelocity[_i] = projectileCurrentVelocity[_i] = math.mul(_transform.rotation, ProjectileHelper.Forward * muzzleVelocity[_i]);

                    projectileInitialPosition[_i] = _transform.position;
                    projectileInitialRotation[_i] = _transform.rotation;
                }

                projectileCurrentVelocity[_i] -= ((-gravity[_i] * _deltaTime) + math.mul(_transform.rotation, ProjectileHelper.Forward * airDensity[_i] * _deltaTime));

                _transform.position += (projectileCurrentVelocity[_i] * _deltaTime).ToVector3();
                _transform.rotation = Quaternion.LookRotation(math.normalize(projectileCurrentVelocity[_i]));
            }
        }
    }
}
