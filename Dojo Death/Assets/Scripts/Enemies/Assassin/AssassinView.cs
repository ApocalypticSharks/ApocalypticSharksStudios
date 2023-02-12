using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssassinView : MonoBehaviour
{
    public AssassinPresenter assassinPresenter;

    private void OnEnable()
    {
        assassinPresenter?.Enable();
    }
    private void Update()
    {
        assassinPresenter.DoSlash();
    }

    public void TakeDamage(float health)
    {

    }

    // Update is called once per frame
    public void Die()
    {
        transform.parent = null;
        gameObject.SetActive(false);
    }

    public void AttackPrepared()
    {
        this.gameObject.GetComponent<Renderer>().material.color = new Color(0, 255, 0);
    }

    public void Slash()
    {
        this.gameObject.GetComponent<Renderer>().material.color = new Color(255, 255, 255);
    }
}
