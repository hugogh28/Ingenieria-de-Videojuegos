using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Weapon;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; set; }

    [Header("Canales")]
    public AudioSource idleChannel;
    public AudioSource shootingChannel;
    public AudioSource reloadChannel;

    [Header("Lanzapatatas")]
    public AudioClip LanzapatatasIdle;
    public List<AudioClip> LanzapatatasShot = new List<AudioClip>();
    public AudioClip LanzapatatasReload;
    
    private bool next = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void PlayShootingSound(WeaponModel weapon)
    {
        switch (weapon)
        {
            case WeaponModel.Lanzapatatas:
                if (next)
                {
                    shootingChannel.PlayOneShot(LanzapatatasShot[0]);
                }
                else
                {
                    shootingChannel.PlayOneShot(LanzapatatasShot[1]);
                }
                next = !next;
                break;
            case WeaponModel.CañonSalado:
                if (next)
                {
                    shootingChannel.PlayOneShot(LanzapatatasShot[0]);
                }
                else
                {
                    shootingChannel.PlayOneShot(LanzapatatasShot[1]);
                }
                next = !next;
                break;
            case WeaponModel.FuriaFrijol:
                // Play Furia Frijol Shoot Sound
                break;
        }
    }

    public void PlayReloadSound(WeaponModel weapon)
    {
        switch (weapon)
        {
            case WeaponModel.Lanzapatatas:
                // Play Lanzapatatas Reload Sound
                break;
            case WeaponModel.CañonSalado:
                reloadChannel.PlayOneShot(LanzapatatasReload);
                break;
            case WeaponModel.FuriaFrijol:
                // Play Furia Frijol Reload Sound
                break;
        }
    }

    public void PlayIdleSound(WeaponModel weapon)
    {
        switch (weapon)
        {
            case WeaponModel.Lanzapatatas:
                // Play Lanzapatatas Idle Sound
                break;
            case WeaponModel.CañonSalado:
                idleChannel.PlayOneShot(LanzapatatasIdle);
                break;
            case WeaponModel.FuriaFrijol:
                // Play Furia Frijol Idle Sound
                break;
        }
    }
}
