using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterScript : MonoBehaviour
{
    public float damage = 25;
    public float rangeDamage = 25;
    public float hp;
    public int kunai;
    [SerializeField] private Object weapon;
    [SerializeField] private GameObject leftBalcony, rightBalcony;
    private Vector3 firstClick;

    private void Start()
    {
        hp = 100;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            firstClick = Input.mousePosition;
        else if (Input.GetMouseButtonUp(0))
        {
            if (Input.mousePosition.x > firstClick.x + 10)
            {
                if (Input.mousePosition.y > firstClick.y + 20)
                    Throw(rightBalcony.transform.position);
                else
                    Attack(0);
            }
            else if (Input.mousePosition.x < firstClick.x - 10)
            {
                if (Input.mousePosition.y > firstClick.y + 20)
                    Throw(leftBalcony.transform.position);
                else
                    Attack(180);
            }
            else
                CatchProjectile();
        }

        if (hp <= 0)
            Destroy(this.gameObject);
    }

    

    private void Attack(float rotation)
    {
        StartCoroutine(Slash(rotation));
    }

    private void Throw(Vector3 direction)
    {
        kunai--;
        GameObject projectile = Instantiate(
            weapon,
            transform.position,
            Quaternion.FromToRotation(transform.position, direction)) as GameObject;
        projectile.GetComponent<MasterKunaiScript>().direction = direction;
        projectile.GetComponent<MasterKunaiScript>().damage = rangeDamage;
        if (kunai == 0)
            StartCoroutine(KunaiReload());
    }

    private void CatchProjectile()
    {
        StartCoroutine(Catch());
    }

    IEnumerator Slash(float rotation)
    {
        transform.rotation = transform.rotation = Quaternion.Euler(0, rotation, 0);
        transform.GetChild(0).gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        transform.GetChild(0).gameObject.SetActive(false);
    }

    IEnumerator Catch()
    {
        transform.GetChild(1).gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        transform.GetChild(1).gameObject.SetActive(false);
    }

    IEnumerator KunaiReload()
    {
        yield return new WaitForSeconds(3);
        if (kunai < 3)
            kunai++;
    }
}
