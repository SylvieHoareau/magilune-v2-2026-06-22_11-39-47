using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGun : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("Shoot Point")]
    [SerializeField] private Transform firePoint;

    [Header("Cooldown")]
    [SerializeField] private float fireRate = 0.3f;

    private float nextFireTime;
    private @PlayerInput playerInputAsset; 
    private InputAction shootAction;

    private void Awake()
    {
        playerInputAsset = new @PlayerInput();    
        shootAction = playerInputAsset.Player.Shoot;
    }

    private void OnEnable()  => shootAction.Enable();
    private void OnDisable() => shootAction.Disable();

    void Update()
    {
          if (shootAction.WasPressedThisFrame() && Time.time >= nextFireTime)
            Shoot();
    }

    private void Shoot()
    {
        nextFireTime = Time.time + fireRate;

        GameObject projectile =
            Instantiate(
                projectilePrefab,
                firePoint.position,
                Quaternion.identity);

        Projectile projectileScript =
            projectile.GetComponent<Projectile>();

        Vector2 direction =
            transform.localScale.x > 0
            ? Vector2.right
            : Vector2.left;

        projectileScript.SetDirection(direction);
    }
}