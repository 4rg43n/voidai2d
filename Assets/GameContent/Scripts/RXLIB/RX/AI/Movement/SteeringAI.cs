using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SteeringAI
{
    public bool debug = false;

    public float lookAheadDist=3;
    public float castRadius = 0.05f;
    public LayerMask steeringMask = ~0;
    public float turnTime = 1;

    public string lastHit;

    public bool TurnTo(Vector3 src, Vector3 dst)
    {
        Vector3 fwd = (dst - src).normalized;
        Ray ray = new Ray(src + (-fwd * castRadius), fwd);
        RaycastHit hit;

        if (Physics.SphereCast(ray, castRadius, out hit, lookAheadDist, steeringMask))
        {
            if (debug)
            {
                Debug.DrawLine(hit.point, hit.point + hit.normal * 2, Color.yellow);
                lastHit = hit.transform.gameObject.name;
                Debug.Break();
            }

            return false;
        }

        return true;
    }

    public bool Steer(Vector3 src, Vector3 fwd, out Vector3 ret)
    {
        float findOffset = castRadius * 2;
        Ray ray = new Ray(src + (-fwd * findOffset), fwd);
        RaycastHit hit;

        if (Physics.SphereCast(ray, castRadius, out hit, lookAheadDist+ findOffset, steeringMask))
        {
            float angle = Vector3.SignedAngle(fwd, hit.normal, Vector3.up);
            if (angle < 0)
                angle = 90;
            else
                angle = -90;
            Vector3 ndir = Quaternion.Euler(0, angle, 0) * hit.normal;

            ret = (fwd + ndir).normalized;

            if (debug)
            {
                Debug.DrawLine(hit.point, hit.point + hit.normal * 2, Color.red);
                Debug.DrawLine(hit.point, hit.point + ndir * 2, Color.green);
                lastHit = hit.transform.name;
                Debug.Break();
            }

            return true;
        }
        else
        {
            if (debug)
            {
                Debug.DrawLine(src, src + fwd * lookAheadDist, Color.green);
                lastHit = "None";
                Debug.Break();
            }

            ret = fwd;
            return false;
        }
    }
}
