using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class ShooterRat : BasicRat
{
    public Transform shotPos;
    public float deviationRadius = 1.5f;
    public float maxDistanceHit = 50f;

    public void Shoot()
    {
        float dmg = attackDamage;
        if(RollDice(criticProbability) == true)
        {
            float criticImpact = UnityEngine.Random.Range(1f, 1.5f);
            dmg *= criticImpact;
        }

        //Trazamos una dirección para que la rata dispare dentro de un rango aleatorio al jugador
        Vector2 deviationCircle = UnityEngine.Random.insideUnitCircle * deviationRadius;
        Vector3 target = player.transform.position + shotPos.right * deviationCircle.x + shotPos.up * deviationCircle.y;
        Vector3 direction = (target - shotPos.position).normalized;

        if (Physics.Raycast(shotPos.position, direction, out RaycastHit hit, maxDistanceHit))
        {
            Debug.DrawRay(shotPos.position,direction);
            if (hit.collider.CompareTag("Player"))
            {
                GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().TakeDamage(dmg);
            }
        }
    }
}
