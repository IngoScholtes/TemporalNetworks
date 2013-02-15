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
                Console.WriteLine("Usage: TempNet aggregate [temporal_network_file] [output_file] [twopath_network=false]");
                return;
            }

            string out_file = args[2];
            bool two_path = false;
            if (args.Length == 4)
                two_path = bool.Parse(args[3]);

            if (!CmdlTools.PromptExistingFile(out_file))
            {
                Console.WriteLine("User opted to exit.");
                return;
            }

            Console.Write("Reading temporal network ...");
            TemporalNetwork temp_net = TemporalNetwork.ReadFromFile(args[1]);
            Console.WriteLine(" done.");

            Console.Write("Aggregating temporal network ...");
            temp_net.ReduceToTwoPaths();
            Console.WriteLine( "done.");

            if (!two_path)
            {
                Console.Write("Saving weighted aggregate network ...");
                WeightedNetwork.SaveToFile(out_file, temp_net.AggregateNetwork);
                Console.WriteLine(" done.");
            }
            else
            {
                WeightedNetwork net = new WeightedNetwork();
                foreach(string tp in temp_net.TwoPathWeights.Keys)
                {
                    string[] comps = tp.Split(',');
                    net.AddEdge(comps[0], comps[2], EdgeType.Directed, temp_net.TwoPathWeights[tp]);
                }
                Console.Write("Saving weighted two-path network ...");
                WeightedNetwork.SaveToFile(out_file, net);
                Console.WriteLine(" done.");
            }
        }
    }
}
