using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TemporalNetworks;

namespace TempNet
{
    class Fn_Distribution
    {
        /// <summary>
        /// Parallely computes the betweenness preference distribution of a given temporal network
        /// </summary>
        /// <param name="args"></param>
        public static void Run(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: TempNet distribution [temporal_network_file] [output_file] [aggregationWndow=1] [undirected]");
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

            Console.Write("Preparing temporal network ...");
            temp_net.ReduceToTwoPaths();
            Console.WriteLine(" done.");

            double nodeNum = temp_net.AggregateNetwork.VertexCount;
            double current = 0d;
            double last_perc = 0d;

            Console.WriteLine("Computing betweenness preference for {0} nodes ...", nodeNum);

            // Parallely compute betweenness preference for all nodes
#if DEBUG 
            foreach(string v in temp_net.AggregateNetwork.Vertices)
#else
            Parallel.ForEach<string>(temp_net.AggregateNetwork.Vertices, v =>
#endif
            {
                double betweennessPref = BetweennessPref.GetBetweennessPref(temp_net, v);

                // Synchronized access to file and to counters ... 
                if(temp_net.AggregateNetwork.GetIndeg(v)>0 && temp_net.AggregateNetwork.GetOutdeg(v)>0)
                    lock (out_file)
                    {
                        System.IO.File.AppendAllText(out_file, v + " " + string.Format(System.Globalization.CultureInfo.GetCultureInfo("en-US").NumberFormat, "{0:0.000000}\n", betweennessPref));
                        current++;
                        if (100 * current / nodeNum >= last_perc + 5d)
                        {
                            last_perc = 100 * current / nodeNum;
                            Console.WriteLine("Completed for {0} nodes [{1:0.0} %]", current, last_perc);
                        }
                    }
            }
#if !DEBUG
                );
#endif
            Console.WriteLine("done.");
        }
    }
}
