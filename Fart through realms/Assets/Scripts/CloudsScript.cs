using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudsScript : MonoBehaviour
{
    [SerializeField] private GameObject levelToActivate, levelToDeactivate;
    [SerializeField] private Sprite coinSprite, head, hand, arm, leg, foot, body;
    public delegate void finishGame(Collider2D collision);
    public finishGame finish;
    [SerializeField] public List<AudioClip> LevelMusic;

    private void Awake()
    {
        finish += FinishGame;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.GetComponent<PlayerView>() != null)
        {
            if (levelToActivate && levelToDeactivate && coinSprite)
            { 
                ChangeLevel(collision);
                collision.GetComponent<SpriteRenderer>().sprite = body;
                collision.transform.Find("head").GetComponent<SpriteRenderer>().sprite = head;
                collision.transform.Find("armLeft").GetComponent<SpriteRenderer>().sprite = arm;
                collision.transform.Find("armLeft").GetChild(0).GetComponent<SpriteRenderer>().sprite = hand;
                collision.transform.Find("armRight").GetComponent<SpriteRenderer>().sprite = arm;
                collision.transform.Find("armRight").GetChild(0).GetComponent<SpriteRenderer>().sprite = hand;
                collision.transform.Find("legLeft").GetComponent<SpriteRenderer>().sprite = leg;
                collision.transform.Find("legLeft").GetChild(0).GetComponent<SpriteRenderer>().sprite = foot;
                collision.transform.Find("legRight").GetComponent<SpriteRenderer>().sprite = leg;
                collision.transform.Find("legRight").GetChild(0).GetComponent<SpriteRenderer>().sprite = foot;
                MusicSystemScript.instance.StopAllCoroutines();
                MusicSystemScript.instance.levelMusic = this.LevelMusic;
                MusicSystemScript.instance.StopAudioClip();
                MusicSystemScript.instance.PlayNextClip();
            }  
            else
                finish?.Invoke(collision);
        }
    }

    private void ChangeLevel(Collider2D collision)
    {
        levelToActivate.SetActive(true);
        levelToDeactivate.SetActive(false);
        var player = collision.transform.GetComponent<PlayerView>();
        if (player != null)
            player.controller.updateUI?.Invoke(coinSprite);
    }

    private void FinishGame(Collider2D collision)
    {
        var camera = UnityEngine.Camera.main;
        var cameraAnimator = camera.gameObject.GetComponent<Animator>();
        cameraAnimator.SetTrigger("gameFinished");
        collision.transform.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        collision.transform.GetComponent<ConstantForce2D>().relativeForce = new Vector3(0, 13, 0);
        collision.transform.rotation = Quaternion.Euler(0, 0, 0);

    }
}
