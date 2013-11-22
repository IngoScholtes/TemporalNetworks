using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TemporalNetworks;

namespace TempNet
{
    class Fn_Filter
    {
        public static void Run(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: TempNet scc [temporal_network_original] [temporal_network_new] [undirected=false] [absolutTime=false]");
                return;
            }

            string file_orig = args[1];
            string file_new = args[2];

            bool undirected = false;

            bool absoluteTime = false;

            if (args.Length >= 4)
                undirected = Boolean.Parse(args[3]);

            if (args.Length >= 5)
                absoluteTime = Boolean.Parse(args[4]);

            Console.Write("Reading temporal network as {0} network...", undirected?"undirected":"directed");
            TemporalNetwork temp_net = TemporalNetwork.ReadFromFile(file_orig, undirected:undirected);
            Console.WriteLine("done.");

            Console.Write("Extracting two paths and reducing original sequence...");
            temp_net.ReduceToTwoPaths(false, absoluteTime);
            Console.WriteLine("done.");

            Console.Write("Building second order aggregate network...");
            WeightedNetwork net = temp_net.SecondOrderAggregateNetwork;
            Console.WriteLine("done.");

            Console.Write("Extracting largest SCC...");
            net.ReduceToLargestStronglyConnectedComponent();
            Console.WriteLine("done.");

            Console.WriteLine("SCC has {0} nodes", net.VertexCount);

            Console.WriteLine("Filtering disconnected edges in temporal network ... ");
            foreach (var t in temp_net.Keys)
            {
                // Iterate through all edges in this time step 
                foreach (var edge in temp_net[t].ToArray())
                {
                    string node = "(" + edge.Item1 + ";" + edge.Item2 + ")";
                    // If this edge is not a node in the second order network
                    if (!net.Vertices.Contains(node))
                        temp_net[t].Remove(edge);                  
                }
            }
            Console.WriteLine("done.");

            Console.Write("Saving new temporal network ... ");
            TemporalNetwork.SaveToFile(file_new, temp_net);
            Console.WriteLine("done.");

        }
    }
}
