using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder
{
    public Vector3 goalPosition;               
    public uint[,] enemyBasePath; // Path to enemies base. TODO find path to enemy base first, then find path to friendly base, then to enemy with flag.

    private Node currentPlayerNode;  // Node that contains node player is on/is going to.

    private uint maxZ;
    private uint maxX;
    List<Node> checkedList;    // Contains nodes we have checked distance for.
    bool foundGoal;
    


    public int GetSimplePath(Vector3 lookDirection, Vector3 currentPosition, Vector3 goalPosition) // Simple version of Pathfinder. Doesn't think about walls.
    {
        Vector3 goalDirection = new Vector3(goalPosition.x - currentPosition.x, goalPosition.y - currentPosition.y, goalPosition.z - currentPosition.z);

        float angle = Vector3.Angle(goalDirection, lookDirection);
        Vector3 cross = Vector3.Cross(goalDirection, lookDirection);
        if (cross.y < 0) angle = -angle;

        return (Vector3.Angle(goalDirection, lookDirection) < 10) ? 1 : 0;
    }

    //TODO, when there is enemy within field of view an amount to the right then the player won't rotate to that point.


    //Each path has startPosition at goal position, and end position at playerPosition


    //TODO, make several paths to goal,
    //Make a new path to goal when player starts game/respawns,
    //If an enemy is within field of view, this enemy is the current goal(delete current path), if there is no enemy within field of view AND path is null, create new one.
    //If holding flag, go for base, if flag is not there, spin right without moving.

    public PathFinder()
    {
        checkedList = new List<Node>();
        foundGoal = false;
        maxX = 17;
        maxZ = 19;
        enemyBasePath = new uint[maxX,maxZ];
    }

    public void CreateComplexPath()
    {

    }
    
    public int GetComplexPath(int startX, int startZ, int goalX, int goalZ, Transform playerTransform) // same as 'def aStar():' in python code. with some adjustments
    {
        // "Transform" playerpositions into valid positions in pathfinder xD :

        //TODO make playerTransform go up if over .5 and down if below .5, to get more approximate position.

        //TODO fix so that z value is correct...

        Vector3 validPosition = new Vector3(playerTransform.position.x + 9, playerTransform.position.y, (playerTransform.position.z * -1) -1);

        // If path not created:
        
        if (currentPlayerNode == null)
        {
            // Make path from goal to player so parent of player position will be next position in path.
            goalPosition.z = startZ;
            goalPosition.x = startX;

            setupMap(goalX, goalZ);
            // Create path in
            createPath();
        }

        // If player has reached next position in path - go to next after that:
        // if reached goal, next node will still be goal until a new path is set for the player.
      
        float distanceToNode = Vector3.Distance(validPosition, new Vector3(goalPosition.x, 1.75f, goalPosition.z)); // Distance between player position and next node position.
        if (distanceToNode < 0.35f) // If close enough to next node.
        {
            UpdateGoal();
        }


        // Get directions to calculate if player should rotate left/right to hit goal:
        Vector3 goalDirection = new Vector3(goalPosition.x - validPosition.x, 0, goalPosition.z - validPosition.z); // Direction to goal from currentPosition
        Vector3 forwardDirectionTest = new Vector3(playerTransform.forward.x, 0, (playerTransform.forward.z * -1)); // Direction with length 1 forward of playerPosition

        int angle = (Vector3.SignedAngle(goalDirection, forwardDirectionTest, Vector3.up) > 0)? 1 : 0;

        //Debug.Log("Angle:" + angle);
        //Debug.Log("goalDirection:" + goalDirection.x + " " + goalDirection.z);                    
        //Debug.Log("forward:" + playerTransform.forward.x + " " + (playerTransform.forward.z * -1));
      
        for (int i = 0; i < enemyBasePath.GetLength(0); i++)
        {
            //Debug.Log("enemyBasePath: " + enemyBasePath[i,0] + " " + enemyBasePath[i,1] + " " + enemyBasePath[i, 2] + " " + enemyBasePath[i, 3] + " " + enemyBasePath[i, 4] + " " + enemyBasePath[i, 5] + " " + enemyBasePath[i, 6] + " " + enemyBasePath[i, 7] + " " + enemyBasePath[i, 8] + " " + enemyBasePath[i, 9] + " " + enemyBasePath[i, 10] + " " + enemyBasePath[i, 11] + " " + enemyBasePath[i, 12] + " " + enemyBasePath[i, 13] + " " + enemyBasePath[i, 14] + " " + enemyBasePath[i, 15] + " " + enemyBasePath[i, 16] + " " + enemyBasePath[i, 17] + " " + enemyBasePath[i, 18]);
        }
        //Debug.Log("done");
        // Return 0 or 1 depending on left or right for path to enemy base.
        return angle;
    }

    // Is run every time player reach a partGoal of a path:         F.ex: from 0,0 to 2,1 a partGoal could be 1,0 or 0,1.
    public void UpdateGoal()
    {
        Debug.Log("updating goal");
        bool updated = false;
        if (goalPosition.x > 0) //TODO when player reached final goal, enemyBasePath[(int)goalPosition.x - 1,(int)goalPosition.z] will be 4. then just run in circles until new path is made.(at same time actually...)
        {
            if (enemyBasePath[(int)goalPosition.z - 1,(int)goalPosition.x] == 3)
            {
                enemyBasePath[(int)goalPosition.z, (int)goalPosition.x] = 6;
                goalPosition.z -= 1;
                updated = true;
            }
        }
        if (goalPosition.x + 1 < maxX)
        {
            if (enemyBasePath[(int)goalPosition.z + 1, (int)goalPosition.x] == 3)
            {
                if (!updated)   // Used to make sure that I am only updating position on map with x or z or none, not both.
                {
                    enemyBasePath[(int)goalPosition.z, (int)goalPosition.x] = 6;
                    goalPosition.z += 1;
                    updated = false;
                }
            }
        }
        if (goalPosition.z > 0)
        {
            if (enemyBasePath[(int)goalPosition.z, (int)goalPosition.x - 1] == 3)
            {
                if (!updated)
                {
                    enemyBasePath[(int)goalPosition.z, (int)goalPosition.x] = 6;
                    goalPosition.x -= 1;    // It is correct with x-- here on the z check, because map is inverted. Easiest fix without destroying my brain atm. was to just do this.
                    updated = false;
                }
            }
        }
        if (goalPosition.z + 1 < maxZ)
        {
            if (enemyBasePath[(int)goalPosition.z, (int)goalPosition.x + 1] == 3)
            {
                if (!updated)
                {
                    enemyBasePath[(int)goalPosition.z, (int)goalPosition.x] = 6;
                    goalPosition.x += 1;
                }
            }
        }
    }

    void createPath()
    {
        while (!foundGoal)
        {

            Node tempNode = checkedList[0];
            checkedList.RemoveAt(0);


            // Checks for new nodes to add to list:

            // Nodes that are left, right, up, down
            if (tempNode.positionX > 0 && !foundGoal)
            {
                checkNode(tempNode, tempNode.positionX - 1, tempNode.positionZ, 1 + tempNode.distanceToThisNode);
            }
            if (tempNode.positionX + 1 < maxX && !foundGoal)
            {
                checkNode(tempNode, tempNode.positionX + 1, tempNode.positionZ, 1 + tempNode.distanceToThisNode);
            }
            if (tempNode.positionZ > 0 && !foundGoal)
            {
                checkNode(tempNode, tempNode.positionX, tempNode.positionZ - 1, 1 + tempNode.distanceToThisNode);
            }
            if (tempNode.positionZ + 1 < maxZ && !foundGoal)
            {
                checkNode(tempNode, tempNode.positionX, tempNode.positionZ + 1, 1 + tempNode.distanceToThisNode);
            }
        }
    }

    // Set next node in path:
    public void getNextNode()
    {
        while (currentPlayerNode.parent != null)
        { 
            Node tempNode = currentPlayerNode;
            try
            {
                currentPlayerNode = currentPlayerNode.parent;
                enemyBasePath[(int)currentPlayerNode.positionZ, (int)currentPlayerNode.positionX] = 3;
            } catch
            {
                currentPlayerNode = tempNode;
            }
        }
    }

    void checkNode(Node parent, float positionX, float positionZ, float distanceToThisNode)
    {
        try
        {
            if (enemyBasePath[(int)positionZ, (int)positionX] == 0)
            {
                enemyBasePath[(int)positionZ, (int)positionX] = 1;

                Node tempNode = new Node(parent, positionX, positionZ, distanceToThisNode, goalPosition.x, goalPosition.z);  // Create new node that could be a part of the shortest path.
                Debug.Log("creating node:" + positionX + " " + positionZ);

                checkedList.Add(tempNode);    // Add node to list before sorting it to right position.
                checkedList.Sort((x1, x2) => x1.totalDistance.CompareTo(x2.totalDistance));
            }
            else if (enemyBasePath[(int)positionZ, (int)positionX] == 4)
            {
                foundGoal = true;
                // Set node of player:
                currentPlayerNode = new Node(parent, positionX, positionZ, distanceToThisNode, goalPosition.x, goalPosition.z);
                // Set node player should go to:
                getNextNode();
            }
        } catch
        {
            Debug.Log("x" + positionX + " z " + positionZ);
        }
    }

    void setupMap(int startX, int startZ)
    {
        // 0 -unchecked
        // 1 -checked
        // 2 -wall
        // 3 -final path
        // 4 -goal
        // 5 -start
        int[,] walls = GameObject.Find("MainFrame").gameObject.GetComponent<RandomizeMap>().walls;
        // Set unchecked/walls:
        for (int i = 0; i < maxX; i++)
        {
            for (int j = 0; j < maxZ - 1; j++)
            {
                if (walls[j,i] == 1)
                {
                    enemyBasePath[i,j] = 2;
                }
                else
                {
                    enemyBasePath[i,j] = 0;
                }
            }
        }
        // Set goal:
        enemyBasePath[(int)goalPosition.z,(int)goalPosition.x] = 4;
        // Set start:
        Debug.Log("x: " + startX + " z: " + startZ);
        enemyBasePath[startX,startZ] = 5;

        // Add start to list:
        Node startNode = new Node(null, startZ, startX, 0, goalPosition.x, goalPosition.z); // Starting point will always start with g(n) = 0.
        Debug.Log("start" + startX + " " + startZ);
        Debug.Log("goal" + goalPosition.x + " " + goalPosition.z);
        checkedList.Add(startNode);
    }


    // From here on I do the good pathfinding algorithm stuff:

    public class Node
    {
        public Node parent;
        public float positionX;
        public float positionZ;
        public float distanceToThisNode;
    
        public float totalDistance;
    
        // Constructor:
        //TODO remove goalPosition in here and use global: Vector3 goalPosition.
        public Node(Node p, float posX, float posZ, float distToNode, float goalPositionX, float goalPositionZ)
        {
            parent = p;
            positionX = posX;
            positionZ = posZ;
            distanceToThisNode = distToNode;
    
            
            float distanceX = Mathf.Abs(goalPositionX - positionX);
            float distanceZ = Mathf.Abs(goalPositionZ - positionZ);
    
            totalDistance = distanceX + distanceZ + distanceToThisNode;
    
        }
    }
}