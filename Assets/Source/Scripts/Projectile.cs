using UnityEngine.Jobs;
using UnityEngine;
using Unity.Mathematics;

namespace flow____.Combat
{
    public class Projectile : MonoBehaviour
    {
        [Range(0f, 5000f)] public float _UnregisterProfileAfterSeconds = 100f;

        private float _removeSecondsRuntime;

        void OnEnable()
        {
            _removeSecondsRuntime = _UnregisterProfileAfterSeconds;
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
    }
}
