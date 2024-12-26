using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadBobController : MonoBehaviour
{
    [SerializeField] private bool _enable = true;

    [SerializeField, Range(0, 0.1f)] private float amplitude = 0.015f;
    [SerializeField, Range(0, 30f)] private float frequency = 10.0f;

    [SerializeField] private Transform _camera = null;
    [SerializeField] private Transform _cameraHolder = null;

    private float toggleSpeed = 3.0f;
    private Vector3 startPos;
    private PlayerMovement playerMovement;
    private PlayerEffectsHandler playerEffectsHandler;

    private bool movingRight = true; // Tracks the current direction of motion
    private float lastStepTime = 0f;
    private float savedAmplitude;
    private float sprintAmplitude;
    private float savedFrequency;
    private float sprintFrequency;

    private void Awake()
    {
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        playerEffectsHandler = FindAnyObjectByType<PlayerEffectsHandler>();
        startPos = _camera.localPosition;
    }

    private void Start()
    {
        CalculateSprintVariants();
    }

    private void CalculateSprintVariants()
    {
        savedAmplitude = amplitude;
        savedFrequency = frequency;
        sprintAmplitude = amplitude + 0.01f;
        sprintFrequency = frequency + 3f;
    }

    void Update()
    {
        if (!_enable) return;
        if (PauseScript.isPaused) return;
        SprintCheck(playerMovement.isSprinting);
        CheckMotion();
        if (playerMovement.GetVelocity() != Vector3.zero)
        {
            ResetPosition();
        }
        else if (playerMovement.GetVelocity() == Vector3.zero)
        {
            FastResetPosition();
        }
    }

    private void PlayMotion(Vector3 motion)
    {
        _camera.localPosition += motion;
    }

    private void CheckMotion()
    {
        float speed = new Vector3(playerMovement.GetVelocity().x, 0, playerMovement.GetVelocity().z).magnitude;

        if (speed < toggleSpeed) return;

        Vector3 motion = FootStepMotion();
        PlayMotion(motion);
        TriggerFootstepSound(motion);
    }

    private Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y += Mathf.Sin(Time.time * frequency) * amplitude;
        pos.x += Mathf.Cos(Time.time * frequency / 2) * amplitude;
        return pos;
    }

    private void ResetPosition()
    {
        if (_camera.localPosition == startPos) return;
        _camera.localPosition = Vector3.Lerp(_camera.localPosition, startPos, 3 * Time.deltaTime);
    }

    private void FastResetPosition()
    {
        if (_camera.localPosition == startPos) return;
        _camera.localPosition = Vector3.Lerp(_camera.localPosition, startPos, 10 * Time.deltaTime);
    }

    private void TriggerFootstepSound(Vector3 motion)
    {
        // Determine current motion direction (right or left based on x-axis)
        bool isMovingRight = motion.x > 0;

        // Play sound only when direction changes
        if (isMovingRight != movingRight)
        {
            movingRight = isMovingRight;

            // Ensure the sound doesn't play too frequently
            if (Time.time - lastStepTime > (1 / frequency))
            {
                lastStepTime = Time.time;

                if (playerEffectsHandler != null)
                {
                    playerEffectsHandler.PlayFootstepSound();
                }
            }
        }
    }

    public void SprintCheck(bool isSprinting)
    {
        if (isSprinting) 
        {
           frequency = sprintFrequency;
            amplitude = sprintAmplitude;
        }
        else 
        {
            frequency = savedFrequency;
            amplitude = savedAmplitude;
        }
    }
}
