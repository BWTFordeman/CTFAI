﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterActions : MonoBehaviour {

    public MeshRenderer meshRenderer;

    public GameObject myBase;
    public GameObject bulletPrefab;
    public string myTeam;    // myTeam is given as 'Red' or 'Blue'
    public int rotateSpeed;
    public float movSpeed;

    // Privates:
    private readonly double shootDelay = 1.0f;
    private double nextBulletShotTime = 0.0f;
    private int playerNumber;
    private bool isAI;
    private bool isDead;
    private Vector3 startPosition;
    private Vector3 bluePosition = new Vector3(7, 1.85f, -15);
    private Vector3 redPosition = new Vector3(-7, 1.85f, -3);

    // Values for network sensors:
    private bool isHoldingFlag;
    private bool isStandingInBase;

    // Pathfinder:
    PathFinder enemyBaseFinder;
    PathFinder enemyWithFlagFinder;
    PathFinder friendlyBaseFinder;

    // Network:
    readonly NeuralNetwork nn = new NeuralNetwork(new int[] { 10, 25, 25, 4 });
    
    
    void Start()
    {
        enemyBaseFinder = new PathFinder();
        enemyWithFlagFinder = new PathFinder(); 
        friendlyBaseFinder = new PathFinder();
    }

    void Update ()
    {
        if (!isDead)
        {
            if (isAI)   // Do actions of Network:
            {
                
                
                Vector2 currentPosition = new Vector2((int)transform.position.x + 8, Mathf.Abs((int)transform.position.z) - 1);

                int pathToEnemyBase = 0;        // Default left
                int enemyBaseforward = 0;       // Is base straight forward.

                int pathToMyBase = 0;           // Default left
                int allyBaseforward = 0;       // Is base straight forward.


                if (myTeam == "Red")
                {
                    // Complex version:
                    pathToEnemyBase = enemyBaseFinder.GetComplexPath((int)currentPosition.x, (int)currentPosition.y, 15, 15, this.transform); // Curent position, enemyFlagPosition, playertransform.
                    enemyBaseFinder.IsLookingStraightAtGoal(this.transform);

                    pathToMyBase = friendlyBaseFinder.GetComplexPath((int)currentPosition.x, (int)currentPosition.y, 2, 1, this.transform); // Curent position, enemyFlagPosition, playertransform.
                    allyBaseforward = friendlyBaseFinder.IsLookingStraightAtGoal(this.transform);

                    // Simple version:
                    //pathToEnemyBase = enemyBaseFinder.GetSimplePath(this.gameObject.transform.forward, this.transform.position, bluePosition);
                    //pathToMyBase = friendlyBaseFinder.GetSimplePath(this.gameObject.transform.forward, this.transform.position, redPosition);
                }
                else if (myTeam == "Blue")
                {
                    // Complex version:
                    pathToEnemyBase = enemyBaseFinder.GetComplexPath((int)currentPosition.x, (int)currentPosition.y, 2, 1, this.transform); // Curent position, enemyFlagPosition, playertransform.
                    pathToMyBase = friendlyBaseFinder.GetComplexPath((int)currentPosition.x, (int)currentPosition.y, 15, 15, this.transform); // Curent position, enemyFlagPosition, playertransform.

                    // Simple version:
                    //pathToEnemyBase = enemyBaseFinder.GetSimplePath(this.gameObject.transform.forward, this.transform.position, redPosition);
                    //pathToMyBase = friendlyBaseFinder.GetSimplePath(this.gameObject.transform.forward, this.transform.position, bluePosition);
                }

                // Input to the brain of this player:
                float[] input = new float[10];
                
                input[0] = 1;
                // I hold the enemy flag:
                input[1] = (isHoldingFlag) ? 1 : 0;
                // I am in my base:
                input[2] = (isStandingInBase) ? 1 : 0;
                // I rotate left/right for path to enemy base:
                input[3] = pathToEnemyBase;
                // Is enemy base straight forward:
                input[4] = enemyBaseforward;
                // I rotate left/right for path to my base:
                input[5] = pathToMyBase;
                // Is ally base straight forward:
                input[6] = allyBaseforward;
                // Is an enemy within field of view:
                input[7] = this.gameObject.GetComponent<FieldOfView>().IsEnemyWithinFieldOfView();
                // Enemy left or right:
                input[8] = this.gameObject.GetComponent<FieldOfView>().IsEnemyLeftOrRight(this.gameObject.transform);
                // Am I looking directly at enemy:
                input[9] = this.gameObject.GetComponent<FieldOfView>().IsLookingStraightAtEnemy(this.gameObject.transform);
                
                
                float[] output = nn.Run(input);

                Debug.Log("Input: " + input[0] + " " + input[1] + " " + input[2] + " " + input[3] + " " + input[4] + " " + input[5] + " " + input[6] + " " + input[7]);
                Debug.Log("output: " + output[0] + " " + output[1] + " " + output[2] + " " + output[3]);
                

                // Output functions:
                if (output[0] > 0.8)
                {
                    move();
                }
                if (output[1] > 0.8)   // Rotate left:
                {
                    rotate(0.2f);
                }
                if (output[2] > 0.8)   // Rotate right:
                {
                    rotate(-0.2f);
                }
                if (output[3] > 0.8)
                {
                    ShootBullet();
                }

            }
            else // Human players:
            {

                if (playerNumber == 0)
                {
                    if (Input.GetKey(KeyCode.W))
                    {
                        move();
                    }
                    if (Input.GetKey(KeyCode.A))
                    {
                        rotate(0.1f);
                    }
                    if (Input.GetKey(KeyCode.D))
                    {
                        rotate(-0.1f);
                    }
                    if (Input.GetKey(KeyCode.S))
                    {
                        ShootBullet();
                    }
                }
                else if (playerNumber == 1)
                {
                    if (Input.GetKey(KeyCode.I))
                    {
                        move();
                    }
                    if (Input.GetKey(KeyCode.J))
                    {
                        rotate(0.1f);
                    }
                    if (Input.GetKey(KeyCode.L))
                    {
                        rotate(-0.1f);
                    }
                    if (Input.GetKey(KeyCode.K))
                    {
                        ShootBullet();
                    }
                }
                else if (playerNumber == 2)
                {
                    if (Input.GetKey(KeyCode.UpArrow))
                    {
                        move();
                    }
                    if (Input.GetKey(KeyCode.LeftArrow))
                    {
                        rotate(0.1f);
                    }
                    if (Input.GetKey(KeyCode.RightArrow))
                    {
                        rotate(-0.1f);
                    }
                    if (Input.GetKey(KeyCode.DownArrow))
                    {
                        ShootBullet();
                    }
                }
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (myTeam == "Red")
        {
            if (other.gameObject.tag == "RedBase")
            {
                isStandingInBase = true;
            }
        }
        else if (myTeam == "Blue")
        {
            if (other.gameObject.tag == "BlueBase")
            {
                isStandingInBase = true;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (myTeam == "Red")
        {
            if (other.gameObject.tag == "RedBase")
            {
                isStandingInBase = false;
            }
        }
        else if (myTeam == "Blue")
        {
            if (other.gameObject.tag == "BlueBase")
            {
                isStandingInBase = false;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Set up an IEnumerator which checks every half second for a total of 3 seconds if player is colliding, and a bool isColliding that is set true here, and false in OnTriggerExit
        // If not colliding all the time, stop checking for more, if entering collision again, set up IEnumerator again.
        //TODO if colliding with own flag, if noone is holding it - then put back to base after staying on it for few seconds.
        //TODO: Add another input to network: "Is our flag on the ground(not in base)?" && "Am I standing on our flag?"
        if (myTeam == "Red")
        {

            // Retrieve own flag: TODO do differently later on.
            if (other.gameObject.tag == "Red")
            {
                // You can not pickup your flag if someone else is holding it.
                if (other.gameObject.transform.parent == null)
                {
                    other.gameObject.transform.position = redPosition;
                }
            }

            // Pickup flag:
            if (other.gameObject.tag == "Blue")
            {
                // As long as someone else is holding the flag, you can't pick it up.
                if (other.gameObject.transform.parent == null)
                {
                    // Reset paths:
                    enemyBaseFinder = new PathFinder();
                    friendlyBaseFinder = new PathFinder();

                    PickupFlag(other.gameObject);
                }
            }

            // Put flag in base:
            if (other.gameObject.tag == "RedBase")
            {
                try
                {
                    GameObject flag = this.gameObject.transform.Find("FlagPoleBlue").gameObject;
                    flag.transform.position = bluePosition;
                    flag.transform.parent = null;
                    isHoldingFlag = false;
                    // Reset paths:
                    enemyBaseFinder = new PathFinder();
                    friendlyBaseFinder = new PathFinder();
                } catch
                {
                    //Don't do anything.
                    //Debug.Log("IDK");
                }
            }
        }
        else if (myTeam == "Blue")
        {

            // Retrieve own flag: TODO make player have to wait a couple seconds. TODO: make player wait a random amount of seconds to capture flag from enemy base aswell?
            if (other.gameObject.tag == "Blue")
            {
                // You can not pickup your flag if someone else is holding it.
                if (other.gameObject.transform.parent == null)
                {
                    other.gameObject.transform.position = bluePosition;
                    // Reset paths:
                    enemyBaseFinder = new PathFinder();
                    friendlyBaseFinder = new PathFinder();
                }
            }

            // Pickup flag:
            if (other.gameObject.tag == "Red")
            {
                // As long as someone else is holding the flag, you can't pick it up.
                if (other.gameObject.transform.parent == null)
                {
                    // Reset paths:
                    enemyBaseFinder = new PathFinder();
                    friendlyBaseFinder = new PathFinder();

                    PickupFlag(other.gameObject);
                }
            }

            // Put flag in base:
            if (other.gameObject.tag == "BlueBase")
            {
                try
                {
                    GameObject flag = this.gameObject.transform.Find("FlagPoleRed").gameObject;
                    flag.transform.position = redPosition;
                    flag.transform.parent = null;
                    isHoldingFlag = false;

                    // Reset paths:
                    enemyBaseFinder = new PathFinder();
                    friendlyBaseFinder = new PathFinder();
                }
                catch
                {
                    //Don't do anything.
                    //Debug.Log("IDK");
                }
            }
        }
    }

    private void DropFlag() // Drop flag when dying.
    {
        if (isHoldingFlag)
        {
            // Find flag:
            GameObject flag = new GameObject();
            if (myTeam == "Blue")
            {
                flag = this.gameObject.transform.Find("FlagPoleRed").gameObject;
            }
            else if (myTeam == "Red")
            {
                flag = this.gameObject.transform.Find("FlagPoleBlue").gameObject;
            }


            // Take flag out of players hands:
            flag.transform.SetParent(null);

            isHoldingFlag = false;

            // Set flag on ground:
            flag.transform.position = flag.transform.position + flag.transform.up * -0.5f;
        }
    }

    void PickupFlag(GameObject flag) // Set yourself as parent to flag.
    {
        enemyBaseFinder = new PathFinder();
        friendlyBaseFinder = new PathFinder();

        isHoldingFlag = true;
        flag.transform.SetParent(this.gameObject.transform);
        flag.transform.position = this.gameObject.transform.position + 0.5f * this.gameObject.transform.right + 0.5f * this.gameObject.transform.up;
    }

    public void move()
    {
        transform.position += transform.forward * movSpeed * Time.deltaTime * 0.40f;
    }

    public void rotate(float angleAmount)
    {
        transform.Rotate(Vector3.down * angleAmount * rotateSpeed * Time.deltaTime * 50);
    }

    public void ShootBullet()   // Shoots a bullet from players position and direction forward.
    {
        if (Time.time > nextBulletShotTime) // Make sure that player only can shoot one bullet every now and then.
        {
            if (nextBulletShotTime < Time.time - 0.5f)
            {
                nextBulletShotTime = Time.time;
            }
            nextBulletShotTime += shootDelay;
            //Vector3 position = transform.position;
            //position.y += 1;
            GameObject bullet = Instantiate(bulletPrefab, transform.position + (transform.forward * 1), Quaternion.identity);
            bullet.transform.Find("Sphere").gameObject.GetComponent<Rigidbody>().AddForce(transform.forward * 150f + transform.up * 2f);
        }
    }

    public void KillPlayer()
    {
        DropFlag(); // If player is holding flag, then drop it on the ground.
        

        // Remove player from map, and respawn later:
        GameObject go = this.gameObject.transform.Find("Cube").gameObject;

        IEnumerator coroutine;
        coroutine = Respawn();
        StartCoroutine(coroutine);

    }

    IEnumerator Respawn()
    {
        isDead = true;  //Make user/ai not able to do anything.
        // Set position out of map:
        Vector3 pos = this.gameObject.transform.position;
        pos.z -= 22;    // This is a weird number which puts player into a position out of the map...
        this.gameObject.transform.position = pos;
        
        yield return new WaitForSeconds(4);

        // Set position to respawn position, make player able to do actions again:
        isDead = false;
        this.gameObject.transform.position = startPosition;
        this.gameObject.transform.rotation = Quaternion.identity;

        enemyBaseFinder = new PathFinder();
        friendlyBaseFinder = new PathFinder();
    }

    public void setHuman(Vector3 position, int playerNr) // Sets character this script is runned on as human.
    {
        playerNumber = playerNr;
        startPosition = position;
        isAI = false;
    }

    public void setAI(Vector3 position, int trainGenerations)    // Sets character this script is runned on as AI.
    {
        startPosition = position;
        isAI = true;
        nn.LoadBrain(); // Make the humans remember their stuff :P
    }
    
}
