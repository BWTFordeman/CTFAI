using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class NetworkVisualization : MonoBehaviour {
    
    public GameObject Dendrite;
    public GameObject Canvas;

    // Arrays used to get position so I can set values for all those crazy dendrites, also to set colors of nodes.
    private GameObject[] layer1Nodes = new GameObject[8];
    private GameObject[] layer2Nodes = new GameObject[25];
    private GameObject[] layer3Nodes = new GameObject[25];
    private GameObject[] layer4Nodes = new GameObject[4];


    // Dendrite layers:
    private GameObject[] firstLayer = new GameObject[8 * 25];
    private GameObject[] secondLayer = new GameObject[25 * 25];
    private GameObject[] thirdLayer = new GameObject[25 * 4];

    readonly NeuralNetwork nn = new NeuralNetwork(new int[] { 8, 25, 25, 4 });

    // Used to change colors:
    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;


    void Start()
    {
        // Load in data to network:
        nn.LoadBrain();


        // Get nodes in visualization:      //TODO MAKE nodes instead of getting them...
        for (int i = 0; i < layer1Nodes.Length; i++)
        {
            if (i == 0)
            {
                layer1Nodes[i] = Canvas.transform.Find("InputLayer").gameObject.transform.Find("Node").gameObject;
                
            }
            else
            {
                layer1Nodes[i] = Canvas.transform.Find("InputLayer").gameObject.transform.Find("Node (" + i + ")").gameObject;
                
            }
        }
        for (int i = 0; i < layer2Nodes.Length; i++)
        {
            if (i == 0)
            {
                layer2Nodes[i] = Canvas.transform.Find("HiddenLayer1").gameObject.transform.Find("Node").gameObject;
                layer3Nodes[i] = Canvas.transform.Find("HiddenLayer2").gameObject.transform.Find("Node").gameObject;
                
            }
            else
            {
                layer2Nodes[i] = Canvas.transform.Find("HiddenLayer1").gameObject.transform.Find("Node (" + i + ")").gameObject;
                layer3Nodes[i] = Canvas.transform.Find("HiddenLayer2").gameObject.transform.Find("Node (" + i + ")").gameObject;
                
            }
        }
        for (int i = 0; i < layer4Nodes.Length; i++)
        {
            if (i == 0)
            {
                layer4Nodes[i] = Canvas.transform.Find("OutputLayer").gameObject.transform.Find("Node").gameObject;
                
            }
            else
            {
                layer4Nodes[i] = Canvas.transform.Find("OutputLayer").gameObject.transform.Find("Node (" + i + ")").gameObject;
                
            }
        }
        

        // Create dendrites:
        for (int j = 0; j < 8; j++)
        {
            for (int i = 0; i < 25; i++)
            {
                int dendriteNr = (i + 25 * j);
                firstLayer[dendriteNr] = Instantiate(Dendrite, new Vector3(6, 23, -38f), Quaternion.identity);

                // Set position of object:
                Vector3 tempPosition = firstLayer[dendriteNr].transform.position;
                tempPosition.y += 1.05f;
                tempPosition.y -= (i * 0.16f);   // Set position depending on node it goes to.
                tempPosition.y -= (j * 0.5f);    // Set position depending on each node it comes from.
                firstLayer[dendriteNr].transform.position = tempPosition;

                // Set rotation:
                firstLayer[dendriteNr].transform.LookAt(layer2Nodes[i].transform);

                // Set length of object:
                Vector3 tempScale = firstLayer[dendriteNr].transform.localScale;
                float distance = Vector3.Distance(layer2Nodes[i].transform.position, firstLayer[dendriteNr].transform.position);
                tempScale.z = 0;
                tempScale.z += (2.0f * distance);
                firstLayer[dendriteNr].transform.localScale = tempScale;

                /*// I don't need shadows, so turning it off makes the game more efficient.
                firstLayer[dendriteNr].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                firstLayer[dendriteNr].GetComponent<Renderer>().receiveShadows = false;*/
            }
        }

        for (int j = 0; j < 25; j++)
        {
            for (int i = 0; i < 25; i++)
            {
                int dendriteNr = (i + 25 * j);
                secondLayer[dendriteNr] = Instantiate(Dendrite, new Vector3(2, 23, -38f), Quaternion.identity);

                // Set position of object:
                Vector3 tempPosition = secondLayer[dendriteNr].transform.position;
                tempPosition.y += 1.23f;
                tempPosition.y -= (i * 0.16f);   // Set position depending on node it goes to.
                tempPosition.y -= (j * 0.16f);    // Set position depending on each node it comes from.
                secondLayer[dendriteNr].transform.position = tempPosition;

                // Set rotation:
                secondLayer[dendriteNr].transform.LookAt(layer3Nodes[i].transform);

                // Set length of object:
                Vector3 tempScale = secondLayer[dendriteNr].transform.localScale;
                float distance = Vector3.Distance(layer3Nodes[i].transform.position, secondLayer[dendriteNr].transform.position);
                tempScale.z = 0;
                tempScale.z += (2.0f * distance);
                secondLayer[dendriteNr].transform.localScale = tempScale;
            }
        }


        for (int j = 0; j < 25; j++)
        {
            for (int i = 0; i < 4; i++)
            {
                int dendriteNr = (i + 4 * j);
                //int dendriteNr = (i + 25 * j);
                thirdLayer[dendriteNr] = Instantiate(Dendrite, new Vector3(-2, 23, -38f), Quaternion.identity);

                // Set position of object:
                Vector3 tempPosition = thirdLayer[dendriteNr].transform.position;
                tempPosition.y += 0.815f;
                tempPosition.y -= (i * 1.0f);   // Set position depending on node it goes to.
                tempPosition.y -= (j * 0.16f);    // Set position depending on each node it comes from.
                thirdLayer[dendriteNr].transform.position = tempPosition;

                // Set rotation:
                thirdLayer[dendriteNr].transform.LookAt(layer4Nodes[i].transform);

                // Set length of object:
                Vector3 tempScale = thirdLayer[dendriteNr].transform.localScale;
                float distance = Vector3.Distance(layer4Nodes[i].transform.position, thirdLayer[dendriteNr].transform.position);
                tempScale.z = 0;
                tempScale.z += (2.0f * distance);
                thirdLayer[dendriteNr].transform.localScale = tempScale;
            }
        }

        SetColor(); // Sets color of dendrites(weights)

        // Change colors of nodes:
        IEnumerator coroutine;
        coroutine = Visualize();
        StartCoroutine(coroutine);
    }

    IEnumerator Visualize()
    {
        while (true)
        {
            // Change the color of nodes depending on their value in network when running with random inputs:
            float[] input = new float[8];
            input[0] = 1;
            for (int i = 1; i < 8; i++)
            {
                input[i] = (Random.Range(0, 100) < 50) ? 1 : 0;
            }
            nn.FeedForwardVisualize(input);
            yield return new WaitForSeconds(5);
        }
    }


    public void SetNodeColors(int layerNr, float[] nodeValues)
    {
        switch(layerNr)
        {
            case 0:
                for (int i = 0; i < nodeValues.Length; i++) // Set colors for input layer:
                {
                    if (nodeValues[i] < 0.4f)   // Make node white
                    {
                        layer1Nodes[i].GetComponent<Image>().color = Color.white;
                    }
                    else // Make node black:
                    {
                        layer1Nodes[i].GetComponent<Image>().color = Color.black;
                    }
                }
                break;
            case 1:
                for (int i = 0; i < nodeValues.Length; i++) // Set colors for hidden layer 1:
                {
                    if (nodeValues[i] < 0.4f)   // Make node white
                    {
                        layer2Nodes[i].GetComponent<Image>().color = Color.white;
                    }
                    else // Make node black:
                    {
                        layer2Nodes[i].GetComponent<Image>().color = Color.black;
                    }
                }
                break;
            case 2:
                for (int i = 0; i < nodeValues.Length; i++) // Set colors for hidden layer 2:
                {
                    if (nodeValues[i] < 0.4f)   // Make node white
                    {
                        layer3Nodes[i].GetComponent<Image>().color = Color.white;
                    }
                    else // Make node black:
                    {
                        layer3Nodes[i].GetComponent<Image>().color = Color.black;
                    }
                }
                break;
            case 3:
                for (int i = 0; i < nodeValues.Length; i++) // Set colors for output layer:
                {
                    if (nodeValues[i] < 0.4f)   // Make node white
                    {
                        layer4Nodes[i].GetComponent<Image>().color = Color.white;
                    }
                    else // Make node black:
                    {
                        layer4Nodes[i].GetComponent<Image>().color = Color.black;
                    }
                }
                break;
        }
    }
   

    void SetColor() // Sets color of dendrites based on values in the network.
    {
        //Get weights:
        float[,,] weightValues = nn.GetWeights(); // layer, node, dendrite
        

        // Change color:
        // First layer:
        for (int j = 0; j < 8; j++) // Nodes
        {
            for (int k = 0; k < 25; k++)    // Dendrites
            {
                float weight = (weightValues[0, j, k] + 1) / 2;
                float alpha = (weight > 0.5f) ? 1 : 0;
                weight = weight * 2 - 1;
                Color Color = new Color(weight * alpha, 0, 0, alpha);
                _propBlock = new MaterialPropertyBlock();
                _renderer = firstLayer[(j * 25) + k].gameObject.transform.Find("Dendrite").gameObject.GetComponent<Renderer>();


                // Get the current value of the material properties in the renderer.
                _renderer.GetPropertyBlock(_propBlock);
                // Assign our new value.
                _propBlock.SetColor("_Color", Color);
                // Apply the edited values to the renderer.
                _renderer.SetPropertyBlock(_propBlock);
            }
        }

        // Second layer:
        for (int j = 0; j < 25; j++) // Nodes
        {
            for (int k = 0; k < 25; k++)    // Dendrites
            {
                float weight = (weightValues[0, j, k] + 1) / 2;
                float alpha = (weight > 0.5f) ? 1 : 0;
                weight = weight * 2 - 1;
                Color Color = new Color(weight * alpha, 0, 0, alpha);


                /*float weight = (weightValues[0, j, k] + 1) / 2;
                float alpha = (weight > 0.5f) ? 1 : 0;
                Color Color = new Color(1, 1, 1, alpha);*/
                _propBlock = new MaterialPropertyBlock();
                _renderer = secondLayer[(j * 25) + k].gameObject.transform.Find("Dendrite").gameObject.GetComponent<Renderer>();


                // Get the current value of the material properties in the renderer.
                _renderer.GetPropertyBlock(_propBlock);
                // Assign our new value.
                _propBlock.SetColor("_Color", Color);
                // Apply the edited values to the renderer.
                _renderer.SetPropertyBlock(_propBlock);
            }
        }

        // Third layer:
        for (int j = 0; j < 25; j++) // Nodes
        {
            for (int k = 0; k < 4; k++)    // Dendrites
            {
                float weight = (weightValues[0, j, k] + 1) / 2;
                float alpha = (weight > 0.5f) ? 1 : 0;
                weight = weight * 2 - 1;
                Color Color = new Color(weight * alpha, 0, 0, alpha);

                /*float weight = (weightValues[0, j, k] + 1) / 2;
                float alpha = (weight > 0.5f) ? 1 : 0;
                Color Color = new Color(1, 1, 1, alpha);*/
                _propBlock = new MaterialPropertyBlock();
                _renderer = thirdLayer[(j * 4) + k].gameObject.transform.Find("Dendrite").gameObject.GetComponent<Renderer>();


                // Get the current value of the material properties in the renderer.
                _renderer.GetPropertyBlock(_propBlock);
                // Assign our new value.
                _propBlock.SetColor("_Color", Color);
                // Apply the edited values to the renderer.
                _renderer.SetPropertyBlock(_propBlock);
            }
        }
    }
}
