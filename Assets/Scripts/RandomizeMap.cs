﻿using UnityEngine;
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

    private void Start()
    {
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
        

        /*
        for (int j = 0; j < (lineNr/2); j++)
        {
            Debug.Log("input" + input[j][0]);
            Debug.Log("input" + input[j][1]);
            Debug.Log("input" + input[j][2]);
            Debug.Log("input" + input[j][3]);
            Debug.Log("input" + input[j][4]);
            Debug.Log("input" + input[j][5]);
            Debug.Log("input" + input[j][6]);
            Debug.Log("output" + output[j][0]);
            Debug.Log("output" + output[j][1]);
            Debug.Log("output" + output[j][2]);
            Debug.Log("output" + output[j][3]);
        }*/



        NeuralNetwork net = new NeuralNetwork(new int[] { 8, 25, 25, 4 }); // initialize network
        
        for (int i = 0; i < trainGenerations; i++)
        {
            for (int j = 0; j < (lineNr/2); j++)
            {
                net.FeedForward(input[j]);
                net.BackProp(output[j]);
            }
        }

        
        // Testing the network:
        float[] outputt = net.FeedForward(new float[] { 1, 1, 1, 1, 1, 1, 1, 1 }); //should give 0 0 0 1
        Debug.Log(outputt[0]);
        Debug.Log(outputt[1]);
        Debug.Log(outputt[2]);
        Debug.Log(outputt[3]);
        
    
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

            // Disable Canvas.
            for (int x = 0; x < 18; x++)
            {
                for (int z = 0; z < 17; z++)
                {
                    if (x > 4 || z > 4) // Not left base
                    {
                        if (x < 14 || z < 12) // Not right base
                        {
                            // Set random map 
                            if (Random.Range(1, 100) <= blockPercentage)
                            {
                                // Some weird values to give correct position according to the map:     (should probably get size of map dynamicly, but for now this is good enough.)
                                // Random.Range(-9, 9) - x        Random.Range(-19, -1) - z
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
            redTeamPlayerPositions.Add(new Vector3(-6, 2, -4));
            redTeamPlayerPositions.Add(new Vector3(-6, 2, -2));
            redTeamPlayerPositions.Add(new Vector3(-8, 2, -4));

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