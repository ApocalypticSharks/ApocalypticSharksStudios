using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Controls : MonoBehaviour
{
    private PlayerInput _playerInput;
    private Vector2 _moveInput;
    [SerializeField] private float _speed;
    [SerializeField] private Rigidbody2D _rigidBody;
    [SerializeField] Animator _animator;
    private bool _isMoving, _isInteracting;
    private void Awake()
    {
        _playerInput = new PlayerInput();
    }
    private void OnEnable()
    {
        _playerInput.Enable();
        _playerInput.Player.Interact.performed += Interact;
        _playerInput.Player.NextLevel.performed += NextLevel;
    }
    private void OnDisable()
    {
        _playerInput.Disable();
    }
    // Update is called once per frame
    void Update()
    {
        if (!_isInteracting)
        {
            _moveInput = _playerInput.Player.Move.ReadValue<Vector2>();
            _rigidBody.velocity = _speed * _moveInput;
            if (_rigidBody.velocity.x > 0)
                transform.rotation = Quaternion.Euler(0, 180, 0);
            else if (_rigidBody.velocity.x < 0)
                transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        if (_rigidBody.velocity != Vector2.zero && !_isMoving)
        {
            _animator.SetTrigger("isMoving");
            _isMoving = true;
        }
        else if(_rigidBody.velocity == Vector2.zero && _isMoving)
        {
            _animator.SetTrigger("isStoped");
            _isMoving = false;
        }
    }

    private void Interact(InputAction.CallbackContext context)
    {
        var questSystem = GetComponent<QuestSystem>();
        if (questSystem.npcIsNear)
        {
            if (!_isInteracting)
            {
                _isInteracting = true;
                _rigidBody.velocity = Vector2.zero;
                questSystem.questTarget.GetComponent<DialogSystem>().Talk(questSystem, ref _isInteracting);
            }
            else
            {
                questSystem.questTarget.GetComponent<DialogSystem>().Talk(questSystem, ref _isInteracting);
            }
        }
    }
    private void NextLevel(InputAction.CallbackContext context)
    {
        var questSystem = GetComponent<QuestSystem>();
        if (questSystem.questsComplited)
        {
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}