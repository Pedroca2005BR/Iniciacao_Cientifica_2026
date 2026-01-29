using UnityEngine;

public static class ConnectorUtils
{
    public static void IgnoreAllCollisionsBetween(GameObject a, GameObject b, bool ignore)
    {
        var colA = a.GetComponentsInChildren<Collider>();
        var colB = b.GetComponentsInChildren<Collider>();
        foreach (var ca in colA)
            foreach (var cb in colB)
                Physics.IgnoreCollision(ca, cb, ignore);
    }

    public static void AttachObjects(GameObject objA, GameObject objB)
    {
        // objA ==> epi
        // objB ==> Phantom

        // Set posicoes
        objA.transform.SetPositionAndRotation(objB.transform.position, objB.transform.rotation);

        // Cria o joint em A
        FixedJoint joint = objA.AddComponent<FixedJoint>();

        // Conecta ao rigidbody de B
        joint.connectedBody = objB.GetComponent<Rigidbody>();

        // seta as ancoras
        joint.anchor = Vector3.zero;
        joint.connectedAnchor = Vector3.zero;

        
    }

    public static void DetachObject(GameObject objA)
    {
        int counter = 10;
        while (objA.TryGetComponent<FixedJoint>(out var joint) && counter > 0)
        {
            if (joint.connectedBody.gameObject.TryGetComponent<PhantomObjectController>(out var phantom))
            {
                phantom.ObjectConnected = null;
            }

            GameObject.Destroy(joint);
            counter--;
        }

        Debug.Log("Joint sumiu: " + (objA.GetComponent<FixedJoint>() == null));
    }
}
