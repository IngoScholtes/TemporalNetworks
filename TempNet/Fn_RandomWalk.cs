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
                Console.WriteLine("Usage: TempNet rw [network_file] [output_file] [WalkType=random] [aggregationWindow=1] [runs=1] [length=100000]");
                Console.WriteLine("\t where WalkType can be ... ");
                Console.WriteLine("\t bwp_pres\t Performs a random walk on the aggregate network that considers betweenness preferences computed from the temporal network");
                Console.WriteLine("\t bwp_null\t ...");
                return;
            }
            string out_file = args[2];
            string type = "bwp_pres";
            int runs = 1;
            int length = 100000;

            int aggregationWindow = 1;
            if (args.Length >= 4)
                type = args[3];
            if (args.Length >= 5)
                aggregationWindow = int.Parse(args[4]);
            if (args.Length >= 6)
                runs = int.Parse(args[5]);
            if (args.Length >= 7)
                length = int.Parse(args[6]);  

            if (!CmdlTools.PromptExistingFile(out_file))
            {
                Console.WriteLine("User opted to exit.");
                return;
            }

            Console.Write("Reading temporal network as undirected network...");
            TemporalNetwork temp_net = TemporalNetwork.ReadFromFile(args[1], true);

            Console.WriteLine("done.");

            if (aggregationWindow != 1)
            {
                Console.WriteLine("Applying aggregation window, length = {0}, time steps before = {1}", aggregationWindow, temp_net.Length);
                temp_net.AggregateTime(aggregationWindow);
                Console.WriteLine("done, time steps after = {0}", temp_net.Length);
            }

            Console.Write("Building aggregate network...");
            WeightedNetwork aggregateNet = temp_net.AggregateNetwork;
            Console.WriteLine("done.");

            for (int r = 1; r <= runs; r++)
            {
                Console.WriteLine("Running Random walk [{0}] in mode [{1}] ...", r, type);
                IDictionary<int, double> tvd = null;
                if (type == "bwp_pres")
                    tvd = RandomWalk.RunRW_BWP(temp_net, length, null_model: false);
                else if (type == "bwp_null")
                    tvd = RandomWalk.RunRW_BWP(temp_net, length, null_model: true);
                else
                {
                    Console.WriteLine("\nError: RWType {0} unknown", type);
                    return;
                }

                Console.Write("Writing time series of total variation distance ...");
                StringBuilder sb = new StringBuilder();
                foreach (var step in tvd)
                    sb.AppendLine(string.Format(System.Globalization.CultureInfo.GetCultureInfo("en-US").NumberFormat, "{0} {1}", step.Key, step.Value));
                System.IO.File.WriteAllText(out_file+"_r_"+r+".dat", sb.ToString());
                Console.WriteLine(" done.");
            }
        }
    }
}
