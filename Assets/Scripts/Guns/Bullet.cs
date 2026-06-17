using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float dmg;
    public float criticalChance;
    public float criticalMultiplier = 1.75f;
    public int pointsGivenOnRatHit = 10;

    private void OnCollisionEnter(Collision collision)
    {
        BasicRat rat = collision.gameObject.GetComponentInParent<BasicRat>();

        if (rat != null)
        {
            bool isCritical = Random.value < criticalChance;
            float finalDamage = isCritical ? dmg * criticalMultiplier : dmg;
            Vector3 hitPoint = collision.contacts.Length > 0
                ? collision.contacts[0].point
                : collision.transform.position;

            rat.TakeDamage(finalDamage, isCritical, hitPoint);
            GivePointsForRatHit();

            Destroy(gameObject);
            return;
        }

        if (collision.gameObject.CompareTag("Target"))
        {
            print("hit " + collision.gameObject.name + "!");

            createBulletImpactEffect(collision);

            Destroy(gameObject);
            return;
        }
        if (collision.gameObject.CompareTag("Wall"))
        {
            print("hit a Wall!");

            createBulletImpactEffect(collision);

            Destroy(gameObject);
            return;
        }
        if (collision.gameObject.CompareTag("Bottle"))
        {
            print("Hit a Bottle");

            collision.gameObject.GetComponent<ShatterDestruction>().Shatter();

            Destroy(gameObject);
        }
    }

    private void GivePointsForRatHit()
    {
        if (pointsGivenOnRatHit <= 0)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            return;
        }

        PlayerController player = playerObject.GetComponent<PlayerController>();

        if (player == null)
        {
            player = playerObject.GetComponentInParent<PlayerController>();
        }

        if (player == null)
        {
            player = playerObject.GetComponentInChildren<PlayerController>();
        }

        if (player != null)
        {
            player.AddPoints(pointsGivenOnRatHit);
        }
    }

    void createBulletImpactEffect(Collision objHit)
    {
        ContactPoint contact = objHit.contacts[0];

        GameObject hole = Instantiate(
            GlobalReferences.Instance.bulletImpactEffectPrefab,
            contact.point,
            Quaternion.LookRotation(contact.normal)
            );

        hole.transform.SetParent(objHit.gameObject.transform);
    }


}
