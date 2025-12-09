using UnityEngine;

public class BasicRat : MonoBehaviour
{
    [SerializeField] private float health = 100;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(float dmg)
    {
        // provisional
        health -= dmg;// ReactionManager.Instance.calcDamage(dmg);

        if (health <= 0)
        {
            animator.SetTrigger("die");
            Destroy(gameObject);
        }
        else
        {
            animator.SetTrigger("damage");
        }
    }
}
