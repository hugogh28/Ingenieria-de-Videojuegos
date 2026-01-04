using UnityEngine;

public class CommonRat : nBasicRatn
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] CommonRat commonRatTank;
    [SerializeField] CommonRat commonRatNormal;


    public float attackDamage = 5f;
    //public float attackRange = 2f;
    public float criticProbability = 0.05f;

    

    void Start()
    {
        attackRange = 2f;
    }

    public CommonRat Clone(bool tank)
    {
        CommonRat rat = tank == true ? Instantiate(commonRatTank) : Instantiate(commonRatNormal); //Si requerimos de utilizar el patrón Prototype para crear una rata tanque, tan solo debemos habilitar al clonar tank = true
        
        return rat;
    }

    public void Melee()
    {
        if (distanceToPlayer <= attackRange)
        {
            animator.SetBool("isAttacking", true);
            if (RollDice(criticProbability) == true)
            {
                float criticImpact = Random.Range(1f, 2f);
                attackDamage *= criticImpact;//Añadir indicador de crítico
            }
            else attackDamage = 5f;
            //Aplicar daño al jugador
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
