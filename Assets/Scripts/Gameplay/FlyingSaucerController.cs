using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FlyingSaucerController : MonoBehaviour
{
    [SerializeField] private float bulletCooldown = 0.5f;
    [SerializeField] private float bulletSpeed = 15f;
    private float currentCooldown = 0f;

    void Update()
    {
        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
        }
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        if (!collider.CompareTag("Player"))
        {
            return;
        }
        if (currentCooldown <= 0)
        {
            BulletController bullet = InstancePoolsManager.Instance.BulletControllerPool.Get();
            bullet.Init();

            bullet.transform.position = transform.position;
            bullet.FromTo(transform.position, collider.transform.position);
            bullet.speed = bulletSpeed;

            currentCooldown = bulletCooldown;
        }
    }

}
