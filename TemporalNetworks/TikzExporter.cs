using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TemporalNetworks
{
    /// <summary>
    /// Can be used to export temporal networks to tikz source files. With this, the visualizations of the paper where produced.
    /// </summary>
    public class TikzExporter
    {

        /// <summary>
        /// Creates a TikZ representation of the temporal unfolding of the temporal network
        /// </summary>
        /// <param name="path">The path to which to write the tikz file</param>
        /// <param name="temp_net">The temporal network that shall be exported</param>
        public static void CreateTikzUnfolding(string path, string between_node, TemporalNetwork temp_net)
        {
            WeightedNetwork net = WeightedNetwork.FromTemporalNetwork(temp_net);

            StringBuilder strB = new StringBuilder();
            strB.AppendLine("\\begin{tikzpicture}[->,>=stealth',auto,scale=0.5, every node/.style={scale=0.9}]");
            strB.AppendLine("\\tikzstyle{node} = [fill=lightgray,text=black,circle]");
            strB.AppendLine("\\tikzstyle{v} = [fill=black,text=white,circle]");
            strB.AppendLine("\\tikzstyle{dst} = [fill=lightgray,text=black,circle]");
            strB.AppendLine("\\tikzstyle{lbl} = [fill=white,text=black,circle]");

            string last = "";
            foreach (string v in net.Vertices)
            {
                if (last == "")
                    strB.AppendLine(" \node[lbl]                     (" + v + "-0)   {$" + v + "$};");
                else
                    strB.AppendLine("  \node[lbl,right=0.5cm of n2-0] (" + v + "-0)   {$" + v + "$};");
                last = v;
            }

            strB.AppendLine("\\setcounter{a}{0}");
            strB.AppendLine("\\foreach \\number in {1,...," + temp_net.Count + 1 + "}{");
            strB.AppendLine("\\setcounter{a}{\\number}");
            strB.AppendLine("\\addtocounter{a}{-1}");
            strB.AppendLine("\\pgfmathparse{\\thea}");
            foreach (string v in net.Vertices)
            {
                if (v != between_node)
                    strB.AppendLine("\\node[node,below=0.3cm of " + v + "-\\pgfmathresult]   (" + v + "-\\number) {};");
                else
                    strB.AppendLine("\\node[v,below=0.3cm of " + v + "-\\pgfmathresult]     (" + v + "-\\number) {};");
            }
            strB.AppendLine("\\node[lbl,left=0.5cm of " + net.Vertices.ElementAt(0) + "-\\number]    (col-\\pgfmathresult) {$t=$\\number};");
            strB.AppendLine("}");
            strB.AppendLine("\\path[->,thick]");
            int i = 1;
            foreach (var t in temp_net.Keys)
            {
                foreach (var edge in temp_net[t])
                {
                    strB.AppendLine("(" + edge.Item1 + "-" + i + ") edge (" + edge.Item2 + "-" + (i + 1) + ")");
                    i++;
                    if (i % 2 == 1)
                        strB.AppendLine();
                }
            }
            strB.Append(";");
            strB.AppendLine("\\end{tikzpicture} }");

            System.IO.File.WriteAllText(path, strB.ToString());
        }
    }
}
