using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TemporalNetworks;

namespace TempNet
{
    class Fn_T2
    {
        /// <summary>
        /// Parallely computes the betweenness preference distribution of a given temporal network
        /// </summary>
        /// <param name="args"></param>
        public static void Run(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: TempNet T2 [temporal_network_file] [output_file] [aggregationWndow=1] [undirected=false]");
                return;
            }
            string out_file = args[2];

            if (!CmdlTools.PromptExistingFile(out_file))
            {
                Console.WriteLine("User opted to exit.");
                return;
            }

            bool undirected = false;
            int aggregationWindow = 1;            
            if (args.Length >= 4)
                aggregationWindow = int.Parse(args[3]);
            if (args.Length >= 5)
                undirected = Boolean.Parse(args[4]);


            Console.Write("Reading temporal network as {0} network...", undirected ? "undirected" : "directed");
            TemporalNetwork temp_net = TemporalNetwork.ReadFromFile(args[1], undirected:undirected);
            Console.WriteLine(" done.");

            Console.WriteLine("Applying aggregation window, length = {0}, time steps before = {1}", aggregationWindow, temp_net.Length);
            temp_net.AggregateTime(aggregationWindow);
            Console.WriteLine("done, time steps after = {0}", temp_net.Length);

            Console.Write("Building aggregate networks ...");
            temp_net.ReduceToTwoPaths();
            Console.WriteLine(" done.");

            Console.Write("Writing transition matrix ...");
            WriteTwopathTransitionMatrix(temp_net, out_file);
            Console.WriteLine("done.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static void WriteTwopathTransitionMatrix(TemporalNetwork temp_net, string file)
        {
            // Dictionary that takes transition probabilities for pairs of edges (i.e. two-paths)
            var P = new Dictionary<Tuple<string, string>, double>();

            var edge_nodes = temp_net.SecondOrderAggregateNetwork.Vertices;

            foreach (var source in edge_nodes)
            {
                foreach (var target in edge_nodes)
                {
                    var two_path = new Tuple<string, string>(source, target);
                    if (temp_net.SecondOrderAggregateNetwork.ContainsKey(two_path))
                        P[two_path] = temp_net.SecondOrderAggregateNetwork[two_path] / temp_net.SecondOrderAggregateNetwork.GetCumulativeOutWeight(source);
                    else
                        P[two_path] = 0d;
                }
            }

            System.IO.StreamWriter sw = new System.IO.StreamWriter(file);

            // Write column header
            foreach (var target in edge_nodes)
                sw.Write(target + " ");
            sw.WriteLine();

            foreach (var source in edge_nodes)
            {
                sw.Write(source + " ");
                foreach (var target in edge_nodes)
                {
                    var two_path = new Tuple<string, string>(source, target);
                    sw.Write("{0} ", string.Format(System.Globalization.CultureInfo.GetCultureInfo("en-US").NumberFormat, "{0:0.000000}", P[two_path]));
                }
                sw.WriteLine();
            }
            sw.Close();
        }
    }
}
