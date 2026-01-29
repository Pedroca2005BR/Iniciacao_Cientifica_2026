using UnityEngine;
using System.Collections.Generic;

public class LadderController : MonoBehaviour
{
    List<CapsuleCollider> barColliders = new List<CapsuleCollider>();
    List<bool> lockedBars = new List<bool>();

    [Header("Climb Limiter")]
    [SerializeField] int rangeOfMotionInBars = 1;

    private void Start()
    {
        int ct = transform.childCount;

        for (int i = 0; i < ct; i++)
        {
            barColliders.Add(transform.GetChild(i).GetComponent<CapsuleCollider>());
            lockedBars.Add(false);
        }
    }

    public void SetAvailableBars(int index)
    {
        if (index > barColliders.Count || index < 0)
        {
            Debug.LogError("Index out of range. Not enough horizontal bars!", this);
            return;
        }
        
        int lowerLimit = index - rangeOfMotionInBars, upperLimit = index + rangeOfMotionInBars;
        if (lowerLimit < 0)
        {
            lowerLimit = 0;
        }
        if (upperLimit >= barColliders.Count)
        {
            upperLimit = barColliders.Count - 1;
        }

        for(int i = lowerLimit;i <= upperLimit;i++)
        {
            //if (barColliders[i].enabled && lockedBars[i])
            //{

            //}
        }
    }
}
