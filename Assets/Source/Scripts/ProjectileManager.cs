using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace flow____.Combat
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public partial class ProjectileManager : MonoBehaviour
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

        public struct UnregisterProjectileResult
        {
            public GameObject ProjectileInSpawnPool;
            public bool SuccessPooling;
            public ProjectileData ProjectileData;
        }

        [SerializeField] protected ProjectileProfile[] _Profiles = default;
        protected Dictionary<string, ProjectileProfile> _ProfilesHashMap = new Dictionary<string, ProjectileProfile>();

        List<(Transform _projectile, ProjectileData _data, float _registerTime)> toRegisterProjectiles = new List<(Transform _projectile, ProjectileData _data, float _registerTime)>();
        List<(Transform runtimeProjectile, Action<UnregisterProjectileResult> onDone, float _unregisterTime)> toUnregisterProjectiles = new List<(Transform runtimeProjectile, Action<UnregisterProjectileResult> onDone, float _unregisterTime)>();

        protected JobHandle _projectileJobHandle;
        protected ProjectileJob _job;
        public static ProjectileManager instance;

        void Awake()
        {
            if (instance == null || instance != this)
            {
                if (instance != this)
                {
                    Debug.Log("More than 1 instance found.....");
                }
                instance = this;
            }
            _parallelOption = new ParallelOptions();
            _parallelOption.MaxDegreeOfParallelism = -1;
            for (int p = 0; p < _Profiles.Length; p++)
            {
                RegisterProjectileProfile(_Profiles[p]);
            }
        }

        void OnDestroy()
        {
            StopAllCoroutines();

            HandleDisposing();
        }

        void Update()
        {
            Solve1(Time.deltaTime);
        }

        void LateUpdate()
        {
            Solve2(Time.deltaTime);
        }

        void FixedUpdate()
        {
            OnFixedUpdateCollision();
        }

        void Solve1(float _deltaTime)
        {
            HandleUnregisterJobs();
            HandleRegisterJobs();

            HandleSchedulling(_deltaTime);
        }

        void Solve2(float _deltaTime)
        {
            HandleCompliting();
        }

        public FireResult Fire(float3 startPosition, float3 forward, GameObject ProjectilePrefab)
        {
            FireResult result = new FireResult();
            var spawnResult = SpawnProjectile(startPosition, forward, ProjectilePrefab);
            result.Projectile = spawnResult.projectile;
            result.ProjectileData = spawnResult.profile.ProjectileData;

            bool contains = false;
            for (int i = 0; i < toRegisterProjectiles.Count; i++)
            {
                if (toRegisterProjectiles[i]._projectile == spawnResult.projectile.transform)
                {
                    contains = true;
                    break;
                }
            }
            if (contains == false) toRegisterProjectiles.Add((spawnResult.projectile.transform, result.ProjectileData, Time.timeSinceLevelLoad));

            return result;
        }

        public void UnregisterProjectile(GameObject ProjectileRuntime, Action<UnregisterProjectileResult> onDone)
        {
            bool contains = false;
            for (int i = 0; i < toUnregisterProjectiles.Count; i++)
            {
                if (toUnregisterProjectiles[i].runtimeProjectile == ProjectileRuntime.transform)
                {
                    contains = true;
                    break;
                }
            }
            if (!contains) toUnregisterProjectiles.Add((ProjectileRuntime.transform, onDone, Time.timeSinceLevelLoad));
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

        void HandleRegisterJobs()
        {
            if (toRegisterProjectiles.Count > 0)
            {
                for (int r = toRegisterProjectiles.Count - 1; r >= 0; r--)
                {
                    var entity = toRegisterProjectiles[r];
                    toRegisterProjectiles.RemoveAt(r);

                    HandleAddtransformArray(entity._projectile, entity._data);
                    AddCollisionData(entity._projectile);
                }
            }
        }

        void HandleUnregisterJobs()
        {
            if (toUnregisterProjectiles.Count > 0)
            {
                for (int r = toUnregisterProjectiles.Count - 1; r >= 0; r--)
                {
                    var entity = toUnregisterProjectiles[r];
                    toUnregisterProjectiles.RemoveAt(r);

                    GameObject projectile = entity.runtimeProjectile?.gameObject;
                    UnregisterProjectileResult result = new UnregisterProjectileResult();
                    var poolResult = PoolProjectile(projectile);
                    result.SuccessPooling = poolResult.success;
                    result.ProjectileInSpawnPool = projectile;
                    result.ProjectileData = poolResult.profile.ProjectileData;

                    int index = _projectileTransformToCompute.IndexOf(entity.runtimeProjectile);
                    HandleRemoveTransformArray(index);
                    RemoveCollisionData(index);

                    entity.onDone?.Invoke(result);
                }
            }
        }

        ParallelOptions _parallelOption;

        void HandleSchedulling(float _deltaTime)
        {
            if (_job.Equals(default(ProjectileJob))) _job = new ProjectileJob();
            if (_transformArray.isCreated == false) return;
            if (_transformArray.length == 0) return;

            _job._deltaTime = _deltaTime;

            // STUFF
            Parallel.For(0, _Profiles.Length, _parallelOption, (int p) =>
            {
                if (string.IsNullOrEmpty(_Profiles[p]._projectileNameInPool) == false)
                {
                    ProjectileData data = _Profiles[p].ProjectileData;
                    int hash = data.GetHashCode();

                    for (int i = 0; i < _projectileTransformToCompute.Count; i++)
                    {
                        if (hash != _job._settingHash[i]) continue;

                        HandleAssignValuesMotion(data, i);
                    }
                }
            });

            // SCHEDULLE
            _projectileJobHandle = _job.Schedule(_transformArray);
        }

        void HandleCompliting()
        {
            if (_transformArray.isCreated == false)
            {
                if (_projectileJobHandle.Equals(default(JobHandle)) == false)
                {
                    _projectileJobHandle.Complete();
                    _projectileJobHandle = default(JobHandle);
                }
            }

            _projectileJobHandle.Complete();
        }

        void HandleDisposing()
        {
            _projectileJobHandle.Complete();

            if (_job._settingHash.IsCreated) _job._settingHash.Dispose();

            OnDisposeMotion();
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public partial struct ProjectileJob : IJobParallelForTransform
        {
            public float _deltaTime;

            [ReadOnly] public NativeList<int> _settingHash;

            public void Execute(int _i, TransformAccess _transform)
            {
                ProjectileMove(_i, _transform);
            }
        }
    }
}
