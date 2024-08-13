using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public delegate void onDeliveredEvent();
    public onDeliveredEvent onDelivered;
    public delegate void onClientTakenEvent();
    public onClientTakenEvent onClientTaken;
    public delegate void onObstacleCollisionEvent();
    public onObstacleCollisionEvent onObstacleCollision;

    [SerializeField] private PrometeoCarController carController;
    [SerializeField] public int _carHealthPoints;
    [SerializeField] private Game game;

    private void Awake()
    {
        _carHealthPoints = 100;

        onObstacleCollision += GetDamage;
        onDelivered += GetMoney;
        onDelivered += GetDelivered;

        if (game)
        {
            game._carStats = this;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Client")
        {
            onClientTaken?.Invoke();
        }
        if (other.tag == "Destination")
        { 
            onDelivered?.Invoke();
            UIDataScript.instance.UpdateUIData();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 3)
        {
            onObstacleCollision?.Invoke();
        }
    }

    private void GetDamage()
    {
        _carHealthPoints -= Mathf.RoundToInt(Mathf.Abs(carController.carSpeed) * 0.8f);
        UIDataScript.instance.UpdateHealth(_carHealthPoints);
    }
    private void GetMoney()
    {
        Progress.instance.savedGameData.coins += 10 + game.bonusCoins;
        game.bonusCoins += 2;
        Progress.instance.SaveData();
    }
    private void GetDelivered()
    {
        Progress.instance.savedGameData.clients += 1;
    }
}
