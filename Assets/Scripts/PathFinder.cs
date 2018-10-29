using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder
{

    // This class will use a* to find a path for the ai to follow.
    private Vector3 goalPosition;               // IDK
    private Vector3 nextPosition;               // This is the next position the player will try to go to.
    private List<Vector2> EnemyBasePath;        // Path to enemies base.
    private List<Vector2> FriendlyBasePath;     // Path to your own base.
    private List<Vector2> EnemyWithFlagPath;    // Path to the enemy holding your teams flag.//TODO Should I have this? or make players look around on the map randomly?
                                                // Perhaps make this path if someone on your team has seen this enemy? - a lot of work...


    public int GetSimplePath(Vector3 lookDirection, Vector3 currentPosition, Vector3 goalPosition) // Simple version of Pathfinder. Doesn't think about walls.
    {
        Vector3 goalDirection = new Vector3(goalPosition.x - currentPosition.x, goalPosition.y - currentPosition.y, goalPosition.z - currentPosition.z);

        float angle = Vector3.Angle(goalDirection, lookDirection);
        Vector3 cross = Vector3.Cross(goalDirection, lookDirection);
        if (cross.y < 0) angle = -angle;
        Debug.Log("currentPosition:" + currentPosition);
        Debug.Log("GoalPosition:" + goalPosition);
        Debug.Log("GoalDirection:" + goalDirection);

        return (Vector3.Angle(goalDirection, lookDirection) < 10) ? 1 : 0;
    }




    //Each path has startPosition at goal position, and end position at playerPosition


    //TODO, make several paths to goal,
    //Make a new path to goal when player starts game/respawns,
    //If an enemy is within field of view, this enemy is the current goal(delete current path), if there is no enemy within field of view AND path is null, create new one.
    //If holding flag, go for base, if flag is not there, spin right without moving.

    public PathFinder()
    {
        EnemyBasePath = null;
        FriendlyBasePath = null;
        EnemyWithFlagPath = null;
    }

    public void DeletePath() // If player dies, or finished something then reset some values. Haven't thought about what values yet.
    {

    }

    public void SetPath(Vector3 currentPosition, Vector3 goalPosition) // This function will return 0/1 depending on the player looking in correct direction of path or wrong. (Correct is within fieldOfView.)
    {

    }

    public int GetPath(Vector3 lookDirection) // Parameter is direction player is looking right now.    // Returns for current position, considering next position wanted.
    {
        //Should I go 2 tiles at a time?TODO
        return 0;   // Return 0 if looking in wrong direction, return 1 if looking in right direction.
    }

   
}