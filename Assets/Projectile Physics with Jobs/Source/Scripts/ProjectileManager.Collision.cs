using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            public List<PhysicMaterial> _toIgnorePhysicsMaterials;
            //public RaycastHit[] _hits;
            public object _sender;
            public bool hasHitOnce;
            public bool _allowSelfCollisionOfRoot;

            public CollisionData(Transform runtimeProjectile, LayerMask hitLayer, PhysicMaterial[] toIgnorePhysicsMaterial, bool allowSelfCollisionOfRoot, object sender)
            {
                _lastPosition = _currentPosition = runtimeProjectile.position;
                _runtimeProjectile = runtimeProjectile;
                _hitLayer = hitLayer;
                _sender = sender;
                _toIgnorePhysicsMaterials = toIgnorePhysicsMaterial != null ? toIgnorePhysicsMaterial.ToList() : null;
                // _hits = new RaycastHit[_toIgnorePhysicsMaterials != null && _toIgnorePhysicsMaterials.Length > 0 ? _toIgnorePhysicsMaterials.Length + 1 : 1];
                hasHitOnce = false;
                _allowSelfCollisionOfRoot = allowSelfCollisionOfRoot;
            }
        }

        List<CollisionData> _collisionData = new List<CollisionData>();
        //RaycastHit[] _hits = new RaycastHit[1];
        // [SerializeField] protected LayerMask _CollisionLayerMask;

        void AddCollisionData(Transform t, ProjectileData data, object sender)
        {
            if (_collisionData.Exists(x => x._runtimeProjectile == t) == false) _collisionData.Add(new CollisionData(t, data.hitLayer, data.CollidersWithPhysicsMaterialIgnore, data.AllowHitFromTheSameRootTransformAsSender, sender));
        }

        void RemoveCollisionData(int index)
        {
            if (index < _collisionData.Count && index != -1) _collisionData.RemoveAt(index);
        }

        const string _instanceName = "(Instance)";

        void OnFixedUpdateCollision()
        {
            for (int p = 0; p < _collisionData.Count; p++)
            {
                CollisionData _data = _collisionData[p];

                _data._currentPosition = _data._runtimeProjectile.position;

                float3 direction = _data._currentPosition - _data._lastPosition;
                float3 directionNormalized = math.normalize(direction);
                RaycastHit[] hits = Physics.RaycastAll(_data._lastPosition, directionNormalized, direction.ToMagnitude(), _data._hitLayer, QueryTriggerInteraction.Ignore);


                if (hits.Length > 0 && !_data.hasHitOnce)
                {
                    RaycastHit _hit = default;
                    Collider _validCollider = null;
                    float distance = math.INFINITY;
                    for (int h = 0; h < hits.Length; h++)
                    {
                        Collider[] _colliders = hits[h].collider.GetComponents<Collider>();
                        MonoBehaviour s = (_data._sender as MonoBehaviour);
                        Transform senderRoot = s != null ? s.transform.root : null;
                        for (int c = 0; c < _colliders.Length; c++)
                        {
                            Collider collider = _colliders[c];
                            Transform colliderRoot = collider.transform.root;

                            bool isSelfCollision = senderRoot != null && senderRoot == colliderRoot;
                            bool isWrongMaterial = false;
                            if (collider.material != null && _data._toIgnorePhysicsMaterials != null)
                            {
                                bool isNotAllowedByName = _data._toIgnorePhysicsMaterials.Exists(x => collider.material.name.Contains(x.name));
                                bool isNotAllowByRef = _data._toIgnorePhysicsMaterials.Contains(collider.material);

                                if (isNotAllowedByName || isNotAllowByRef) isWrongMaterial = true;
                            }

                            bool valid = (!isSelfCollision && _data._allowSelfCollisionOfRoot == false || _data._allowSelfCollisionOfRoot) && !isWrongMaterial;
                            if (valid && hits[h].distance < distance)
                            {
                                distance = hits[h].distance;
                                _validCollider = collider;
                                _hit = hits[h];
                            }
                        }
                    }

                    if (_validCollider != null)
                    {

                        UnregisterProjectile(_data._runtimeProjectile.gameObject,
                                            (UnregisterProjectileResult result) =>
                                            {
                                                GameObject _fx = null;
                                                if (result.ProjectileData.Hit_FXes != null)
                                                {
                                                    for (int fx = 0; fx < result.ProjectileData.Hit_FXes.Length; fx++)
                                                    {
                                                        bool isTag = string.IsNullOrEmpty(result.ProjectileData.Hit_FXes[fx]._Hit_Collider_Tag) == false && result.ProjectileData.Hit_FXes[fx]._Hit_Collider_Tag == _validCollider.gameObject.tag;

                                                        bool isLayer = _validCollider.gameObject.layer == result.ProjectileData.Hit_FXes[fx]._HitCollider_Layer;

                                                        if ((isTag || isLayer) && result.ProjectileData.Hit_FXes[fx]._Instatiate_FX_On_Hit != null)
                                                        {
                                                            _fx = Instantiate(result.ProjectileData.Hit_FXes[fx]._Instatiate_FX_On_Hit, _hit.point, Quaternion.LookRotation(-_hit.normal));
                                                            break;
                                                        }
                                                    }
                                                }
                                                if (_fx != null)
                                                {
                                                    _fx = Instantiate(result.ProjectileData.FallBackHit_FX, _hit.point, Quaternion.LookRotation(_hit.normal));
                                                }

                                                OnHit?.Invoke(new ProjectileHitResult(_hit, result, _fx, directionNormalized, _validCollider, _data._sender));
                                            });
                        _data.hasHitOnce = true;
                    }
                }

                _data._lastPosition = _data._currentPosition;
                _collisionData[p] = _data;
            }
        }
    }
}
