using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class RatManager : MonoBehaviour
{
    public int nCommonRatNormal = 50;
    public int nCommonRatTank = 50;
    public int nSupportRatNormal = 50;
    public int nSupportRatTank = 50;
    public int nShooterRatNormal = 50;
    public int nShooterRatTank = 50;

    public nBasicRatn[] poolOfRats = new nBasicRatn[300];

    //Se pasan los prefabs al manager para hacer el pool con los 3 tipos de ratas (y sus variaciones normal y tanque)

    [SerializeField] public CommonRat commonRatTank; 
    [SerializeField] public CommonRat commonRatNormal;

    [SerializeField] public SupportRat supportRatTank;
    [SerializeField] public SupportRat supportRatNormal;

    [SerializeField] public ShooterRat shooterRatTank;
    [SerializeField] public ShooterRat shooterRatNormal;

    /*public BasicRat Clone(bool tank, BasicRat tankRat, BasicRat normalRat)
    {
        BasicRat rat = tank == true ? Instantiate(tankRat) : Instantiate(normalRat); //Si requerimos de utilizar el patrón Prototype para crear una rata tanque, tan solo debemos habilitar al clonar tank = true

        return rat;
    }*/


    private void Start()
    {
        for(int i = 0; i < nCommonRatNormal*6; i++) 
        {
            if (i <= nCommonRatNormal - 1) // Menor a 50 las ratas son comunes y normales
            {
                poolOfRats[i] = (CommonRat)commonRatNormal.Clone();
            }else if (i > nCommonRatNormal-1 && i <= nCommonRatNormal * 2 - 1) //Mayor a 50 y menor a 100 son ratas comunes y tanque
            {
                poolOfRats[i] = (CommonRat)commonRatTank.Clone();
            }else if(i > nCommonRatNormal * 2 - 1 && i <= nCommonRatNormal * 3 - 1) //Mayor a 100 y menor a 150 son ratas support y normales
            {
                poolOfRats[i] = (SupportRat)supportRatNormal.Clone();
            }else if(i > nCommonRatNormal * 3 - 1 && i <= nCommonRatNormal * 4 - 1) //Mayor a 150 y menor a 200 son ratas support y tanques
            {
                poolOfRats[i] = (SupportRat)supportRatTank.Clone();
            }else if(i > nCommonRatNormal * 4 - 1 && i <= nCommonRatNormal * 5 - 1) //Mayor a 200 y menor a 250 son ratas shooter normales
            {
                poolOfRats[i] = (ShooterRat)shooterRatNormal.Clone();
            }else if (i > nCommonRatNormal * 5 - 1) //Mayor a 250 son ratas shooters tanques
            {
                poolOfRats[i] = (ShooterRat)shooterRatTank.Clone();
            }
        }
    }
}
