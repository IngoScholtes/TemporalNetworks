using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TemporalNetworks;

namespace TempNet
{
    class Fn_Example
    {
        /// <summary>
        /// Creates the simple example network with betweenness preference that can be seen in the paper in Figure 2 (right)
        /// </summary>
        /// <param name="args"></param>
        public static void Run(string[] args)
        {

            if (args.Length < 2)
            {
                Console.WriteLine("Usage: TempNet example [output_file]");
                return;
            }

            string out_file = args[1];

            if (!CmdlTools.PromptExistingFile(out_file))
            {
                Console.WriteLine("User opted to exit.");
                return;
            }

            TemporalNetwork temporal_net = new TemporalNetwork();

            int k = 0;

            for (int i = 0; i < 1; i++)
            {
                // Add edges according to two paths
                temporal_net.AddTemporalEdge(k++, "c", "e");
                temporal_net.AddTemporalEdge(k++, "e", "f");

                temporal_net.AddTemporalEdge(k++, "a", "e");
                temporal_net.AddTemporalEdge(k++, "e", "g");

                temporal_net.AddTemporalEdge(k++, "c", "e");
                temporal_net.AddTemporalEdge(k++, "e", "f");

                temporal_net.AddTemporalEdge(k++, "a", "e");
                temporal_net.AddTemporalEdge(k++, "e", "g");

                temporal_net.AddTemporalEdge(k++, "c", "e");
                temporal_net.AddTemporalEdge(k++, "e", "f");

                // Note that the next added edge additionally continues a two-path e -> f -> e
                temporal_net.AddTemporalEdge(k++, "f", "e");
                temporal_net.AddTemporalEdge(k++, "e", "b");

                // An additional edge that will be filtered during preprocessing
                temporal_net.AddTemporalEdge(k++, "e", "b");

                temporal_net.AddTemporalEdge(k++, "c", "e");
                temporal_net.AddTemporalEdge(k++, "e", "f");

                temporal_net.AddTemporalEdge(k++, "b", "e");
                temporal_net.AddTemporalEdge(k++, "e", "g");

                temporal_net.AddTemporalEdge(k++, "c", "e");
                temporal_net.AddTemporalEdge(k++, "e", "f");

                temporal_net.AddTemporalEdge(k++, "c", "e");
                temporal_net.AddTemporalEdge(k++, "e", "f");
            }

            Console.Write("Saving to file ...");
            TemporalNetwork.SaveToFile(out_file, temporal_net);
            Console.WriteLine(" done.");
        }
    }
}
