using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
	public float dmgValue = 4;
	public GameObject throwableObject;
	public Transform attackCheck;
	private Rigidbody2D m_Rigidbody2D;
	public Animator animator;
	public bool canAttack = true;
	public bool isTimeToCheck = false;
	public Transform collidingLight;
	private float shootCooldown;

	public GameObject cam;

	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		shootCooldown -= Time.deltaTime;
		if (Input.GetKeyDown(KeyCode.X) && canAttack)
		{
			canAttack = false;
			animator.SetBool("IsAttacking", true);
			StartCoroutine(AttackCooldown());
		}

		if (Input.GetKeyDown(KeyCode.V) && collidingLight != null && shootCooldown <= 0)
		{
			GameObject throwableWeapon = Instantiate(throwableObject, transform.position, Quaternion.identity) as GameObject;
			Vector2 direction = (transform.position - collidingLight.position).normalized;
			throwableWeapon.GetComponent<ThrowableWeapon>().direction = direction;
			throwableWeapon.name = "ThrowableWeapon";
			shootCooldown = 0.3f;
		}
	}

	IEnumerator AttackCooldown()
	{
		yield return new WaitForSeconds(0.5f);
		canAttack = true;
	}

	public void DoDashDamage()
	{
		dmgValue = Mathf.Abs(dmgValue);
		Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, 0.9f);
		for (int i = 0; i < collidersEnemies.Length; i++)
		{
			if (collidersEnemies[i].gameObject.tag == "Enemy")
			{
				if (collidersEnemies[i].transform.position.x - transform.position.x < 0)
				{
					dmgValue = -dmgValue;
				}
				collidersEnemies[i].gameObject.SendMessage("ApplyDamage", dmgValue);
				cam.GetComponent<CameraFollow>().ShakeCamera();
			}
		}
	}
}
