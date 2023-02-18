using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssassinPresenter : MonoBehaviour
{
    private Enemy enemy;
    private AssassinView assassin;
    private GameObject kunai;

    public AssassinPresenter(AssassinView assassin, Enemy enemy, GameObject kunai)
    {
        this.assassin = assassin;
        this.enemy = enemy;
        this.kunai = kunai;
    }

    public void Enable()
    {
        enemy.slashHandler += Slash;
    }

    public void DoSlash()
    {
        enemy.Slash();
    }

    private void Slash(float damage, ref float attackSpeed, float deffaultAttackSpeed, ref float attackPreparedTimer, ref bool attackPrepared)
    {
        if (attackSpeed > 0)
            attackSpeed -= Time.deltaTime;
        else
        {
            if (!attackPrepared)
                assassin.animator.SetTrigger("Attack");
            attackPrepared = true;
            if (attackPreparedTimer > 0)
                attackPreparedTimer -= Time.deltaTime;
            else
            {
                assassin.Slash();
                assassin.StartCoroutine(ThrowKunai());
                attackSpeed = deffaultAttackSpeed;
                attackPreparedTimer = 0.5f;
            }
        }
    }

    IEnumerator ThrowKunai()
    {
        Debug.Log("Here");
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.1f);
            GameObject kunaiThrown = Instantiate(kunai, assassin.transform.position, Quaternion.identity) as GameObject;
        }
        Disable();
    }

    public void Disable()
    {
        enemy.slashHandler -= Slash;
    }
}
