using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enterance : MonoBehaviour
{
    [SerializeField]private int realm;
    [SerializeField] Transform spawnPoint;
    private Player player;
    private Level level;
    private PlayerUI ui;
    private Camera camera;
    private void Awake()
    {
        level = new Level(realm);
        ui = new PlayerUI(realm);
        player = new Player(realm, spawnPoint);
        player.playerController.getCoin += ui.playerUIController.CountCoins;
        player.playerController.updateUI += ui.playerUIController.SetCoinCounterImage;
        camera = new Camera(player.instance.transform.Find("body"));
        player.playerController.getBeaned += camera.instance.GetComponent<CameraFollowScript>().ChangeCameraSizeOut;
        player.playerController.getUnbeaned += camera.instance.GetComponent<CameraFollowScript>().ChangeCameraSizeIn;
        player.playerController.getBeaned += camera.instance.GetComponent<CameraFollowScript>().ChangeFollowMode;
        player.playerController.getUnbeaned += camera.instance.GetComponent<CameraFollowScript>().ChangeFollowMode;
    }
}
