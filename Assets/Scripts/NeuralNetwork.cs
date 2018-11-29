using System;
using System.IO;
using UnityEngine;

// Simple Neural Network used for AI in a simple capture the flag game, I added some extra functionality for visualization and saving/loading the brain into textfiles.
// so you won't have to train your network before every game.
public class NeuralNetwork
{
    int[] layer; //layer information
    Layer[] layers; //layers in the network
    
    // Constructor, set values:
    public NeuralNetwork(int[] layer)
    {
        // Deep copy layers
        this.layer = new int[layer.Length];
        for (int i = 0; i < layer.Length; i++)
            this.layer[i] = layer[i];

        // Create neural layers
        layers = new Layer[layer.Length - 1];

        for (int i = 0; i < layers.Length; i++)
        {
            layers[i] = new Layer(layer[i], layer[i + 1]);
        }
    }

    // Used for sending input, getting output for AI to make moves in the game.
    public float[] Run(float[] inputs)
    {
        return FeedForward(inputs);
    }
    

    public float[,,] GetWeights()   // Used for visualization of weights in the network.
    {
        float[,,] returnValue = new float[layer.Length - 1,25,25]; // Set some max values.
        
        // Get weights of all dendrites in the network:
        for (int k = 0; k < 3; k++)
        {
            for (int i = 0; i < layers[k].weights.GetLength(0); i++)
            {
                for (int j = 0; j < layers[k].weights.GetLength(1); j++)
                {
                    returnValue[k,i,j] = layers[k].weights[i, j];
                }
            }
        }

        return returnValue;
    }
    

    // Saves the weights of the network in files. TODO: maybe use 1 file and make several layers inside there?..
    public void SaveBrain()
    {
        
        string layer1Path = "Assets/Resources/layer-1.txt";
        string layer2Path = "Assets/Resources/layer-2.txt";
        string layer3Path = "Assets/Resources/layer-3.txt";

        // Write to file:
        StreamWriter layer1Writer = new StreamWriter(layer1Path, false);
        StreamWriter layer2Writer = new StreamWriter(layer2Path, false);
        StreamWriter layer3Writer = new StreamWriter(layer3Path, false);

        // Goes through each of the files:
        for (int i = 0; i < layers[0].weights.GetLength(0); i++)
        {
            for (int j = 0; j < layers[0].weights.GetLength(1); j++)
            {
                layer1Writer.Write(layers[0].weights[i, j] + " ");
            }
            layer1Writer.Write("\n");
        }
        for (int i = 0; i < layers[1].weights.GetLength(0); i++)
        {
            for (int j = 0; j < layers[1].weights.GetLength(1); j++)
            {
                layer2Writer.Write(layers[1].weights[i, j] + " ");
            }
            layer2Writer.Write("\n");
        }
        for (int i = 0; i < layers[2].weights.GetLength(0); i++)
        {
            for (int j = 0; j < layers[2].weights.GetLength(1); j++)
            {
                layer3Writer.Write(layers[2].weights[i, j] + " ");
            }
            layer3Writer.Write("\n");
        }


        layer1Writer.Close();
        layer2Writer.Close();
        layer3Writer.Close();

        Debug.Log("Saved Brain!");
    }

    // Loads weights from a file:
    public void LoadBrain()
    {
        Debug.Log("Loading brain!");
        string layer1Path = "Assets/Resources/layer-1.txt";
        string layer2Path = "Assets/Resources/layer-2.txt";
        string layer3Path = "Assets/Resources/layer-3.txt";
        
        StreamReader reader1 = new StreamReader(layer1Path);
        StreamReader reader2 = new StreamReader(layer2Path);
        StreamReader reader3 = new StreamReader(layer3Path);
        
        int inputNr = 0;

        string line;
        while ((line = reader1.ReadLine()) != null)
        {
            string[] bits = line.Split(' ');
            for (int i = 0; i < bits.Length - 1; i++)
            {
                layers[0].weights[inputNr, i] = float.Parse(bits[i]);
            }
            inputNr++;
        }
        inputNr = 0;

        while ((line = reader2.ReadLine()) != null)
        {
            string[] bits = line.Split(' ');
            for (int i = 0; i < bits.Length - 1; i++)
            {
                layers[1].weights[inputNr, i] = float.Parse(bits[i]);
            }
            inputNr++;
        }
        inputNr = 0;
        while ((line = reader3.ReadLine()) != null)
        {
            string[] bits = line.Split(' ');
            for (int i = 0; i < bits.Length - 1; i++)
            {
                layers[2].weights[inputNr, i] = float.Parse(bits[i]);
            }
            inputNr++;
        }

        reader1.Close();
        reader2.Close();
        reader3.Close();
    }

    // Function used for visualization of network nodes:
    public void FeedForwardVisualize(float[] inputs)
    {
        GameObject visualizerObject = GameObject.Find("Visualize Camera").gameObject;

        // Colorize nodes in all layers:
        visualizerObject.GetComponent<NetworkVisualization>().SetNodeColors(0, inputs);
        visualizerObject.GetComponent<NetworkVisualization>().SetNodeColors(1, layers[0].FeedForward(inputs));
        // TODO make it more modular by having a for loop of length of network by amount of layers:
        visualizerObject.GetComponent<NetworkVisualization>().SetNodeColors(2, layers[1].FeedForward(layers[0].outputs));
        visualizerObject.GetComponent<NetworkVisualization>().SetNodeColors(3, layers[2].FeedForward(layers[1].outputs));
    }
    

    // Feeds input forward in the network:
    public float[] FeedForward(float[] inputs)
    {
        // TODO remove output here. not used anymore.
        float[] output = layers[0].FeedForward(inputs);

        for (int i = 1; i < layers.Length; i++)
        {
            layers[i].FeedForward(layers[i - 1].outputs);
        }
        
        return layers[layers.Length - 1].outputs; //return output of last layer
    }
    

    // Feeds expected output backwards in the network:
    public void BackProp(float[] expected)
    {
        // Run over all layers backwards
        for (int i = layers.Length - 1; i >= 0; i--)
        {
            if (i == layers.Length - 1)
            {
                // Propagate output layer.
                layers[i].BackPropOutput(expected);
            }
            else
            {
                // Propagate hidden layer.
                layers[i].BackPropHidden(layers[i + 1].gamma, layers[i + 1].weights);
            }
        }

        // Update weights
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].UpdateWeights();
        }
    }
    
    // Class for every layer in the network
    public class Layer
    {
        int numberOfInputs; // # of neurons in the previous layer
        int numberOfOuputs; // # of neurons in the current layer


        public float[] outputs;         // Outputs of this layer
        public float[] inputs;          // Inputs in into this layer
        public float[,] weights;        // Weights of this layer
        public float[,] weightsDelta;   // Deltas of this layer
        public float[] gamma;           // Gamma of this layer
        public float[] error;           // Error of the output layer

        public static System.Random random = new System.Random(); //Static random class variable
        

        // Constructor
        public Layer(int numberOfInputs, int numberOfOuputs)
        {
            this.numberOfInputs = numberOfInputs;
            this.numberOfOuputs = numberOfOuputs;

            // Initilize datastructures
            outputs = new float[numberOfOuputs];
            inputs = new float[numberOfInputs];
            weights = new float[numberOfOuputs, numberOfInputs];
            weightsDelta = new float[numberOfOuputs, numberOfInputs];
            gamma = new float[numberOfOuputs];
            error = new float[numberOfOuputs];

            InitilizeWeights(); //initilize weights
        }
        
        // Initalize weights between -0.5 and 0.5.
        public void InitilizeWeights()
        {
            for (int i = 0; i < numberOfOuputs; i++)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weights[i, j] = (float)random.NextDouble() - 0.5f;
                }
            }
        }
        

        // Feedforward within each layer:
        public float[] FeedForward(float[] inputs)
        {
            this.inputs = inputs;// keep copy which can be used for back propagation

            //feed forwards
            for (int i = 0; i < numberOfOuputs; i++)
            {
                outputs[i] = 0;
                for (int j = 0; j < numberOfInputs; j++)
                {
                    outputs[i] += inputs[j] * weights[i, j];
                }

                outputs[i] = (float)Math.Tanh(outputs[i]);
            }

            return outputs;
        }
        
        // TanH derivate.
        public float TanHDer(float value)
        {
            return 1 - (value * value);
        }
        
        // Backpropagate the expected output to train output layer:
        public void BackPropOutput(float[] expected)
        {
            // Error dervative of the cost function.
            for (int i = 0; i < numberOfOuputs; i++)
                error[i] = outputs[i] - expected[i];

            // Gamma calculation.
            for (int i = 0; i < numberOfOuputs; i++)
                gamma[i] = error[i] * TanHDer(outputs[i]);

            // Caluclating detla weights.
            for (int i = 0; i < numberOfOuputs; i++)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weightsDelta[i, j] = gamma[i] * inputs[j];
                }
            }
        }
        

        // Backpropagate the expected output to train hidden layer:
        public void BackPropHidden(float[] gammaForward, float[,] weightsFoward)
        {
            // Caluclate new gamma using gamma sums of the forward layer.
            for (int i = 0; i < numberOfOuputs; i++)
            {
                gamma[i] = 0;

                for (int j = 0; j < gammaForward.Length; j++)
                {
                    gamma[i] += gammaForward[j] * weightsFoward[j, i];
                }

                gamma[i] *= TanHDer(outputs[i]);
            }

            // Caluclating detla weights.
            for (int i = 0; i < numberOfOuputs; i++)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weightsDelta[i, j] = gamma[i] * inputs[j];
                }
            }
        }
        
        // Update weight value of every dendrite:
        public void UpdateWeights()
        {
            for (int i = 0; i < numberOfOuputs; i++)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weights[i, j] -= weightsDelta[i, j] * 0.033f;   // Update with delta and TR.
                }
            }
        }
    }
}
