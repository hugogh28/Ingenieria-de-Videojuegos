using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float dmg;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Target"))
        {
            print("hit " + collision.gameObject.name + "!");

            createBulletImpactEffect(collision);

            Destroy(gameObject);
        }
        if (collision.gameObject.CompareTag("Wall"))
        {
            print("hit a Wall!");

            createBulletImpactEffect(collision);

            Destroy(gameObject);
        }
        if (collision.gameObject.CompareTag("Bottle"))
        {
            print("Hit a Bottle");

            collision.gameObject.GetComponent<ShatterDestruction>().Shatter();
        }
        if (collision.gameObject.CompareTag("Rat"))
        {
            collision.gameObject.GetComponent<nBasicRatn>().TakeDamage(dmg);

            Destroy(gameObject);
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
