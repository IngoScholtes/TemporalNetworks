using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TemporalNetworks;

namespace TempNet
{
    class Fn_RandomWalk
    {
        /// <summary>
        /// Runs a random walk process on an aggregate networks and outputs the dynamics of the infection size
        /// </summary>
        /// <param name="args"></param>
        public static void Run(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: TempNet rw [temporal_network_file] [output_file] [betwpref=false]");
                return;
            }
            string out_file = args[2];
            bool betwprefpres = false;
            if(args.Length==4)
                betwprefpres = bool.Parse(args[3]);

            if (!CmdlTools.PromptExistingFile(out_file))
            {
                Console.WriteLine("User opted to exit.");
                return;
            }

            Console.Write("Reading temporal network as undirected network...");
            TemporalNetwork temp_net = TemporalNetwork.ReadFromFile(args[1], true);

            // Prevent the network from being reduced to two paths
            temp_net.StripEdgesToTwoPaths = false;
            Console.WriteLine("done.");

            Console.Write("Running random walk on temporal network with length T={0} ...", temp_net.Length);
            IDictionary<int, int> output = RandomWalk.RunRandomWalker(temp_net, true, betwprefpres);
            Console.WriteLine(" done.");

            Console.WriteLine("Info: Visited {0} nodes after {1} steps", output.Last().Value, output.Last().Key);
            Console.Write("Writing first passage time dynamics ...");
            StringBuilder sb = new StringBuilder();
            foreach (var step in output)
                sb.AppendLine(string.Format("{0} {1}\n", step.Key, step.Value));
            System.IO.File.WriteAllText(out_file, sb.ToString());
            Console.WriteLine(" done.");
        }
    }
}
