using UnityEngine;

public class CommonRat : nBasicRatn
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public float attackDamage = 5f;
    //public float attackRange = 2f;
    public float criticProbability = 0.2f;

    void Start()
    {
        attackRange = 2f;
    }

    public void Melee()
    {
        if (distanceToPlayer <= attackRange)
        {
            animator.SetBool("isAttacking", true);
            if (RollDice(criticProbability) == true)
            {
                float criticImpact = Random.Range(1f, 2f);
                attackDamage *= criticImpact;
            }
            else attackDamage = 5f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
