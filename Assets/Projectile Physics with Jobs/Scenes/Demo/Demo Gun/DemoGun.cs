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
                for (int i = 0; i < 5; i++)
                {
                    var result = _manager.Fire(this.transform.position, this.transform.forward, _currentProjectilePrefab);
                    testList.Add(result.Projectile);
                }
            }
            if (Input.GetKey(KeyCode.Mouse1))
            {
                if (testList.Count > 0)
                {
                    GameObject go = testList[0];
                    _manager.UnregisterProjectile(go, (ProjectileManager.UnregisterProjectileResult result) => testList.Remove(go));
                }
            }
        }
    }
}
