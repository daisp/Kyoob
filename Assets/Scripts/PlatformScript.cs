﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformScript : MonoBehaviour {
    private PlayerScriptWithAnimator _playerScript;
    private Animator _playerParentAnim;
    private ParticleSystem _playerDebrisParticleSystem;
    private ParticleSystem _platformDebrisParticleSystem;
    private GameManagerScript _gameManager;
    public bool IsDestroyed;
    public bool IsDestroyable;
    private CameraShake _cameraShake;
    private AudioManager _audioManager;

    private void Start() {
        _playerScript = GameObject
            .Find("Player Animation Parent/Boost Stretcher/Player")
            .GetComponent<PlayerScriptWithAnimator>();
        _gameManager = GameObject
            .Find("GrandDaddy/Menu Parent/Menu/Game Manager")
            .GetComponent<GameManagerScript>();
        _playerParentAnim = GameObject.Find("Player Animation Parent")
            .GetComponent<Animator>();
        _playerDebrisParticleSystem = GameObject
            .Find("Player Animation Parent/Debris Particle System")
            .GetComponent<ParticleSystem>();
        _platformDebrisParticleSystem =
            gameObject.transform.parent.transform.parent.gameObject
                .GetComponent<ParticleSystem>();
        IsDestroyed = false;
        _cameraShake = Camera.main.GetComponent<CameraShake>();
        _audioManager = GameObject.Find("Audio Manager")
            .GetComponent<AudioManager>();
    }

    public void OnTriggerEnter(Collider other) {
        if (other.name != "Player" || IsDestroyed) return;
        _audioManager.Play("PlatformDestruction");
        _cameraShake.enabled = false;
        _cameraShake.enabled = true;
        _platformDebrisParticleSystem.Play(true);
        gameObject.transform.parent.gameObject.GetComponent<Animator>()
            .Play("PlatformParentDestroyAnimation");
        if (Vibration.HasVibrator()) Vibration.Vibrate(20);
        if (!_playerScript.IsDestructive && !IsDestroyable &&
            !_playerScript.IsBoosted &&
            !_playerScript.IsAddingLosingLife &&
            !_playerScript.IsExploding) {
            _gameManager.LoseLife();
            _playerDebrisParticleSystem.Play(true);
            if (!_playerScript.IsAboutToExplode)
                _playerParentAnim.Play("PlayerParentHitAnimation");
        }

        if (IsDestroyable || _playerScript.IsExploding)
            _playerScript.AddToScore(50, false);
        if (_playerScript.IsDestructive) _playerScript.AddToScore(20, false);
        var components = gameObject.GetComponents<Collider>();
        Destroy(components[0]);
        Destroy(GetComponent<Light>());
        IsDestroyed = true;
    }

    private void OnParticleCollision(GameObject particleSystem) {
        if (particleSystem.gameObject.name !=
            "Projectiles Particle System") return;
        _audioManager.Play("PlatformDestruction");
        _cameraShake.enabled = false;
        _cameraShake.enabled = true;
        _platformDebrisParticleSystem.Play(true);
        gameObject.transform.parent.gameObject.GetComponent<Animator>()
            .Play("PlatformParentDestroyAnimation");
        if (Vibration.HasVibrator()) Vibration.Vibrate(20);
        if (IsDestroyable) _playerScript.AddToScore(50, false);
        _playerScript.AddToScore(20, false);
        IsDestroyed = true;
        var components = gameObject.GetComponents<Collider>();
        Destroy(components[0]);
        Destroy(GetComponent<Light>());
    }
}