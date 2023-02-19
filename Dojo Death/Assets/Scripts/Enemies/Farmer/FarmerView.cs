
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
        farmerPresenter.DoSlash();
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
        this.gameObject.GetComponent<Renderer>().material.color = new Color(0, 255, 0);
    }

    public void Slash()
    {
        this.gameObject.GetComponent<Renderer>().material.color = new Color(255, 255, 255);
    }
}
