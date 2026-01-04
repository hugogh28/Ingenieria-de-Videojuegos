using UnityEngine;

public class ShooterRat : nBasicRatn
{
    [SerializeField] private ShooterRat shooterRatTank;
    [SerializeField] private ShooterRat shooterRatNormal;

    

    public float shootinRange = 12f;
    public float bulletDamage = 20f;
    public float fireRate = 1f;
    public int ammo = 20;
    public float critic = 0.01f;

    //Podría ser interesante hacer un ataque especial, o que se meta más daño por un crítico a la rata

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        attackRange = shootinRange;
    }

    public ShooterRat Clone(bool tank)
    {
        ShooterRat rat = tank == true ? Instantiate(shooterRatTank) : Instantiate(shooterRatNormal); //Si requerimos de utilizar el patrón Prototype para crear una rata tanque, tan solo debemos habilitar al clonar tank = true
        return rat;
    }

    public void Shoot()//Añadir delay y un cargador de 20 de munición (puedes copiar el código de las armas)
    {
        if (ShouldStop()==true)
        {
            animator.SetBool("isShooting", true);
            if (RollDice(critic) == true)//Hay cierta probabilidad de recibir un daño crítico
            {
                float criticImpact = Random.Range(1f, 1.5f);
                bulletDamage *= criticImpact;//Añadir indicador de crítico
            }
            else bulletDamage = 20f;
            //Aplicar daño al jugador
        }
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/
}
