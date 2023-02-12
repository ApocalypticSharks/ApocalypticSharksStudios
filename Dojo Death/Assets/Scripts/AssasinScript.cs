using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssasinScript : MonoBehaviour
{
    private GameObject character;
    [SerializeField] private Object weapon;
    void Start()
    {
        character = GameObject.Find("Master");
        StartCoroutine(Throw());
    }

    IEnumerator Throw()
    {
        this.gameObject.GetComponent<Renderer>().material.color = new Color(0, 255, 0);
        yield return new WaitForSeconds(0.3f);
        GameObject projectile1 = Instantiate(
            weapon,
            transform.position,
            Quaternion.FromToRotation(transform.position, character.transform.position)) as GameObject;
        this.gameObject.GetComponent<Renderer>().material.color = new Color(255, 255, 255);
        yield return new WaitForSeconds(0.1f);
        GameObject projectile2 = Instantiate(
            weapon,
            transform.position,
            Quaternion.FromToRotation(transform.position, character.transform.position)) as GameObject;
        yield return new WaitForSeconds(0.1f);
        GameObject projectile3 = Instantiate(
            weapon,
            transform.position,
            Quaternion.FromToRotation(transform.position, character.transform.position)) as GameObject;
        Destroy(this.gameObject);
    }
}
