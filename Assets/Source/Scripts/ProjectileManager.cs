using System;
using System.Collections;
using System.Linq;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace flow____.Combat
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public class ProjectileManager : MonoBehaviour
    {
        [System.Serializable]
        public class ProjectileProfile
        {
            [Range(0, 10000)] public int ProjectilePreLoad = 1000;
            [Range(1, 10000)] public int ProjectilesPreLoadPerFrame = 100;
            public GameObject ProjectilePrefab;
            public ProjectileData ProjectileData;
            public Transform _poolRoot;
            public bool _finishedPrePool;
            public string _projectileNameInPool;
            public IEnumerator _poolingCoroutine;
        }

        public struct FireResult
        {
            public GameObject Projectile;
            public ProjectileData ProjectileData;
        }

        public struct DestroyProjectileResult
        {
            public GameObject ProjectileInSpawnPool;
            public bool SuccessPooling;
            public ProjectileData ProjectileData;
        }

        [SerializeField] protected ProjectileProfile[] _Profiles = default;

        void Awake()
        {
            for (int p = 0; p < _Profiles.Length; p++)
            {
                RegisterProjectileProfile(_Profiles[p]);
            }
        }

        void OnDestroy()
        {
            StopAllCoroutines();
        }

        public FireResult Fire(float3 startPosition, float3 forward, GameObject ProjectilePrefab)
        {
            FireResult result = new FireResult();
            var spawnResult = SpawnProjectile(startPosition, forward, ProjectilePrefab);
            result.Projectile = spawnResult.projectile;
            result.ProjectileData = spawnResult.profile.ProjectileData;

            return result;
        }

        public DestroyProjectileResult DestroyProjectile(GameObject ProjectileRuntime)
        {
            DestroyProjectileResult result = new DestroyProjectileResult();
            var poolResult = PoolProjectile(ProjectileRuntime);
            result.SuccessPooling = poolResult.success;
            result.ProjectileInSpawnPool = ProjectileRuntime;
            result.ProjectileData = poolResult.profile.ProjectileData;

            return result;
        }



        public void RegisterProjectileProfile(ProjectileProfile _profile)
        {
            if (_profile != null)
            {
                bool exists = Array.Exists(_Profiles, x => x == _profile);
                if (!exists)
                {
                    Array.Resize(ref _Profiles, _Profiles.Length + 1);
                    _Profiles[_Profiles.Length - 1] = _profile;
                }

                _profile._poolingCoroutine = PrePool(_profile);
                StartCoroutine(_profile._poolingCoroutine);
            }
        }

        public void UnregisterProjectileProfile(ProjectileProfile _profile)
        {
            if (_profile != null)
            {
                int index = Array.IndexOf(_Profiles, _profile);
                if (index != -1)
                {
                    var list = _Profiles.ToList();
                    list.RemoveAt(index);
                    _Profiles = list.ToArray();
                }

                if (_profile._poolingCoroutine != null) StopCoroutine(_profile._poolingCoroutine);
            }
        }

        const string _poolRootName = "Pool Root -> {0} | Count -> {1}";
        const string _projectilePoolName = "Projectile ID -> {0}";

        public IEnumerator PrePool(ProjectileProfile _profile)
        {
            if (_profile != null)
            {
                _profile._finishedPrePool = false;
                if (_profile.ProjectilePrefab != null)
                {
                    if (_profile._poolRoot == null)
                    {
                        GameObject poolRoot = new GameObject(string.Format(_poolRootName, _profile.ProjectilePrefab.name, 0));
                        _profile._poolRoot = poolRoot.transform;
                        _profile._poolRoot.parent = this.transform;
                    }

                    string hashCode = _profile.ProjectilePrefab.GetHashCode().ToString();

                    GameObject _prototype = Instantiate(_profile.ProjectilePrefab);
                    _prototype.transform.parent = this.transform;
                    _prototype.SetActive(false);
                    _prototype.name = string.Format(_projectilePoolName, hashCode);
                    _profile._projectileNameInPool = _prototype.name;


                    int isValidPoolRuntime = 0;
                    for (int p = 0; p < _profile.ProjectilePreLoad; p++)
                    {
                        Transform _go = Instantiate(_prototype, float3.zero, quaternion.identity, _profile._poolRoot).transform;
                        _go.name = _prototype.name;

                        _profile._poolRoot.name = string.Format(_poolRootName, _profile.ProjectilePrefab.name, _profile._poolRoot.childCount);

                        isValidPoolRuntime++;
                        if (isValidPoolRuntime >= _profile.ProjectilesPreLoadPerFrame)
                        {
                            isValidPoolRuntime = 0;
                            yield return null;
                        }
                    }

                    _profile._finishedPrePool = true;
                }
                _profile._poolingCoroutine = null;
            }
        }

        (GameObject projectile, ProjectileProfile profile) SpawnProjectile(float3 position, float3 forward, GameObject ProjectilePrefab)
        {
            ProjectileProfile _profile = null;
            for (int p = 0; p < _Profiles.Length; p++)
            {
                if (_Profiles[p] != null && _Profiles[p].ProjectilePrefab == ProjectilePrefab)
                {
                    _profile = _Profiles[p];
                    break;
                }
            }
            if (_profile == null) return default;
            Transform projectile = null;
            if (_profile._poolRoot.childCount > 0)
            {
                projectile = _profile._poolRoot.GetChild(0);
                projectile.parent = null;
            }
            else
            {
                GameObject _go = Instantiate(ProjectilePrefab, position, Quaternion.LookRotation(math.normalize(forward)));
                _go.name = string.Format(_projectilePoolName, _profile.ProjectilePrefab.GetHashCode());
                if (_go.activeInHierarchy == false) _go.SetActive(true);
                return (_go, _profile);
            }
            if (projectile != null)
            {
                projectile.position = position;
                projectile.rotation = Quaternion.LookRotation(math.normalize(forward));
                projectile.gameObject.SetActive(true);
                return (projectile.gameObject, _profile);
            }
            return default;
        }

        (bool success, ProjectileProfile profile) PoolProjectile(GameObject projectile)
        {
            ProjectileProfile _profile = null;
            for (int p = 0; p < _Profiles.Length; p++)
            {
                if (_Profiles[p] != null && _Profiles[p]._projectileNameInPool == projectile.name)
                {
                    _profile = _Profiles[p];
                    break;
                }
            }
            if (_profile == null)
            {
                Destroy(projectile);
                return default;
            }
            else
            {
                projectile.SetActive(false);
                projectile.transform.parent = _profile._poolRoot;
                return (true, _profile);
            }
        }

        public struct ProjectileJob : IJobParallelForTransform
        {
            public void Execute(int _i, TransformAccess transform)
            {

            }
        }
    }
}
