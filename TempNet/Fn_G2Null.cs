using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TempNet
{
    public class Fn_G2Null
    {

        static System.Globalization.NumberFormatInfo nf = System.Globalization.CultureInfo.GetCultureInfo("en-US").NumberFormat;

        public static void Run(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: TempNet G2_null [weighted_edge_list.agg.1.edges] [output_file]");
                return;
            }
            string out_file = args[2];
            string input_file = args[1];       
            
            if(!CmdlTools.PromptExistingFile(out_file))
                return;

            Console.Write("Reading edge list...");
            string[] agg1 = System.IO.File.ReadAllLines(input_file);
            Console.WriteLine("done.");

            // Prepare output file
            StringBuilder output = new StringBuilder();
            output.AppendLine("source target weight");

            Console.Write("Computing cumulative edge weights...");
            // Compute the cumulative weight of all outgoing edges for all nodes
            Dictionary<string, float> sum_out_w = new Dictionary<string, float>();
            for (int i = 1; i < agg1.Length; i++)
            {
                string[] fields = agg1[i].Split(' ');
                if (!sum_out_w.ContainsKey(fields[0]))
                    sum_out_w[fields[0]] = float.Parse(fields[2], nf);
                else
                    sum_out_w[fields[0]] += float.Parse(fields[2], nf);
            }
            Console.WriteLine("done.");

            Console.Write("Computing null model edge weights...");
            // Compute the null model second-order edge weights for all second-order edges (i.e. two-paths)
            for (int i = 1; i < agg1.Length; i++)
                for (int j = 1; j < agg1.Length; j++)
                {
                    string[] fields1 = agg1[i].Split(' ');
                    string[] fields2 = agg1[j].Split(' ');

                    // This is a two-path candidate
                    string a = fields1[0];
                    string b = fields1[1];
                    string c = fields2[0];
                    string d = fields2[1];

                    // It is a possible two-path if b==c
                    if (b == c)
                    {
                        float w_ab = float.Parse(fields1[2], nf);
                        float w_cd = float.Parse(fields2[2], nf);
                        float null_model_weight = 0.5f * w_ab * w_cd / (sum_out_w[c]);
                        output.AppendLine(string.Format(nf, "({0};{1}) ({1};{2}) {3:0.0000}", a, b, d, null_model_weight));
                    }
                }
            Console.WriteLine("done.");

            // Write to file
            Console.Write("Writing output file...");
            System.IO.File.WriteAllText(out_file, output.ToString());
            Console.WriteLine("done.");

        }
    }
}
