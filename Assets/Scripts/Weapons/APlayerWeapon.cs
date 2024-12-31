using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Component.Animating;
using FishNet.Object;
using Unity.Mathematics;
using UnityEngine;

public abstract class APlayerWeapon : NetworkBehaviour
{
    public int damage;
    public float maxRange = 20f;
    public float fireRate = 0.5f;
    public LayerMask weaponHitLayers;
    
    private Transform _cameraTransform;
    private float _lastFireTime;
    private NetworkAnimator _networkAnimator;
    private Animator _animator;

    [SerializeField] private ParticleSystem muzzleFlash, bloodParticles, terrainHitParticles;

    private void Awake()
    {
        _cameraTransform = Camera.main.transform;
        if (TryGetComponent(out NetworkAnimator networkAnimator))
            _networkAnimator = networkAnimator;
        
        if (TryGetComponent(out Animator animator))
            _animator = animator;
        
        _animator.speed = 1 / fireRate;
    }

    public void Fire()
    {
        if (Time.time < _lastFireTime + fireRate)
            return;
        
        _lastFireTime = Time.time;
        AnimateWeapon();

        if (!Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, maxRange,
                weaponHitLayers))
            return;

        if (hit.transform.TryGetComponent(out PlayerHealth health))
        {
            health.TakeDamage(damage, OwnerId);
            Instantiate(bloodParticles, hit.point, Quaternion.LookRotation(hit.normal));
            return;
        }
        
        Instantiate(terrainHitParticles, hit.point, Quaternion.LookRotation(hit.normal));
    }

    [ServerRpc]
    public virtual void AnimateWeapon()
    {
        PlayAnimationObserver();
    }

    [ObserversRpc]
    private void PlayAnimationObserver()
    {
        muzzleFlash.Play();
        _networkAnimator.SetTrigger("Fire");
    }
}