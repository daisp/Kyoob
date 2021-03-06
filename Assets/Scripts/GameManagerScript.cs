﻿using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerScript : MonoBehaviour {
    private Renderer _instructions1;
    private Renderer _instructions2;
    private Animator _pauseMenuAnim;
    private Animator _livesLeftTextAnim;
    private Animator _livesLeftKyoobParentAnim;
    private Animator _camAnim;
    private ParticleSystem _backgroundParticles;
    private ParticleSystem _backgroundParticlesBright;
    private ParticleSystem _magnetismParticles;
    private ParticleSystem _projectilesParticles;
    private ParticleSystem _playerDebrisParticles;
    private ParticleSystem _playerExplosionParticles;
    private PowerUpManager _powerUpManager;
    public int LivesLeft;
    private TextMeshProUGUI _livesLeftText;
    public GameTimer Timer;
    public bool IsPaused;
    private bool _isEscKeyReleased;
    private PlayerScriptWithAnimator _playerScript;
    private AudioManager _audioManager;
    public GameObject BroadcastPrefab;
    private GameObject _scoreCanvas;
    public List<GameObject> Broadcasts;
    public ScoreStreak ScoreStreakObj;

    private static void Adjust3dGUIToWideScreen() {
        GameObject.Find("3D GUI Parent").transform.position =
            new Vector3(-0.9f, 0.2f, 0f);
        GameObject.Find("Left Lose Limit").transform.position =
            new Vector3(-14.5f, -11.3f, 0f);
    }

    private static bool closeTo(float a, float b, float tolerance) {
        return Mathf.Abs(a - b) <= tolerance;
    }

    // Use this for initialization
    private void Start() {
        var ratio = (float) Screen.width / Screen.height;
//        Debug.Log("Aspect ratio is " + ratio);
        if (closeTo(ratio, 18 / 9f, 0.01f)) {
            Adjust3dGUIToWideScreen();
        }

        _backgroundParticles = GameObject.Find("Background Particle System")
            .GetComponent<ParticleSystem>();
        _backgroundParticlesBright = GameObject
            .Find("Background Particle System Bright")
            .GetComponent<ParticleSystem>();
        _magnetismParticles = GameObject
            .Find("GrandDaddy/Player Animation Parent/Magnet Particle System")
            .GetComponent<ParticleSystem>();
        _projectilesParticles = GameObject
            .Find("GrandDaddy/Projectiles Particle System")
            .GetComponent<ParticleSystem>();
        _playerExplosionParticles = GameObject
            .Find("GrandDaddy/Player Animation Parent/Boost Stretcher/Player")
            .GetComponent<ParticleSystem>();
        _playerDebrisParticles = GameObject
            .Find("GrandDaddy/Player Animation Parent/Debris Particle System")
            .GetComponent<ParticleSystem>();
        _scoreCanvas = GameObject.Find("ScoreCanvas");
        _powerUpManager = GameObject.Find("Power Up Manager")
            .GetComponent<PowerUpManager>();
        _livesLeftText = GameObject.Find("ScoreCanvas/Lives Count")
            .GetComponent<TextMeshProUGUI>();
        _livesLeftTextAnim = GameObject.Find("ScoreCanvas/Lives Count")
            .GetComponent<Animator>();
        _livesLeftKyoobParentAnim = GameObject
            .Find("Lives Count Kyoob Parent").GetComponent<Animator>();
        _pauseMenuAnim = GameObject.Find("GrandDaddy").GetComponent<Animator>();
        _camAnim = Camera.main.GetComponent<Animator>();
        Timer = new GameTimer(_powerUpManager.BoostPowerUpDuration,
            _powerUpManager.DestructionPowerUpDuration,
            _powerUpManager.MagnetPowerUpDuration,
            _powerUpManager.ProjectilesPowerUpDuration,
            _powerUpManager.ExplosionPowerUpDuration);
        _instructions1 =
            GameObject.Find("instructions1").GetComponent<Renderer>();
        _instructions2 =
            GameObject.Find("instructions2").GetComponent<Renderer>();
        _isEscKeyReleased = true;
        _playerScript =
            GameObject.Find(
                    "GrandDaddy/Player Animation Parent/Boost Stretcher/Player")
                .GetComponent<PlayerScriptWithAnimator>();
        _audioManager = GameObject.Find("Audio Manager")
            .GetComponent<AudioManager>();
        _audioManager.FirstGameFrame = true;
        Broadcasts = new List<GameObject>();
        ScoreStreakObj = new ScoreStreak();
        if (_playerScript == null) Debug.LogWarning("_scoreStreak is null");
    }

    public void BroadcastMessageOrScore(string message, bool isScore) {
        var broadcast = Instantiate(BroadcastPrefab, Vector3.zero,
            Quaternion.identity);
        broadcast.transform.SetParent(_scoreCanvas.transform, false);
        broadcast.GetComponent<TextMeshProUGUI>().text = message;
        Broadcasts.Add(broadcast);
        if (isScore)
            broadcast.GetComponent<Animator>().Play("BroadcastScoreAnimation");
    }

    public void LoseLife() {
        _audioManager.Play("LoseLife");
        if (Vibration.HasVibrator()) Vibration.Vibrate(20);
        if (!_camAnim.GetCurrentAnimatorStateInfo(0)
            .IsName("ExplosionAnimation")) {
            if (!_playerScript.IsExploding && !_playerScript.IsAboutToExplode)
                _camAnim.Play("LoseLifeAnimation");
        }

        LivesLeft--;
        if (LivesLeft < 1) {
            RestartLevel();
            return;
        }

        ScoreStreakObj.StreakEnd();
        _livesLeftText.text = "X" + LivesLeft;
        _livesLeftTextAnim.Play("LivesCountTextPickupAnimation");
        _livesLeftKyoobParentAnim.Play("LivesCountKyoobParentPickupAnimation");
    }

    public void Slowdown() {
        if (Vibration.HasVibrator()) Vibration.Vibrate(20);
        BroadcastMessageOrScore(" SLOWDOWN", false);
    }

    public void AddLife() {
        if (Vibration.HasVibrator()) Vibration.Vibrate(20);
        BroadcastMessageOrScore(" EXTRA LIFE", false);
        LivesLeft++;
        _livesLeftText.text = "X" + LivesLeft;
        _livesLeftTextAnim.Play("LivesCountTextPickupAnimation");
        _livesLeftKyoobParentAnim.Play("LivesCountKyoobParentPickupAnimation");
    }

    private void RestartLevel() {
        _audioManager.StopSounds();
        PlayerPrefs.SetInt("lastScore", Mathf.RoundToInt(_playerScript.Score));
        PlayerPrefs.SetInt("highscore",
            Mathf.RoundToInt(_playerScript.HighScore));
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ToggleInstructions() {
        if (_instructions1 != null) {
            _instructions1.enabled = !_instructions1.enabled;
        }

        if (_instructions2 != null)
            _instructions2.enabled = !_instructions2.enabled;
    }

    public void Resume() {
        _audioManager.ResumeAll();
        _backgroundParticlesBright.Play();
        _backgroundParticles.Play();
        if (_magnetismParticles.isPaused) _magnetismParticles.Play();
        if (_projectilesParticles.isPaused) _projectilesParticles.Play();
        if (_playerDebrisParticles.isPaused) _playerDebrisParticles.Play();
        if (_playerExplosionParticles.isPaused)
            _playerExplosionParticles.Play();
        ToggleInstructions();
        _pauseMenuAnim.Play("ResumeAnimation");
    }

    private void Pause() {
        _audioManager.PauseAll();
        _backgroundParticlesBright.Pause();
        _backgroundParticles.Pause();
        if (_magnetismParticles.isPlaying) _magnetismParticles.Pause();
        if (_projectilesParticles.isPlaying) _projectilesParticles.Pause();
        if (_playerDebrisParticles.isPlaying) _playerDebrisParticles.Pause();
        if (_playerExplosionParticles.isPlaying)
            _playerExplosionParticles.Pause();
        _pauseMenuAnim.Play("PauseAnimation");
        ToggleInstructions();
    }

    private void EscKeyPressed() {
        if (!_isEscKeyReleased) return;
        _audioManager.Play("Tap");
        _isEscKeyReleased = false;
        if (IsPaused) Resume();
        else Pause();
    }

    private void EscKeyReleased() {
        _isEscKeyReleased = true;
    }

    private void Update() {
        if (Input.GetKey(KeyCode.Escape)) {
            EscKeyPressed();
        }

        if (Input.GetKeyUp(KeyCode.Escape)) {
            EscKeyReleased();
        }

        if (Broadcasts.Count == 0) return;
        foreach (var broadcast in Broadcasts) {
            if (broadcast == null) continue;
            if (broadcast.GetComponent<Animator>()
                .GetCurrentAnimatorStateInfo(0).IsName("End")) {
                Destroy(broadcast);
            }
        }

        Broadcasts.RemoveAll(broadcast => broadcast == null);
    }

//    private void OnApplicationPause(bool pauseStatus) {
//        IsPaused = pauseStatus;
////        ToggleInstructions();
//        if (!IsPaused) Pause();
//        else Resume();
//    }
}