using UnityEngine;

public class ShopItem : MonoBehaviour
{
    public int pointsNeeded;

    Color originalColor;
    Color color;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalColor = gameObject.GetComponent<Renderer>().material.color;
        color = gameObject.GetComponent<Renderer>().material.color * 1.5f;
    }

    public void OnMouseOver()
    {
        gameObject.GetComponent<Renderer>().material.color = color;
        gameObject.transform.localScale *= 1.2f;
    }

    public void OnMouseExit()
    {
        gameObject.GetComponent<Renderer>().material.color = originalColor;
        gameObject.transform.localScale /= 1.2f;
    }
}
