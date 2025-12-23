using UnityEngine;

public class Page : MonoBehaviour
{
    public GameObject canvasFront;
    public GameObject canvasBack;

    public void SetFront(bool active)
    {
        canvasFront.SetActive(active);
    }

    public void SetBack(bool active)
    {
        canvasBack.SetActive(active);
    }

    public void SetBoth(bool active)
    {
        canvasFront.SetActive(active);
        canvasBack.SetActive(active);
    }
}
