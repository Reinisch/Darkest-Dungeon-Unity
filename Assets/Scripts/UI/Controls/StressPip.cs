using UnityEngine;
using System.Collections;

public class StressPip : MonoBehaviour
{
    private Animator stressController;
    public Animator StressController
    {
        get
        {
            if(stressController == null)
                stressController = GetComponent<Animator>();

            return stressController;
        }
    }
}
