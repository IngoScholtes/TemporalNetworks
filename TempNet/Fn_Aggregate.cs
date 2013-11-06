using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TemporalNetworks;

namespace TempNet
{
    class Fn_Aggregate
    {            
        /// <summary>
        /// Outputs a simple example temporal network
        /// </summary>
        /// <param name="args"></param>
        public static void Run(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: TempNet aggregate [temporal_network_file] [output_file] [aggregationWindow=1] [weighted_aggregate_networks=false] [TimeReversal=false] [Directed=false]");
                return;
            }

            string out_file = args[2];
            bool two_path = false;
            bool timeReversal = false;
            bool directed = false;

            int aggregationWindow = 1;

            if (args.Length >= 4)
                aggregationWindow = int.Parse(args[3]);

            if (args.Length >= 5)
                two_path = bool.Parse(args[4]);

            if (args.Length >= 6)
                timeReversal = bool.Parse(args[5]);

            if (args.Length >= 7)
                directed = bool.Parse(args[6]);

            if (!CmdlTools.PromptExistingFile(out_file))
            {
                Console.WriteLine("User opted to exit.");
                return;
            }

            Console.Write("Reading temporal network ...");
            TemporalNetwork temp_net = TemporalNetwork.ReadFromFile(args[1], undirected: !directed);
            Console.WriteLine(" done.");

            Console.WriteLine(temp_net.Length);
            Console.Write("Applying agggregation window [{0}] ...", aggregationWindow);
            temp_net.AggregateTime(aggregationWindow);
            Console.WriteLine("done.");
            Console.WriteLine(temp_net.Length);

            Console.Write("Reducing to two path networks ...");
            temp_net.ReduceToTwoPaths(timeReversal);
            Console.WriteLine("done.");

            if (!two_path)
            {
                Console.Write("Saving temporal network ...");
                TemporalNetwork.SaveToFile(out_file, temp_net);
                Console.WriteLine(" done.");
            }
            else
            {
                //WeightedNetwork net = new WeightedNetwork();
                //foreach(string tp in temp_net.TwoPathWeights.Keys)
                //{
                //    string[] comps = tp.Split(',');
                //    net.AddEdge(comps[0], comps[2], EdgeType.Directed, temp_net.TwoPathWeights[tp]);
                //}
                Console.Write("Saving weighted first-order aggregate network ...");
                WeightedNetwork.SaveToFile(out_file+".1.edges", temp_net.AggregateNetwork);
                Console.WriteLine(" done.");

                Console.Write("Saving weighted second-order aggregate network ...");
                WeightedNetwork.SaveToFile(out_file+".2.edges", temp_net.SecondOrderAggregateNetwork);
                Console.WriteLine(" done.");
            }
        }
    }
}
