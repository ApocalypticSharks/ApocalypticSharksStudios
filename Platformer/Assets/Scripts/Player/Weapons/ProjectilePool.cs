using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public List<GameObject> lightBlaster = new List<GameObject>();
    public List<GameObject> ricochet = new List<GameObject>();
    public List<GameObject> ray = new List<GameObject>();

    private void Awake()
    {
        do
        {
            GameObject lightBlasterProjectile = Instantiate(Resources.Load("Weapons/LightBlaster") as GameObject);
            lightBlasterProjectile.transform.SetParent(GameObject.Find("LightBlaster").transform);
            lightBlaster.Add(lightBlasterProjectile);
        } 
        while (lightBlaster.Count < 10);


    }

    public GameObject ActivateProjectile(List<GameObject> projectileType)
    {
        GameObject projectile = projectileType.Find(p => !p.activeSelf);
        projectile.SetActive(true); 
        projectile.transform.position = transform.position;
        return projectile;
    }
}
