using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class RandomizeMap : MonoBehaviour {

    public GameObject block;
    [Range(0f, 50f)]
    public int blockPercentage;
    public int trainGenerations;

    //Prefab of players to instantiate:
    public GameObject redPlayer;
    public GameObject bluePlayer;

    // Get amount of players on each team:
    public Text leftTeamAI;
    public Text rightTeamAI;
    public Text leftTeamHuman;
    public Text rightTeamHuman;

    // Contains positions of randomly spawned blocks, to know where the blocks are. Could do it differently, but for now better to not make it too complex.
    public int[,] walls;  //18, 17

    private bool rotationFinish;
    private bool startingGame;
    private Quaternion newRotation;

    private int[] getPlayers()   // Return amount of ai/human players on each team.
    {
        int[] array = { int.Parse(leftTeamAI.text), int.Parse(rightTeamAI.text), int.Parse(leftTeamHuman.text), int.Parse(rightTeamHuman.text) };
        if (array[0] + array[2] != array[1] + array[3])
        {
            throw new System.Exception("Not equal amount on each team!");
        }
        if (array[2] + array[3] > 2)
        {
            throw new System.Exception("Too many players");
        }

        if (array[0] + array[2] >= 3)
        {
            if (array[2] > 0)   // If atleast 1 player on the team is wanted. put him there.
            {
                array[2] = 1;
            }
            array[0] = 2;
        }
        if (array[1] + array[3] >= 3)
        {
            if (array[3] > 0)   // If atleast 1 player on the team is wanted. put him there.
            {
                array[3] = 1;
            }
            array[1] = 2;
        }

        return array;
    }

    public void deleteBlocks()
    {
        foreach (var gameObj in FindObjectsOfType(typeof(GameObject)) as GameObject[])
        {
            if (gameObj.name == "Block(Clone)")
            {
                Destroy(gameObj);
            }
        }
    }

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        walls = new int[18,17];
        rotationFinish = false;
        startingGame = false;
        GameObject camera = GameObject.Find("Main Camera").gameObject;
        newRotation = camera.transform.rotation;
        newRotation.x += 0.6f;
    }

    void Update ()
    {
        if (startingGame)
        {
            GameObject camera = GameObject.Find("Main Camera").gameObject;
            if (camera.transform.rotation.x < newRotation.x)
            {
                camera.transform.rotation = Quaternion.Lerp(camera.transform.rotation, newRotation, Time.time * 0.0125f);
            }
            if (!(Mathf.Approximately(camera.transform.rotation.x, newRotation.x)))
            {
                if (rotationFinish == false)
                {
                    rotationFinish = true;
                    randomize();
                }
            }
        }
    }

    public void TrainNetwork()  //When user presses train button, this function will be run and network will be trained and saved in file.
    {

        // Read in training values:
        string path = "Assets/Resources/TrainingValues.txt";

        // TestValues:
        float[][] input = new float[256][];
        float[][] output = new float[256][];
        

        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        string line;
        int inputNr = 0;
        int outputNr = 0;
        int lineNr = 0;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.StartsWith("#"))   // Don't read comments in the txt file:
            {
                continue;
            }
            if (lineNr % 2 == 0)
            {
                string[] bits = line.Split(' ');

                float[] array = new float[bits.Length];
                for (int i = 0; i < bits.Length; i++)
                {
                    array[i] = float.Parse(bits[i]);
                }
                input[inputNr] = array;
                inputNr++;
            }
            else
            {
                string[] bits = line.Split(' ');
                float[] array = new float[bits.Length];

                for (int i = 0; i < bits.Length; i++)
                {
                    array[i] = float.Parse(bits[i]);
                }

                output[outputNr] = array;
                outputNr++;
            }
            lineNr++;
        }
        
        

        NeuralNetwork net = new NeuralNetwork(new int[] { 10, 25, 25, 4 }); // initialize network

        for (int i = 0; i < trainGenerations; i++)
        {
            for (int j = 0; j < (lineNr/2); j++)
            {
                net.FeedForward(input[j]);
                net.BackProp(output[j]);
            }
        }


        // Used to see the accuracy of the network:
        float accuracy = 0;

        for (int i = 0; i < trainGenerations; i++)
        {
            for (int j = 0; j < (lineNr / 2); j++)
            {
                float[] outputTest = net.FeedForward(input[j]);
                for (int k = 0; k < 4; k++)
                {
                    if (output[j][k] == 1)  // Supposed to be 1.
                    {
                        float newValue = outputTest[k] * 2 - 1; // Making value from 0.5-1 to 0-1.
                        accuracy = (accuracy + newValue) / 2;
                    }
                    else    // Supposed to be 0.
                    {
                        float newValue = 1 - (outputTest[k] * 2);     // Making value from 0.5-0 to 0-1.
                        accuracy = (accuracy + newValue) / 2;
                    }
                }
            }
        }
        Debug.Log("Accuracy: " + accuracy);

        // Save the training values:
        net.SaveBrain();
        net.LoadBrain();
    }

    public void randomize() // Run this when creating a game to make a new layout for the map:
    {
        if (!rotationFinish)
        {
            startingGame = true;
        }
        else 
        {
            // Delete last games blocks:
            deleteBlocks();
            
            for (int x = 0; x < 18; x++)
            {
                for (int z = 0; z < 17; z++)
                {
                    walls[x,z] = 0;    // Set all values.
                    if (x > 4 || z > 4) // Not left base
                    {
                        if (x < 14 || z < 12) // Not right base
                        {
                            // Set random map 
                            if (Random.Range(1, 100) <= blockPercentage)
                            {
                                // Acknowledge a position has wall placed on it.
                                walls[x,z] = 1;
                                // Some weird values to give correct position according to the map:
                                Instantiate(block, new Vector3(-9 + x, 1.75f, -1 + -1 * z), Quaternion.identity);
                            }
                        }
                    }
                }
            }
            

            // Get player(s)/AI(s):
            var t = getPlayers();   //LeftTeamAI, rightTeamAI, leftTeamHuman, rightTeamHuman.

            // Instantiate players to their positions according to number and team.
            int playerNumber = 0;   // This number will give positions for players on the keyboard, wadq/ijlu/up,left,right,down?

            // Red Team:
            int redTeam = 0;    // Count positions in team used up.
            ArrayList redTeamPlayerPositions = new ArrayList();
            redTeamPlayerPositions.Add(new Vector3(-6f, 2, -4));
            redTeamPlayerPositions.Add(new Vector3(-6f, 2, -2));
            redTeamPlayerPositions.Add(new Vector3(-8f, 2, -4));

            for (int i = 0; i < t[0]; i++)   
            {
                GameObject AI = Instantiate(redPlayer, (Vector3)redTeamPlayerPositions[redTeam], Quaternion.Euler(0, 180, 0)) as GameObject;
                AI.GetComponent<CharacterActions>().setAI((Vector3)redTeamPlayerPositions[redTeam], trainGenerations);
                redTeam++;
            }
            for (int i = 0; i < t[2]; i++)
            {
                GameObject human = Instantiate(redPlayer, (Vector3)redTeamPlayerPositions[redTeam], Quaternion.Euler(0, 180, 0)) as GameObject;
                human.GetComponent<CharacterActions>().setHuman((Vector3)redTeamPlayerPositions[redTeam], playerNumber++);
                redTeam++;
            }


            //Blue Team:
            int blueTeam = 0;
            ArrayList blueTeamPlayerPositions = new ArrayList();
            blueTeamPlayerPositions.Add(new Vector3(6, 2, -16));
            blueTeamPlayerPositions.Add(new Vector3(6, 2, -14));
            blueTeamPlayerPositions.Add(new Vector3(8, 2, -14));

            for (int i = 0; i < t[1]; i++)
            {
                GameObject AI = Instantiate(bluePlayer, (Vector3)blueTeamPlayerPositions[blueTeam], Quaternion.identity) as GameObject;
                AI.GetComponent<CharacterActions>().setAI((Vector3)blueTeamPlayerPositions[blueTeam], trainGenerations);
                blueTeam++;
            }
            for (int i = 0; i < t[3]; i++)
            {
                GameObject human = Instantiate(bluePlayer, (Vector3)blueTeamPlayerPositions[blueTeam], Quaternion.identity) as GameObject;
                human.GetComponent<CharacterActions>().setHuman((Vector3)blueTeamPlayerPositions[blueTeam], playerNumber++);
                blueTeam++;
            }
        }
    }
}
