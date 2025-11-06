using UnityEngine;
using UnityEngine.Events;

public class UnityEventController : MonoBehaviour
{
    public string tagName;

    public UnityEvent onTriggerEnter, onTriggerExit;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagName))
        {
            onTriggerEnter.Invoke();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(tagName))
        {
            onTriggerExit.Invoke();
        }
    }
}
