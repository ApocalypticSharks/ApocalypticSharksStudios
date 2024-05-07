using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class CameraFollowScript : MonoBehaviour
{
    public Transform player;
    [SerializeField]private Animator animator;
    public bool flyMode;
    //[SerializeField] private float maxHeight, minHeight;

    private void Update()
    {
        if (!flyMode)
            NormalModeFollow();
        else
            FlyModeFollow();
    }

    public void ChangeCameraSizeOut(params float[] forces)
    {
        animator.SetTrigger("isFlying");
    }

    public void ChangeCameraSizeIn()
    {
        animator.SetTrigger("notFlying");
    }

    public void SetCameraToPlayPosition(InputAction.CallbackContext context)
    {
        transform.position = new Vector3(0, 0, -1);
        UnityEngine.Camera.main.orthographicSize = 6.5f;
    }

    private void NormalModeFollow()
    {
        if (player && math.abs(transform.position.y - player.position.y) >= 2)
        {
            Vector3 newPositionY = transform.position;
            newPositionY.y = Mathf.Lerp(transform.position.y, player.transform.position.y + 2, 2f * Time.deltaTime);
            transform.position = newPositionY;
        }
    }

    private void FlyModeFollow()
    {
        Vector3 newPositionY = transform.position;
        newPositionY.y = player.transform.position.y + 2;
        transform.position = newPositionY;
    }

    public void ChangeFollowMode()
    {
        flyMode = !flyMode;
    }

    public void ChangeFollowMode(params float[] forces)
    {
        flyMode = !flyMode;
    }

    private void OnFadeOutFinished()
    {
        SceneManager.LoadScene(1);
    }
}
