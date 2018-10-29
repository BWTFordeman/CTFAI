using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Simple MLP Neural Network
/// </summary>
public class NeuralNetworkTest
{

    int[] layer; //layer information
    Layer[] layers; //layers in the network

    /// <summary>
    /// Constructor setting up layers
    /// </summary>
    /// <param name="layer">Layers of this network</param>
    public NeuralNetworkTest(int[] layer)
    {
        //deep copy layers
        this.layer = new int[layer.Length];
        for (int i = 0; i < layer.Length; i++)
            this.layer[i] = layer[i];

        //creates neural layers
        layers = new Layer[layer.Length - 1];

        for (int i = 0; i < layers.Length; i++)
        {
            layers[i] = new Layer(layer[i], layer[i + 1]);
        }
    }

   

    public void SaveBrain()
    {
        
        string layer1Path = "Assets/Resources/layer-1.txt";
        string layer2Path = "Assets/Resources/layer-2.txt";
        string layer3Path = "Assets/Resources/layer-3.txt";

        // Write to file:
        StreamWriter layer1Writer = new StreamWriter(layer1Path, false);
        StreamWriter layer2Writer = new StreamWriter(layer2Path, false);
        StreamWriter layer3Writer = new StreamWriter(layer3Path, false);

        
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
    //TODO remove a lot of comments from this code, I need to know what is going on here and must therefore make my own comments.

    public void LoadBrain() // Loads weights from a file:
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

        while ((line = reader2.ReadLine()) != null)
        {
            string[] bits = line.Split(' ');
            for (int i = 0; i < bits.Length - 1; i++)
            {
                layers[1].weights[inputNr, i] = float.Parse(bits[i]);
            }
            inputNr++;
        }
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
        Debug.Log("Brain has been loaded in!");
    }

    /// <summary>
    /// High level feedforward for this network
    /// </summary>
    /// <param name="inputs">Inputs to be feed forwared</param>
    /// <returns></returns>
    public float[] FeedForward(float[] inputs)
    {
        //feed forward
        layers[0].FeedForward(inputs);
        for (int i = 1; i < layers.Length; i++)
        {
            layers[i].FeedForward(layers[i - 1].outputs);
        }

        return layers[layers.Length - 1].outputs; //return output of last layer
    }

    /// <summary>
    /// High level back porpagation
    /// Note: It is expexted the one feed forward was done before this back prop.
    /// </summary>
    /// <param name="expected">The expected output form the last feedforward</param>
    public void BackProp(float[] expected)
    {
        // run over all layers backwards
        for (int i = layers.Length - 1; i >= 0; i--)
        {
            if (i == layers.Length - 1)
            {
                layers[i].BackPropOutput(expected); //back prop output
            }
            else
            {
                layers[i].BackPropHidden(layers[i + 1].gamma, layers[i + 1].weights); //back prop hidden
            }
        }

        //Update weights
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].UpdateWeights();
        }
    }

    /// <summary>
    /// Each individual layer in the ML{
    /// </summary>
    public class Layer
    {
        int numberOfInputs; //# of neurons in the previous layer
        int numberOfOuputs; //# of neurons in the current layer


        public float[] outputs; //outputs of this layer
        public float[] inputs; //inputs in into this layer
        public float[,] weights; //weights of this layer
        public float[,] weightsDelta; //deltas of this layer
        public float[] gamma; //gamma of this layer
        public float[] error; //error of the output layer

        public static System.Random random = new System.Random(); //Static random class variable

        /// <summary>
        /// Constructor initilizes vaiour data structures
        /// </summary>
        /// <param name="numberOfInputs">Number of neurons in the previous layer</param>
        /// <param name="numberOfOuputs">Number of neurons in the current layer</param>
        public Layer(int numberOfInputs, int numberOfOuputs)
        {
            this.numberOfInputs = numberOfInputs;
            this.numberOfOuputs = numberOfOuputs;

            //initilize datastructures
            outputs = new float[numberOfOuputs];
            inputs = new float[numberOfInputs];
            weights = new float[numberOfOuputs, numberOfInputs];
            weightsDelta = new float[numberOfOuputs, numberOfInputs];
            gamma = new float[numberOfOuputs];
            error = new float[numberOfOuputs];

            InitilizeWeights(); //initilize weights
        }

        /// <summary>
        /// Initilize weights between -0.5 and 0.5
        /// </summary>
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

        /// <summary>
        /// Feedforward this layer with a given input
        /// </summary>
        /// <param name="inputs">The output values of the previous layer</param>
        /// <returns></returns>
        public float[] FeedForward(float[] inputs)
        {
            this.inputs = inputs;// keep shallow copy which can be used for back propagation

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

        /// <summary>
        /// TanH derivate 
        /// </summary>
        /// <param name="value">An already computed TanH value</param>
        /// <returns></returns>
        public float TanHDer(float value)
        {
            return 1 - (value * value);
        }

        /// <summary>
        /// Back propagation for the output layer
        /// </summary>
        /// <param name="expected">The expected output</param>
        public void BackPropOutput(float[] expected)
        {
            //Error dervative of the cost function
            for (int i = 0; i < numberOfOuputs; i++)
                error[i] = outputs[i] - expected[i];

            //Gamma calculation
            for (int i = 0; i < numberOfOuputs; i++)
                gamma[i] = error[i] * TanHDer(outputs[i]);

            //Caluclating detla weights
            for (int i = 0; i < numberOfOuputs; i++)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weightsDelta[i, j] = gamma[i] * inputs[j];
                }
            }
        }

        /// <summary>
        /// Back propagation for the hidden layers
        /// </summary>
        /// <param name="gammaForward">the gamma value of the forward layer</param>
        /// <param name="weightsFoward">the weights of the forward layer</param>
        public void BackPropHidden(float[] gammaForward, float[,] weightsFoward)
        {
            //Caluclate new gamma using gamma sums of the forward layer
            for (int i = 0; i < numberOfOuputs; i++)
            {
                gamma[i] = 0;

                for (int j = 0; j < gammaForward.Length; j++)
                {
                    gamma[i] += gammaForward[j] * weightsFoward[j, i];
                }

                gamma[i] *= TanHDer(outputs[i]);
            }

            //Caluclating detla weights
            for (int i = 0; i < numberOfOuputs; i++)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weightsDelta[i, j] = gamma[i] * inputs[j];
                }
            }
        }

        /// <summary>
        /// Updating weights
        /// </summary>
        public void UpdateWeights()
        {
            for (int i = 0; i < numberOfOuputs; i++)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weights[i, j] -= weightsDelta[i, j] * 0.033f;
                }
            }
        }
    }
}
