using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterActions : MonoBehaviour {
    //TODO implement so that your goal is enemy with your flag if they have it and your teammate has their flag.

    public MeshRenderer meshRenderer;

    public GameObject myBase;
    public GameObject bulletPrefab;
    public string myTeam;    // myTeam is given as 'Red' or 'Blue'
    public int rotateSpeed;
    public float movSpeed;

    // Privates.
    private double shootDelay = 1.0f;
    private double nextBulletShotTime = 0.0f;
    private int playerNumber;
    private bool isAI;
    private bool isDead;
    private Vector3 startPosition;
    private Vector3 bluePosition = new Vector3(7, 1.85f, -15);
    private Vector3 redPosition = new Vector3(-7, 1.85f, -3);

    //Values for network sensors:
    private bool isHoldingFlag;
    private bool isStandingInBase;


    // Network:
    readonly NeuralNetwork nn = new NeuralNetwork(new int[] { 8, 25, 25, 4 });
    readonly List<double> inputs = new List<double>();

    //PathFinder pf;    // Use to make better paths for AI, so it won't be trapped in corners.
   

    void Update ()
    {
        if (!isDead)
        {
            if (isAI)   // Do actions of Network:
            {
                // Clear input so there won't be any doubles.
                inputs.Clear();

                // Temp: Make simple pathfinder.
                PathFinder pf = new PathFinder();
                int pathToEnemyBase = 0;        // Default left
                int pathToMyBase = 0;           // Default left

                // TODO get position of walls inside float[][] walls and send it through pf.GetPath()

                if (myTeam == "Red")
                {
                    pathToEnemyBase = pf.GetSimplePath(this.gameObject.transform.forward, this.transform.position, bluePosition);
                    pathToMyBase = pf.GetSimplePath(this.gameObject.transform.forward, this.transform.position, redPosition);
                }
                else if (myTeam == "Blue")
                {
                    pathToEnemyBase = pf.GetSimplePath(this.gameObject.transform.forward, this.transform.position, redPosition);
                    pathToMyBase = pf.GetSimplePath(this.gameObject.transform.forward, this.transform.position, bluePosition);
                }

                // Input to the brain of this player:
                float[] input = new float[8];

                //TODO always have a value 1 at beginning, so when the rest is all 0 we can get output = 1
                input[0] = 1;
                // I hold the enemy flag:
                input[1] = (isHoldingFlag) ? 1 : 0;
                // I am in my base:
                input[2] = (isStandingInBase) ? 1 : 0;
                // I rotate left/right for path to enemy base:
                input[3] = pathToEnemyBase;
                // I rotate left/right for path to my base:
                input[4] = pathToMyBase;
                // Is an enemy within field of view:
                input[5] = this.gameObject.GetComponent<FieldOfView>().IsEnemyWithinFieldOfView();
                // Enemy left or right:
                input[6] = this.gameObject.GetComponent<FieldOfView>().IsEnemyLeftOrRight(this.gameObject.transform);
                // Am I looking directly at enemy:
                input[7] = this.gameObject.GetComponent<FieldOfView>().IsLookingStraightAtEnemy(this.gameObject.transform);

                //TODO set 1 in value of all inputs, so that rest all 0 is valid
                
                float[] output = nn.Run(input);
                

                if (output[0] > 0.5)
                {
                    move();
                }
                if (output[1] > 0.5)   // Rotate left:
                {
                    rotate(-0.1f);
                }
                if (output[2] > 0.5)   // Rotate right:
                {
                    rotate(0.1f);
                }
                if (output[3] > 0.5)
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
        //TODO Right now just make it instantly jump back to base if colliding with it.
        // Set up an IEnumerator which checks every half second for a total of 3 seconds if player is colliding, and a bool isColliding that is set true here, and false in OnTriggerExit
        // If not colliding all the time, stop checking for more, if entering collision again, set up IEnumerator again.
        //TODO if colliding with own flag, if noone is holding it - then put back to base after staying on it for few seconds.
        //FUTURE: Add another input to network: "Is our flag on the ground(not in base)?" && "Am I standing on our flag?"
        if (myTeam == "Red")
        {

            // Retrieve own flag: TODO do differently later on.
            if (other.gameObject.tag == "Red")
            {
                other.gameObject.transform.position = redPosition;
            }

            // Pickup flag:
            if (other.gameObject.tag == "Blue")
            {
                PickupFlag(other.gameObject);
            }

            // Put flag in base:
            if (other.gameObject.tag == "RedBase")
            {
                try
                {
                    //TODO people can only score if their flag is stored in their base.
                    GameObject flag = this.gameObject.transform.Find("FlagPoleBlue").gameObject;
                    flag.transform.position = bluePosition;
                    flag.transform.parent = null;
                    isHoldingFlag = false;
                } catch
                {
                    //Don't do anything.
                    //Debug.Log("IDK");
                }
            }
            // TODO make teams able to score points:
        }
        else if (myTeam == "Blue")
        {

            // Retrieve own flag: TODO do differently later on.
            if (other.gameObject.tag == "Blue")
            {
                other.gameObject.transform.position = bluePosition;
            }

            // Pickup flag:
            if (other.gameObject.tag == "Red")
            {
                PickupFlag(other.gameObject);
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
        isHoldingFlag = true;
        flag.transform.SetParent(this.gameObject.transform);
        flag.transform.position = this.gameObject.transform.position + 0.5f * this.gameObject.transform.right + 0.5f * this.gameObject.transform.up;
    }

    public void move()
    {
        transform.position += transform.forward * 0.01f * movSpeed;
    }

    public void rotate(float angleAmount)
    {
        transform.Rotate(Vector3.down * angleAmount * rotateSpeed);
    }

    public void ShootBullet()   // Shoots a bullet from players position and direction forward. TODO
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
