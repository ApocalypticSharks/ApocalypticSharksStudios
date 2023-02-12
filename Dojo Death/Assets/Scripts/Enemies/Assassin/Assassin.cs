using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assassin : MonoBehaviour, IEnemyInterface
{
    private GameObject kunai, instance;
    private AssassinPresenter assassinPresenter;
    private Enemy enemy;
    private float damage = 15, minAttackSpeed = 2.5f, maxAttackSpeed = 2.5f, attackPreparedTimer = 0.5f;

    public GameObject enemyInstance
    {
        get => instance;
        set => instance = value;
    }
    public Assassin()
    {
        enemy = new Enemy(damage, minAttackSpeed, maxAttackSpeed, attackPreparedTimer);

        instance = Instantiate(Resources.Load("Assassin"), Vector3.zero, Quaternion.identity) as GameObject;
        kunai = Resources.Load("Projectiles/Kunai") as GameObject;
        kunai.GetComponent<KunaiScript>().damage = damage;
        var assassinView = instance.GetComponent<AssassinView>();

        assassinPresenter = new AssassinPresenter(assassinView, enemy, kunai);

        assassinView.assassinPresenter = assassinPresenter;
        instance.SetActive(false);
    }

    public void Activate(Transform position)
    {
        instance.SetActive(true);
        instance.transform.SetParent(position);
        instance.transform.position = position.position;
    }
}
