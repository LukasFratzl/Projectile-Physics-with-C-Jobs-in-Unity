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

        void HandleAddtransformArray(Transform t, ProjectileData _data)
        {
            if (_projectileTransformToCompute.Contains(t) == false)
            {
                _projectileTransformToCompute.Add(t);
                HandleAllocationMotion();
                if (_job._settingHash.IsCreated == false) _job._settingHash = new NativeList<int>(0, Allocator.Persistent);
                _job._settingHash.Add(_data.GetHashCode());
            }
        }

        void HandleRemoveTransformArray(int index)
        {
            if (index != -1)
            {
                _projectileTransformToCompute.RemoveAt(index);
                HandleDeallocationMotion(index);
                _job._settingHash.RemoveAt(index);
            }
        }

        void HandleAllocationMotion()
        {
            if (_job.airDensity.IsCreated == false) _job.airDensity = new NativeList<float>(0, Allocator.Persistent);
            _job.airDensity.Add(default);

            if (_job.projectileCurrentVelocity.IsCreated == false) _job.projectileCurrentVelocity = new NativeList<float3>(0, Allocator.Persistent);
            _job.projectileCurrentVelocity.Add(default);

            if (_job.projectileInitialPosition.IsCreated == false) _job.projectileInitialPosition = new NativeList<float3>(0, Allocator.Persistent);
            _job.projectileInitialPosition.Add(default);

            if (_job.gravity.IsCreated == false) _job.gravity = new NativeList<float3>(0, Allocator.Persistent);
            _job.gravity.Add(default);

            if (_job.isFirstFrameOnProjectilePass.IsCreated == false) _job.isFirstFrameOnProjectilePass = new NativeList<byte>(0, Allocator.Persistent);
            _job.isFirstFrameOnProjectilePass.Add(default);

            if (_job.muzzleVelocity.IsCreated == false) _job.muzzleVelocity = new NativeList<float>(0, Allocator.Persistent);
            _job.muzzleVelocity.Add(default);

            if (_job.windSpeedVector.IsCreated == false) _job.windSpeedVector = new NativeList<float3>(0, Allocator.Persistent);
            _job.windSpeedVector.Add(default);
        }

        void HandleDeallocationMotion(int index)
        {
            _job.airDensity.RemoveAt(index);
            _job.projectileCurrentVelocity.RemoveAt(index);
            _job.projectileInitialPosition.RemoveAt(index);
            _job.gravity.RemoveAt(index);
            _job.isFirstFrameOnProjectilePass.RemoveAt(index);
            _job.muzzleVelocity.RemoveAt(index);
            _job.windSpeedVector.RemoveAt(index);
        }

        void HandleAssignValuesMotion(ProjectileData _data, int _i)
        {
            _job.airDensity[_i] = _data.airDensity;
            _job.gravity[_i] = _data.gravity;
            _job.muzzleVelocity[_i] = _data.muzzleVelocity_m_per_second;
            _job.windSpeedVector[_i] = _data.windSpeedVector;
        }

        void OnDisposeMotion()
        {
            if (_transformArray.isCreated) _transformArray.Dispose();

            if (_job.airDensity.IsCreated) _job.airDensity.Dispose();
            if (_job.projectileCurrentVelocity.IsCreated) _job.projectileCurrentVelocity.Dispose();
            if (_job.projectileInitialPosition.IsCreated) _job.projectileInitialPosition.Dispose();
            if (_job.gravity.IsCreated) _job.gravity.Dispose();
            if (_job.isFirstFrameOnProjectilePass.IsCreated) _job.isFirstFrameOnProjectilePass.Dispose();
            if (_job.muzzleVelocity.IsCreated) _job.muzzleVelocity.Dispose();
            if (_job.windSpeedVector.IsCreated) _job.windSpeedVector.Dispose();
        }

        public partial struct ProjectileJob : IJobParallelForTransform
        {
            [ReadOnly] public NativeList<float> muzzleVelocity;
            [ReadOnly] public NativeList<float3> gravity;
            [ReadOnly] public NativeList<float3> windSpeedVector;
            [ReadOnly] public NativeList<float> airDensity;
            [NativeDisableParallelForRestriction] public NativeList<float3> projectileCurrentVelocity;
            [NativeDisableParallelForRestriction] public NativeList<float3> projectileInitialPosition;
            [NativeDisableParallelForRestriction] public NativeList<byte> isFirstFrameOnProjectilePass;


            void ProjectileMove(int _i, TransformAccess _transform)
            {
                if (isFirstFrameOnProjectilePass[_i] == 0)
                {
                    isFirstFrameOnProjectilePass[_i] = 1;

                    projectileCurrentVelocity[_i] = math.mul(_transform.rotation, ProjectileHelper.Forward * muzzleVelocity[_i]);

                    projectileInitialPosition[_i] = _transform.position;
                }

                projectileCurrentVelocity[_i] += ((gravity[_i] * _deltaTime) + math.mul(_transform.rotation, -ProjectileHelper.Forward * airDensity[_i] * _deltaTime));

                _transform.position += (projectileCurrentVelocity[_i] * _deltaTime).ToVector3();
                _transform.rotation = Quaternion.LookRotation(math.normalize(projectileCurrentVelocity[_i]));
            }
        }
    }
}
