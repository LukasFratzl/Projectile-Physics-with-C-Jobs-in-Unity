using UnityEngine;
using UnityEngine.Events;

namespace flow____.Combat
{
    public class Projectile : MonoBehaviour
    {
        [Range(0f, 5000f), SerializeField] protected float _UnregisterProjectileAfterSeconds = 100f;

        private float _removeSecondsRuntime;

        [SerializeField] protected UnityEvent<ProjectileManager.ProjectileHitResult> OnProjectileHit;

        void OnEnable()
        {
            ProjectileManager.instance.OnHit -= InvokeProjectileHit;
            ProjectileManager.instance.OnHit += InvokeProjectileHit;

            _removeSecondsRuntime = _UnregisterProjectileAfterSeconds;
        }

        void OnDisable()
        {
            ProjectileManager.instance.OnHit -= InvokeProjectileHit;
        }

        void Update()
        {
            if (_removeSecondsRuntime > 0)
            {
                _removeSecondsRuntime -= Time.deltaTime;

                if (_removeSecondsRuntime <= 0f)
                {
                    ProjectileManager.instance.UnregisterProjectile(this.gameObject, null);
                }
            }
        }

        void InvokeProjectileHit(ProjectileManager.ProjectileHitResult result)
        {
            OnProjectileHit?.Invoke(result);
        }
    }
}
