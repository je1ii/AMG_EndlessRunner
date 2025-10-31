using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image primaryBar;   // instant bar (shows current HP)
    [SerializeField] private Image secondaryBar; // ghost/delayed bar

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f; // total hp of enemy

    [Header("Damage / Drain")]
    [Range(0.05f, 2f)]
    [SerializeField] private float timeToDrain = 0.2f; // seconds for ghost to catch up

    [Header("Regen")]
    [SerializeField] private float regenRate = 1f;   // hp per second
    [SerializeField] private float regenDelay = 2f;

    private float _currentHealth;
    private float _ghostHealth; // value shown by secondaryBar
    private float _drainTimer; // time to drain during draining
    private float _regenTimer; // idle timer until regen starts

    // drain start/target for smooth lerp
    private float _drainStartHealth;
    private float _drainTargetHealth;

    private enum State { Idle, Draining, Regenerating }
    private State _state = State.Idle;

    void Start()
    {
        // setting up default values
        _currentHealth = maxHealth;
        _ghostHealth = maxHealth;
        _drainTimer = 0f;
        _regenTimer = 0f;

        if (primaryBar != null) primaryBar.fillAmount = 1f;
        if (secondaryBar != null) secondaryBar.fillAmount = 1f;
    }

    void Update()
    {
        if (primaryBar == null || secondaryBar == null) return;

        var dt = Time.deltaTime;

        switch (_state)
        {
            // --- DAMAGE DRAIN ---
            case State.Draining: 
                _drainTimer += dt;
                var drainTime = Mathf.Clamp01(_drainTimer / Mathf.Max(0.0001f, timeToDrain));
                _ghostHealth = Mathf.Lerp(_drainStartHealth, _drainTargetHealth, drainTime);
                secondaryBar.fillAmount = _ghostHealth / maxHealth;

                // keep primary showing the immediate value
                primaryBar.fillAmount = _currentHealth / maxHealth;

                if (drainTime >= 1f)
                {
                    // finished draining -> go to idle and start regen timer
                    _state = State.Idle;
                    _regenTimer = 0f;
                }
                break;

            case State.Idle:
                // both bars show current amounts
                primaryBar.fillAmount = _currentHealth / maxHealth;
                secondaryBar.fillAmount = _ghostHealth / maxHealth;

                // only start regen after regenDelay has passed and if not full
                _regenTimer += dt;
                if (_regenTimer >= regenDelay && _currentHealth < maxHealth)
                {
                    _state = State.Regenerating;
                }
                break;

            case State.Regenerating:
                _currentHealth += regenRate * dt;
                _currentHealth = Mathf.Min(_currentHealth, maxHealth);

                // during regen, update both bars
                primaryBar.fillAmount = _currentHealth / maxHealth;
                _ghostHealth = _currentHealth;
                secondaryBar.fillAmount = _ghostHealth / maxHealth;
                
                // checks if hp is full and goes back to idle
                if (_currentHealth >= maxHealth - 0.001f)
                {
                    _currentHealth = maxHealth;
                    _state = State.Idle;
                    _regenTimer = 0f;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public void TakeDamage(float damage)
    {
        // checks if is already dead
        if (_currentHealth <= 0f) return;
        
        Debug.Log("Player Health: " + _currentHealth + " - " + damage);
        // apply damage
        _currentHealth -= damage;

        // update primary immediately
        primaryBar.fillAmount = _currentHealth / maxHealth;

        // setup drain from current ghost value toward the new health
        _drainStartHealth = _ghostHealth;
        _drainTargetHealth = _currentHealth;
        _drainTimer = 0f;
        _state = State.Draining;

        // reset regen timer so regen waits again after this damage
        _regenTimer = 0f;

        // if died, notify parent
        if (_currentHealth <= 0f)
        {
            var player = GetComponentInParent<Player>();
            if (player != null) player.PlayerDied();
        }
    }
}
