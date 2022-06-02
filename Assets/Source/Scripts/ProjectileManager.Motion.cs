using System;
using System.Collections.Generic;
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
            }
        }

        void OnDisposeMotion()
        {
            if (_transformArray.isCreated) _transformArray.Dispose();
        }

        public partial struct ProjectileJob : IJobParallelForTransform
        {
            void TestMove(int _i, TransformAccess _transform, float _deltaTime)
            {
                Vector3 delta = math.mul(_transform.rotation, ProjectileHelper.Forward * 10f * _deltaTime);
                _transform.position += delta;
            }
        }
    }
}
