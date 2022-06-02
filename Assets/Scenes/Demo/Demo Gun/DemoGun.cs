using System.Collections.Generic;
using UnityEngine;

namespace flow____.Combat
{
    public class DemoGun : MonoBehaviour
    {
        List<GameObject> testList = new List<GameObject>();

        [SerializeField] protected ProjectileManager _manager;
        [SerializeField] protected GameObject _currentProjectilePrefab;


        void Update()
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                var result = _manager.Fire(this.transform.position, this.transform.forward, _currentProjectilePrefab);
                testList.Add(result.Projectile);
            }
            if (Input.GetKey(KeyCode.Mouse1))
            {
                if (testList.Count > 0)
                {
                    GameObject go = testList[testList.Count - 1];
                    _manager.UnregisterProjectile(go, (ProjectileManager.DestroyProjectileResult result) => testList.Remove(go));
                }
            }
        }
    }
}
