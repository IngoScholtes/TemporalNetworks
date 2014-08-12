using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TempNet
{
    /// <summary>
    /// A command line tool that makes all functions of the temporal network framework accessible
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: TempNet [function] [options]");
                Console.WriteLine("\n[function] can be any of the following ...\n");
                Console.WriteLine("\t\t distribution \t Compute the betweenness preference distribution of a temporal network");
                Console.WriteLine("\t\t spreading \t Run SI spreading process on a temporal network");
                Console.WriteLine("\t\t rw \t\t Run random walk process on a weighted aggregate network");
                Console.WriteLine("\t\t tikz \t\t Generate a tikz visualization of a time-unfolded temporal network");
                Console.WriteLine("\t\t visualize \t\t Generate a bitmap visualization of a time-unfolded temporal network");
                Console.WriteLine("\t\t example \t Create a simple example for a temporal network");
                Console.WriteLine("\t\t scc \t Removes all edges that do not belong to the largest strongly connected component of the second order aggregate network");
                Console.WriteLine("\t\t match \t\t Check whether the aggregated version of two temporal networks are identical");
                Console.WriteLine("\t\t aggregate \t Create a weighted aggregate network from a temporal network");
                Console.WriteLine("\t\t G2_null \t Create a weighted second order network corresponding to a Markovian null model");
                Console.WriteLine("\t\t shuffle \t Shuffle edges or two paths of a temporal network");
                Console.WriteLine("\t\t T2 \t\t Writes a matrix of two-path transition probabilities to a file"); 
                Console.WriteLine("");
                Console.WriteLine("Specify any of the functions without further parameter to get help on available [options].");
                return; 
            }
            string function = args[0];

            // Pass to individual function components.
            if (function == "distribution")
                Fn_Distribution.Run(args);
            else if (function == "spreading")
                Fn_Spreading.Run(args);
            else if (function == "rw")
                Fn_RandomWalk.Run(args);
            else if (function == "tikz")
                Fn_Tikz.Run(args);
            else if (function == "example")
                Fn_Example.Run(args);
            else if (function == "match")
                Fn_Match.Run(args);
            else if (function == "aggregate")
                Fn_Aggregate.Run(args);
            else if (function == "stats")
                Fn_Stats.Run(args);
            else if (function == "shuffle")
                Fn_Shuffle.Run(args);
            else if (function == "T2")
                Fn_T2.Run(args);
            else if (function == "scc")
                Fn_Filter.Run(args);
            else if (function == "visualize")
                Fn_Visualize.Run(args);
            else if (function == "G2_null")
                Fn_G2Null.Run(args);
            else
                Console.WriteLine("Unknown function {0}", function);
        }
    }
}
