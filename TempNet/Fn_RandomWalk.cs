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
                Console.WriteLine("Usage: TempNet rw [network_file] [output_file] [WalkType=random] [aggregationWindow=1] [min_prob=0d]");
                Console.WriteLine("\t where WalkType can be ... ");
                Console.WriteLine("\t random\t Performs a random walk on the aggregate network without considering weights");
                Console.WriteLine("\t weighted\t Performs a random walk on the aggregate network that considers edge weights");
                Console.WriteLine("\t bwp_pres\t Performs a random walk on the aggregate network that considers betweenness preferences computed from the temporal network");
                Console.WriteLine("\t bwp_synth\t Performs a random walk on the aggregate network that considers synthetically generated betweenness preference.");
                return;
            }
            string out_file = args[2];
            string type = "random";
            double min_prob = 0d;
            int aggregationWindow = 1;
            if (args.Length >= 4)
                type = args[3];
            if (args.Length >= 5)
                aggregationWindow = int.Parse(args[4]);            
            if (args.Length >= 6)
                min_prob = double.Parse(args[5], System.Globalization.CultureInfo.GetCultureInfo("en-US").NumberFormat);
                

            if (!CmdlTools.PromptExistingFile(out_file))
            {
                Console.WriteLine("User opted to exit.");
                return;
            }

            Console.Write("Running Random walk in mode [{0}] ", type);
            if (type == "bwp_synth")
                Console.WriteLine("min_prob = {0:0.00} ...", min_prob);
            else
                Console.WriteLine("...");

            Console.Write("Reading temporal network as undirected network...");
            TemporalNetwork temp_net = TemporalNetwork.ReadFromFile(args[1], true);
            Console.WriteLine("done.");

            Console.WriteLine("Applying aggregation window, length = {0}, time steps before = {1}", aggregationWindow, temp_net.Length);
            temp_net.AggregateTime(aggregationWindow);
            Console.WriteLine("done, time steps after = {0}", temp_net.Length);            

            IDictionary<int, int> output = null;
            if (type == "random")
                output = RandomWalk.RunRandomWalk(temp_net.AggregateNetwork, use_weights: false);
            else if (type == "weighted")
                output = RandomWalk.RunRandomWalk(temp_net.AggregateNetwork, use_weights: true);
            else if (type == "bwp_pres")
                output = RandomWalk.RunRandomWalkBWP(temp_net);
            else if (type == "bwp_synth")
                output = RandomWalk.RunRandomWalkSyntheticBWP(temp_net.AggregateNetwork, min_prob);
            else
            {
                Console.WriteLine("\nError: RWType {0} unknown", type);
                return;
            }

            

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
