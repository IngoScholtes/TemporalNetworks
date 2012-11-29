using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TemporalNetworks;

namespace TempNet
{
    class Fn_Match
    {
        /// <summary>
        /// Checks whether the weighted aggregate representation of two temporal networks match
        /// </summary>
        /// <param name="args"></param>
        public static void Run(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: TempNet match [temporal_network_a] [temporal_network_b]");
                return;
            }
            string file_1 = args[1];
            string file_2 = args[2];

            Console.Write("Reading temporal network 1 ...");
            TemporalNetwork temp_net_1 = TemporalNetwork.ReadFromFile(file_1);
            Console.WriteLine("done.");

            Console.Write("Reading temporal network 1 ...");
            TemporalNetwork temp_net_2 = TemporalNetwork.ReadFromFile(file_2);
            Console.WriteLine("done.");

            Console.WriteLine("Comparing weighted aggregate networks ...");
            WeightedNetwork net_1 = temp_net_1.AggregateNetwork;
            WeightedNetwork net_2 = temp_net_2.AggregateNetwork;

            if (net_1.Equals(net_2))
                Console.WriteLine("Both networks are identical.");
            else
                Console.WriteLine("The networks are NOT identical.");
            
        }
    }
}
