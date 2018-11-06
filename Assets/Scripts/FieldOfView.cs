using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FieldOfView : MonoBehaviour {

    public string enemyTarget;    // Used to see if looking at enemy.
    public bool visualization; // Turn on and off visualization with a simple switch.
    public MeshRenderer meshRenderer;

    public float viewRadius;
    [Range(0,360)]
    public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();  // Contains a list of targets within the view of the character at current time.

    public float meshResolution;        // For Visualization inside actual game, not only editor.
    public int edgeResolveIterations;    // Amount of iterations for finding edge of raycasting to obstacles. less is more optimal.
    public float edgeDistanceThreshold;     // Threshold for distance between two raycasts colliding with different objects.

    public MeshFilter viewMeshFilter;
    Mesh viewMesh;

    private void Update()
    {
        if (visualization)
        {
            meshRenderer.enabled = true;
        }
        else
        {
            meshRenderer.enabled = false;
        }

        // visibleTargets contain transforms of enemies within eyesight.
    }

    public int IsEnemyWithinFieldOfView() // Returns 0 if no enemies, 1 if enemies.
    {
        findVisibleTargets();

        if (visibleTargets.Count > 0)
        {
            return 1;
        }
        return 0;
    }

    public int IsEnemyLeftOrRight(Transform playerTransform) // Returns 0 if no enemies, 0/1 if first enemy int list to left or right
    {
        findVisibleTargets();

        if (visibleTargets.Count > 0)
        {
            Vector3 enemyVector = new Vector3(visibleTargets[0].position.x - playerTransform.position.x, visibleTargets[0].position.y - playerTransform.position.y, visibleTargets[0].position.z - playerTransform.position.z);
            //float angle = Vector3.Angle(enemyVector, playerTransform.forward);

            float angle = Vector3.Angle(enemyVector, playerTransform.forward);
            Vector3 cross = Vector3.Cross(enemyVector, playerTransform.forward);
            if (cross.y < 0) angle = -angle;
            
            if (angle > 0)
            {
                return 0;   // rotate left.
            }
            else
            {
                return 1;   // rotate right.
            }
        }
        return 0;   // No enemies
    }

    public int IsLookingStraightAtEnemy(Transform playerTransform) // Returns 0 if not looking straight at enemy, 1 if doing so.
    {
        findVisibleTargets();

        // See if angle between forward vector of playerTransform and vector to enemy in visibleTargets is below a value, if it is, shoot!

        if (visibleTargets.Count > 0)
        {
            Vector3 vectorToEnemy = new Vector3(visibleTargets[0].position.x - playerTransform.position.x, visibleTargets[0].position.y - playerTransform.position.y, visibleTargets[0].position.z - playerTransform.position.z);

            float angle = Vector3.Angle(vectorToEnemy, playerTransform.forward);

            Debug.Log("angle:" + angle);
            if (angle < 5)
            {
                return 1;
            }
        }
        return 0;
    }

    void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
        //StartCoroutine("findTargetsWithDelay", .2f);
    }

    /*IEnumerator findTargetsWithDelay(float delay)   // We use this for the AI to be able to see other teammates/enemies, to act upon.
    {
        while(true)
        {
            yield return new WaitForSeconds(delay);
            findVisibleTargets();
        }
    }*/

    void LateUpdate()
    {
        drawFieldOfView();
    }

    void findVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2)    // If target is within radius and angle of sight.
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }
    }

    void drawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;

        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();  // To narrow down where to edge of the obstacle we are hitting with raycast is.
        for (int i = 0; i <= stepCount; i++)
        {
                // Draw raycast in case there is obstacle in the way:
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = viewCast(angle);

            if (i > 0)
            {
                bool edgeDistanceThresholdExceeded = Mathf.Abs(oldViewCast.dist - newViewCast.dist) > edgeDistanceThreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDistanceThresholdExceeded)) // Edge detection.
                {
                    EdgeInfo edge = findEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }

            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero; // For triangles made through 3 vertices we have first vertex at players position(Vector3.zero) and the 2 others further away, depending on viewRadius/mesh resolution.
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    // Function helps finding edge of obstacle so we get a nicer view with less performance issues when raycasting.
    EdgeInfo findEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = viewCast(angle);

            bool edgeDistanceThresholdExceeded = Mathf.Abs(minViewCast.dist - newViewCast.dist) > edgeDistanceThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDistanceThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }
        return new EdgeInfo(minPoint, maxPoint);
    }

    ViewCastInfo viewCast(float globalAngle)    // Returns values about raycast sent to see walls within eyesight of AI.
    {
        Vector3 dir = directionFromAngle(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, obstacleMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);    // We hit a wall with the raycast.
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);  // We did not hit a wall.
        }

    }

    public Vector3 directionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public struct ViewCastInfo  // Stores info for raycasting to see if a wall is within view radius.
    {
        public bool hit;
        public Vector3 point;
        public float dist;
        public float angle;


        public ViewCastInfo(bool _hit, Vector3 _point, float _dist, float _angle)
        {
            hit = _hit;
            point = _point;
            dist = _dist;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }

}
