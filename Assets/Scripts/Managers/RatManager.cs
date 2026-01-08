using UnityEngine;

public class RatManager : MonoBehaviour
{
    [SerializeField] public CommonRat commonRatTank;
    [SerializeField] public CommonRat commonRatNormal;

    [SerializeField] public SupportRat supportRatTank;
    [SerializeField] public SupportRat supportRatNormal;

    [SerializeField] public ShooterRat shooterRatTank;
    [SerializeField] public ShooterRat shooterRatNormal;

    public BasicRat Clone(bool tank, BasicRat tankRat, BasicRat normalRat)
    {
        BasicRat rat = tank == true ? Instantiate(tankRat) : Instantiate(normalRat); //Si requerimos de utilizar el patrón Prototype para crear una rata tanque, tan solo debemos habilitar al clonar tank = true

        return rat;
    }
}
