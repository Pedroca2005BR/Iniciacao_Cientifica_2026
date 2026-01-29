using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HingeJointHelper : MonoBehaviour
{
    public List<HingeJoint> joints;

    public void AddHingeJoints()
    {
        var hingeJoint = gameObject.AddComponent<HingeJoint>();
        joints.Add(hingeJoint);
        var rb = gameObject.GetComponent<Rigidbody>();
        AddHingeJointToChild(transform, rb);
    }

    private void AddHingeJointToChild(Transform transformToAdd, Rigidbody rb)
    {
        foreach (Transform child in transformToAdd)
        {
            HingeJoint hingeJoint = child.gameObject.AddComponent<HingeJoint>();
            joints.Add(hingeJoint);
            var childRb = child.gameObject.GetComponent<Rigidbody>();
            hingeJoint.connectedBody = rb;
            AddHingeJointToChild(child, childRb);
        }
    }

    public void DeleteHingeJoints()
    {
        DestroyImmediate(gameObject.GetComponent<HingeJoint>());
        DestroyImmediate(gameObject.GetComponent<Rigidbody>());
        DeleteHingeJointForChild(transform);
        joints.Clear();
    }

    private void DeleteHingeJointForChild(Transform transformToDelete)
    {
        foreach (Transform child in transformToDelete)
        {

            DestroyImmediate(child.gameObject.GetComponent<HingeJoint>());
            DestroyImmediate(child.gameObject.GetComponent<Rigidbody>());
            DeleteHingeJointForChild(child);
        }
    }
}