using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private Transform bowPoint;
    [SerializeField] private GameObject arrow;
    [SerializeField] private Transform playerHead;
    [SerializeField] private float maxPower;
    [SerializeField] private float shootCooldown = 0.5f;
    public bool CanShoot { get; private set; } = true;

    /**
     * Power should be in range [0,1]
     */
    public void Shoot(float power)
    {
        if (!CanShoot)
            return;

        GameObject projectile = Instantiate(arrow, bowPoint.position, playerHead.rotation);
        projectile.GetComponent<Rigidbody>().AddForce(power * maxPower * projectile.transform.forward, ForceMode.Impulse);

        CanShoot = false;
        Invoke(nameof(ResetCooldown), shootCooldown);
    }

    private void ResetCooldown() => CanShoot = true;
}
