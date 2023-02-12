using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thrower : MonoBehaviour, IEnemyInterface
{
    private GameObject kunai, instance;
    private ThrowerPresenter throwerPresenter;
    private Enemy enemy;
    private float damage = 15, hp = 25, minAttackSpeed = 2.5f, maxAttackSpeed = 4, attackPreparedTimer = 0.5f;

    public GameObject enemyInstance
    {
        get => instance;
        set => instance = value;
    }

    public Thrower()
    {
        enemy = new Enemy(damage, hp, minAttackSpeed, maxAttackSpeed, attackPreparedTimer);

        instance = Instantiate(Resources.Load("Thrower"), Vector3.zero, Quaternion.identity) as GameObject;
        kunai = Resources.Load("Projectiles/Kunai") as GameObject;
        kunai.GetComponent<KunaiScript>().damage = damage;
        var throwerView = instance.GetComponent<ThrowerView>();

        throwerPresenter = new ThrowerPresenter(throwerView, enemy, kunai);

        throwerView.throwerPresenter = throwerPresenter;
        instance.SetActive(false);
    }
    public void Activate(Transform position)
    {
        instance.SetActive(true);
        instance.transform.SetParent(position);
        instance.transform.position = position.position;
    }
}
