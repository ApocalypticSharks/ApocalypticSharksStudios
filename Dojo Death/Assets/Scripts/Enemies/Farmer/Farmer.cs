using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farmer : MonoBehaviour, IEnemyInterface
{
    public GameObject instance;
    private MasterScript master;
    private FarmerPresenter farmerPresenter;
    private Enemy enemy;
    private float damage = 15, hp = 25, minAttackSpeed = 2, maxAttackSpeed =3.5f, attackPreparedTimer = 0.5f;

    public GameObject enemyInstance
    {
        get => instance;
        set => instance = value;
    }

    public Farmer()
    {
        enemy = new Enemy(damage, hp, minAttackSpeed, maxAttackSpeed, attackPreparedTimer);

        instance = Instantiate(Resources.Load("Farmer"), Vector3.zero, Quaternion.identity) as GameObject;

        master = GameObject.Find("Master").GetComponent<MasterScript>();
        var farmerView = instance.GetComponent<FarmerView>(); 

        farmerPresenter = new FarmerPresenter(farmerView, enemy, master);

        farmerView.farmerPresenter = farmerPresenter;
        instance.SetActive(false);
    }

    public void Activate(Transform position)
    {
        instance.SetActive(true);
        instance.transform.SetParent(position);
        instance.transform.position = position.position;
    }
}
