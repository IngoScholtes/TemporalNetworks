using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TemporalNetworks;

namespace TempNet
{
    class Fn_Stats
    {
        public static void Run(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: TempNet stats [temporal_network_file] [undirected=false]");
                return;
            }

            bool undirected = false;

            if (args.Length == 3)
                undirected = Boolean.Parse(args[2]);

            Console.Write("Reading temporal network as {0} network...", undirected?"undirected":"directed");
            TemporalNetwork temp_net = TemporalNetwork.ReadFromFile(args[1], undirected:undirected);
            Console.WriteLine("done.");

            int interactions_total = temp_net.EdgeCount;

            Console.WriteLine("\nTemporal network");
            Console.WriteLine("=================");
            Console.WriteLine("Number of nodes:                  \t{0}", temp_net.VertexCount);
            Console.WriteLine("Number of time steps:             \t{0}", temp_net.Length);
            Console.WriteLine("Number of interactions:           \t{0}", interactions_total);
            Console.WriteLine("Highest granularity               \t{0} edges per time step", temp_net.MaxGranularity);
            Console.WriteLine("Lowest granularity                \t{0} edges per time step", temp_net.MinGranularity);

            temp_net.ReduceToTwoPaths();
            Console.WriteLine("Fraction of two-path interactions \t{0:0.00}", (double) temp_net.AggregateNetwork.CumulativeWeight/(double) interactions_total);

            Console.WriteLine("\nAggregate network (only nodes and edges contributing to two-paths)");
            Console.WriteLine("==================================================================");
            Console.WriteLine("Number of nodes:                  \t{0}", temp_net.AggregateNetwork.VertexCount);
            Console.WriteLine("Number of interactions:           \t{0}", temp_net.AggregateNetwork.CumulativeWeight);
            Console.WriteLine("Number of two-paths:              \t{0}", temp_net.TwoPathCount);            
            Console.WriteLine("Number of edges in aggregate net  \t{0}", temp_net.AggregateNetwork.EdgeCount);
            Console.WriteLine("Max in-degree in aggregate net    \t{0}", temp_net.AggregateNetwork.MaxIndeg);
            Console.WriteLine("Max out-degree in aggregate net   \t{0}", temp_net.AggregateNetwork.MaxOutdeg);
            Console.WriteLine("Max weight in aggregate net       \t{0}", temp_net.AggregateNetwork.MaxWeight);            
            Console.WriteLine("Min weight in aggregate net       \t{0}", temp_net.AggregateNetwork.MinWeight);
            Console.WriteLine("Max rel. weight in aggregate net  \t{0:0.00000}", (double)temp_net.AggregateNetwork.MaxWeight /  (double) temp_net.AggregateNetwork.CumulativeWeight);
            Console.WriteLine("Min rel. weight in aggregate net  \t{0:0.00000}", (double) temp_net.AggregateNetwork.MinWeight / (double) temp_net.AggregateNetwork.CumulativeWeight);

        }
    }
}
