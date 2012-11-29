using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TemporalNetworks;

namespace TempNet
{
    class Fn_Tikz
    {
        public static void Run(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: TempNet tikz [temporal_network_file] [output_file]");
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
            Console.WriteLine("done.");

            Console.Write("Exporting to tikz...");
            TikzExporter.CreateTikzUnfolding(out_file, null, temp_net);
            Console.WriteLine(" done.");
        }
    }
}
