using System.Data;
using Unity.VisualScripting;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    public Animator fork;
    public ShatterDestruction bottle;

    public void TriggerBottleEvent()
    {
        bottle.Shatter();
    }

    public void TriggerForkEvent()
    {
        fork.SetTrigger("Fork");
    }
}
