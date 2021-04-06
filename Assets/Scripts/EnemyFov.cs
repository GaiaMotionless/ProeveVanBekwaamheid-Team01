using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFov : MonoBehaviour
{
    [SerializeField]
    private Transform player;

    [SerializeField]
    private Transform[] corpse,
                        enemies;

    [Range(0, 180)]
    [SerializeField]
    private float maxAngle, alertAngle;

    [Range(0, 20)]
    [SerializeField]
    private float maxRadius, alertRadius;

    [SerializeField]
    private float foundPlayerRadius,
                  foundPlayerAngle,
                  originalRadius,
                  originalAngle,
                  heightMultiplayer;

    public static bool isInFov = false;

    private bool checkObject;

    private bool inFOV = false,
                 inAlertFOV = false;

    private EnemyAI guardBehaviorScript;
    public void Awake()
    {
        guardBehaviorScript = GetComponent<EnemyAI>();
    }
    public void OnDrawGizmos()
    {
        if (isInFov && EnemyAI.foundPlayer)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, foundPlayerRadius);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, maxRadius);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, alertRadius);
        }

        Vector3 fovLine1 = Quaternion.AngleAxis(maxAngle, transform.up) * transform.forward * maxRadius;
        Vector3 fovLine2 = Quaternion.AngleAxis(-maxAngle, transform.up) * transform.forward * maxRadius;

        Vector3 fovLine3 = Quaternion.AngleAxis(alertAngle, transform.up) * -transform.forward * alertRadius;
        Vector3 fovLine4 = Quaternion.AngleAxis(-alertAngle, transform.up) * -transform.forward * alertRadius;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, fovLine1);
        Gizmos.DrawRay(transform.position, fovLine2);

        Gizmos.color = Color.gray;
        Gizmos.DrawRay(transform.position, fovLine3);
        Gizmos.DrawRay(transform.position, fovLine4);

        Gizmos.color = Color.red;
        if (isInFov)
            Gizmos.color = Color.green;
        if (!isInFov)
            Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, (player.position - transform.position).normalized * maxRadius);

        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, transform.forward * maxRadius);
        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, -transform.forward * alertRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position, transform.forward * 3.5f);
    }

    //check if the player is in the Fov
    public void InFOV(Transform checkingObject, Transform target, float maxAngle, float maxRadius, float alertRadius)
    {
        Vector3 directionBetween = (target.position - checkingObject.position).normalized;
        directionBetween.y *= 0;

        RaycastHit hit;

        //Max radius frontal vision AI
        if (Physics.Raycast(checkingObject.position + Vector3.up * heightMultiplayer, (target.position - checkingObject.position).normalized, out hit, maxRadius))
        {
            if (LayerMask.LayerToName(hit.transform.gameObject.layer) == "Player")
            {
                float angle = Vector3.Angle(checkingObject.forward + Vector3.up * heightMultiplayer, directionBetween);

                if (angle <= maxAngle)
                {
                    isInFov = true;
                    inFOV = true;
                }
                else if (inAlertFOV == false) isInFov = false;
            }
        }
        //Alert radius back vision AI
        if (Physics.Raycast(checkingObject.position + Vector3.up * heightMultiplayer, (target.position - checkingObject.position).normalized, out hit, alertRadius))
        {
            if (LayerMask.LayerToName(hit.transform.gameObject.layer) == "Player")
            {
                float angle = Vector3.Angle(checkingObject.forward + Vector3.up * heightMultiplayer, -directionBetween);

                if (angle <= alertAngle)
                {
                    isInFov = true;
                    inAlertFOV = true;
                }
                else if (inFOV == false) isInFov = false;
            }
        }

        // TO DO: When an enemy dies add his dead body transform to the body array
        for (int i = 0; i < corpse.Length; i++)
        {
            Vector3 dirBetween = (corpse[i].position - checkingObject.position).normalized;
            dirBetween.y *= 0;

            if (Physics.Raycast(checkingObject.position + Vector3.up * heightMultiplayer, (corpse[i].position - checkingObject.position).normalized, out hit, maxRadius))
            {
                if (LayerMask.LayerToName(hit.transform.gameObject.layer) == "DeadEnemy")
                {
                    float angle = Vector3.Angle(checkingObject.forward + Vector3.up * heightMultiplayer, dirBetween);

                    if (angle <= maxAngle)
                    {
                        EnemyAI.foundBody = true;
                    }
                }
            }
        }

        // TO DO: make that when you have an angry Enemy AI in your Fov that you'll chase the angry AI
        //        and when you see the player in your Fov that you'll chase the player

        for (int i = 0; i < enemies.Length; i++)
        {
            Vector3 dirBetween = (enemies[i].position - checkingObject.position).normalized;
            dirBetween.y *= 0;

            if (Physics.Raycast(checkingObject.position + Vector3.up * heightMultiplayer, (enemies[i].position - checkingObject.position).normalized, out hit, maxRadius))
            {
                if (LayerMask.LayerToName(hit.transform.gameObject.layer) == "ChasingEnemy")
                {
                    float angle = Vector3.Angle(checkingObject.forward + Vector3.up * heightMultiplayer, dirBetween);

                    if (angle <= maxAngle)
                    {
                        print("rheee");
                        isInFov = true;
                        inAlertFOV = true;
                    }
                }
            }
        }
    }

    public void FixedUpdate()
    {
        print(checkObject);
        InFOV(transform, player, maxAngle, maxRadius, alertRadius);
        if (isInFov && EnemyAI.foundPlayer)
        {
            maxAngle = foundPlayerAngle;
            maxRadius = foundPlayerRadius;
        }
        else
        {
            maxRadius = originalRadius;
            maxAngle = originalAngle;
        }
    }
}