using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace flow____.Combat
{
    public partial class ProjectileManager : MonoBehaviour
    {
        public struct CollisionData
        {
            public float3 _lastPosition;
            public float3 _currentPosition;
            public Transform _runtimeProjectile;
            public LayerMask _hitLayer;

            public CollisionData(Transform runtimeProjectile, LayerMask hitLayer)
            {
                _lastPosition = _currentPosition = runtimeProjectile.position;
                _runtimeProjectile = runtimeProjectile;
                _hitLayer = hitLayer;
            }
        }

        List<CollisionData> _collisionData = new List<CollisionData>();
        RaycastHit[] _hits = new RaycastHit[1];
        // [SerializeField] protected LayerMask _CollisionLayerMask;

        void AddCollisionData(Transform t, ProjectileData data)
        {
            if (_collisionData.Exists(x => x._runtimeProjectile == t) == false) _collisionData.Add(new CollisionData(t, data.hitLayer));
        }

        void RemoveCollisionData(int index)
        {
            if (index < _collisionData.Count && index != -1) _collisionData.RemoveAt(index);
        }

        void OnFixedUpdateCollision()
        {
            for (int p = 0; p < _collisionData.Count; p++)
            {
                CollisionData _data = _collisionData[p];

                _data._currentPosition = _data._runtimeProjectile.position;

                float3 direction = _data._currentPosition - _data._lastPosition;
                int hit = Physics.RaycastNonAlloc(_data._lastPosition, math.normalize(direction), _hits, direction.ToMagnitude(), _data._hitLayer, QueryTriggerInteraction.Ignore);

                if (hit > 0)
                {
                    UnregisterProjectile(_data._runtimeProjectile.gameObject, (UnregisterProjectileResult result) => OnHit?.Invoke(new ProjectileHitResult(_hits[0], result)));
                }

                _data._lastPosition = _data._currentPosition;
                _collisionData[p] = _data;
            }
        }
    }
}
