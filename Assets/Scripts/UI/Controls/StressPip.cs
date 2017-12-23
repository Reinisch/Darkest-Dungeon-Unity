using UnityEngine;

public class StressPip : MonoBehaviour
{
    private Animator stressController;

    public Animator StressController
    {
        get { return stressController ?? (stressController = GetComponent<Animator>()); }
    }
}
