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

            public CollisionData(Transform runtimeProjectile)
            {
                _lastPosition = _currentPosition = runtimeProjectile.position;
                _runtimeProjectile = runtimeProjectile;
            }
        }

        List<CollisionData> _collisionData = new List<CollisionData>();
        RaycastHit[] _hits = new RaycastHit[1];
        [SerializeField] protected LayerMask _CollisionLayerMask;

        void AddCollisionData(Transform t)
        {
            if (_collisionData.Exists(x => x._runtimeProjectile == t) == false) _collisionData.Add(new CollisionData(t));
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
                int hit = Physics.RaycastNonAlloc(_data._lastPosition, math.normalize(direction), _hits, direction.ToMagnitude(), _CollisionLayerMask, QueryTriggerInteraction.Ignore);

                if (hit > 0)
                {
                    // Debug.Log(_data._runtimeProjectile.gameObject.GetHashCode());
                    UnregisterProjectile(_data._runtimeProjectile.gameObject, null);
                }

                _data._lastPosition = _data._currentPosition;
                _collisionData[p] = _data;
            }
        }
    }
}
