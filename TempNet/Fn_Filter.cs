using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TemporalNetworks;

namespace TempNet
{
    class Fn_Filter
    {
        public static void Run(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: TempNet filter [temporal_network_original] [filter_file] [temporal_network_new] [undirected=true]");
                return;
            }

            string file_orig = args[1];
            string filter = args[2];
            string file_new = args[3];

            bool undirected = true;

            if (args.Length >= 5)
                undirected = Boolean.Parse(args[4]);

            Console.Write("Reading temporal network as {0} network...", undirected?"undirected":"directed");
            TemporalNetwork temp_net = TemporalNetwork.ReadFromFile(file_orig, undirected:undirected);
            Console.WriteLine("done.");

            string[] filtered_edges = System.IO.File.ReadAllLines(filter);

            foreach (string x in filtered_edges)
            {
                string[] comps = x.Split(';');
                temp_net.Remove(new Tuple<string, string>(comps[0], comps[1]));
            }

            TemporalNetwork.SaveToFile(file_new, temp_net);

        }
    }
}
