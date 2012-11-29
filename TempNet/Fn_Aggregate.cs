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
                Console.WriteLine("Usage: TempNet aggregate [temporal_network_file] [output_file]");
                return;
            }
            string out_file = args[2];

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

            Console.Write("Saving weighted aggregate network ...");
            WeightedNetwork.SaveToFile(out_file, temp_net.AggregateNetwork);
            Console.WriteLine(" done.");
        }
    }
}
