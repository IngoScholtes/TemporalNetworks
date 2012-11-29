using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TemporalNetworks;

namespace TempNet
{
    class Fn_Shuffle
    {
        /// <summary>
        /// Creates randomized temporal networks based on a shuffling of edges or two-paths
        /// </summary>
        /// <param name="args"></param>
        public static void Run(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: TempNet shuffle [what] [original_net_file] [output_file] [undirected=false] [length=0]");
                Console.WriteLine("... where [what] is ...");
                Console.WriteLine("\t 'twopaths' \t Shuffle the two-paths in the original network. Preserves the aggregate network and the betweenness preference distribution.");
                Console.WriteLine("\t 'edges' \t Shuffles the edges in the original network. Preserves the aggregate network only.");
                Console.WriteLine("");
                return;
            }
            string shuffling = args[1];
            string out_file = args[3];

            if (!CmdlTools.PromptExistingFile(out_file))
            {
                Console.WriteLine("User opted to exit.");
                return;
            }

            bool undirected = false;
            if (args.Length >= 5)
                undirected = bool.Parse(args[4]);

            int length = 0;
            if (args.Length == 6)
                length = int.Parse(args[5]);

            TemporalNetwork output = null;

            Console.Write("Reading temporal network as {0} network...", undirected ? "undirected" : "directed");
            TemporalNetwork temp_net = TemporalNetwork.ReadFromFile(args[2], undirected: undirected);
            Console.WriteLine(" done.");

            Console.Write("Extracting two paths and building aggregate network...");

            temp_net.ReduceToTwoPaths();

            Console.WriteLine(" done.");

            if (shuffling == "twopaths")
            {
                Console.Write("Shuffling two paths ...");

                output = TemporalNetworkEnsemble.ShuffleTwoPaths(temp_net, length);

                Console.WriteLine(" done.");
            }
            else if (shuffling == "edges")
            {
                Console.Write("Shuffling edges ...");

                output = TemporalNetworkEnsemble.ShuffleEdges(temp_net, length);

                Console.WriteLine(" done.");
            }
            else
                Console.WriteLine("{0} is not a valid parameter!", shuffling);

            if (output != null)
            {
                Console.Write("Saving temporal network to {0}...", out_file);
                TemporalNetwork.SaveToFile(out_file, output);
                Console.WriteLine(" done.");
            }
        }
    }
}
