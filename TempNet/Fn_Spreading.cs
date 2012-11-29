using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TemporalNetworks;

namespace TempNet
{
    class Fn_Spreading
    {
        /// <summary>
        /// Runs an SI spreading process on a temporal network and outputs the dynamics of the infection size
        /// </summary>
        /// <param name="args"></param>
        public static void Run(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: TempNet spreading [temporal_network_file] [output_file]");
                return;
            }
            string out_file = args[2];

            if (!CmdlTools.PromptExistingFile(out_file))
            {
                Console.WriteLine("User opted to exit.");
                return;
            }

            Console.Write("Reading temporal network as directed...");
            TemporalNetwork temp_net = TemporalNetwork.ReadFromFile(args[1]);

            // Prevent the network from being reduced to two paths
            temp_net.StripEdgesToTwoPaths = false;
            Console.WriteLine("done.");

            Console.Write("Running SI spreading on temporal network with {0} time steps ...", temp_net.Length);
            IDictionary<int,int> output = SISpreading.RunSpreading(temp_net, 1d);
            Console.WriteLine(" done.");

            Console.WriteLine("Info: Infected {0} nodes after {1} steps", output.Last(), output.Count());
            Console.Write("Writing spreading dynamics ...");
            StringBuilder sb = new StringBuilder();
            foreach (var step in output)
                sb.AppendLine(string.Format("{0} {1}\n", step.Key, step.Value));
            System.IO.File.WriteAllText(out_file, sb.ToString());
            Console.WriteLine(" done.");
        }
    }
}
