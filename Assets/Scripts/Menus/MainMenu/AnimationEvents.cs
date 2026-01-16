using System.Data;
using Unity.VisualScripting;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    public Animator camera;

    public void TriggerCameraEvent()
    {
        camera.SetTrigger("toCredits");
    }
}
