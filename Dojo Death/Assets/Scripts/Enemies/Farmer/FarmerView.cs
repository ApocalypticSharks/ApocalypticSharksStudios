
using UnityEngine;

public class FarmerView : MonoBehaviour
{
    public FarmerPresenter farmerPresenter;
    public Animator animator;
    // Start is called before the first frame update

    private void OnEnable()
    {
        farmerPresenter?.Enable();
    }
    private void Update()
    {
        farmerPresenter.SlashTimerTick();
        farmerPresenter.StunCheck();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "hitbox")
            farmerPresenter.Hit();
    }
    public void TakeDamage(float health)
    {
        
    }
    public void Die()
    {
        farmerPresenter.Disable();
        transform.parent = null;
        gameObject.SetActive(false);
    }

    public void AttackPrepared()
    {
        farmerPresenter.AttackPrepared();
    }

    public void DoSlash()
    {
        farmerPresenter.DoSlash();
    }
}
