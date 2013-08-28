using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TemporalNetworks;
using System.Drawing;

namespace TempNet
{
    class Fn_Visualize
    {
        public static void Run(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: TempNet visualize [temporal_network_file] [output_png] [square=TRUE] [border=5]");
                return;
            }
            string out_file = args[2];

            bool square = true;
            int border = 5;

            if (args.Length >= 4)
                square = bool.Parse(args[3]);

            if (args.Length >= 5)
                border = int.Parse(args[4]);

            if (!CmdlTools.PromptExistingFile(out_file))
            {
                Console.WriteLine("User opted to exit.");
                return;
            }

            Console.Write("Reading temporal network ...");
            TemporalNetwork temp_net = TemporalNetwork.ReadFromFile(args[1]);
            Console.WriteLine("done.");
            
            CreateBitmap(temp_net, out_file, square, border);
            Console.WriteLine(" done.");
        }

        public static void CreateBitmap(TemporalNetwork net, string output_path, bool square = true, int border_width=5)
        {
            int columns = 1;
            if(square)
                columns = ((int)Math.Sqrt(net.AggregateNetwork.VertexCount * net.Length)) / net.VertexCount + 1;
            int width = (border_width + net.AggregateNetwork.VertexCount) * columns;
            int height = (int) Math.Ceiling( (double) net.Length / (double) columns);
            Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Console.WriteLine("Exporting to png, output dimensions: {0} x {1}", width, height);

            Color[] colorWheel = new Color[] {Color.Red, Color.Green, Color.LightBlue, Color.Orange, Color.Blue, Color.Brown, Color.DarkViolet};
            Random r = new Random();

            Dictionary<string, int> vertex_map = new Dictionary<string, int>();
            int i = 0, line = 0;
            foreach (var v in net.AggregateNetwork.Vertices)
                vertex_map[v] = i++;
            

            foreach(int t in net.Keys)
            {
                int y = line % height;
                int x = (line / height) * (net.AggregateNetwork.VertexCount + border_width);
                int c = r.Next(colorWheel.Length);
                foreach (var edge in net[t])
                {
                    bmp.SetPixel(x + vertex_map[edge.Item1], y, colorWheel[c%colorWheel.Length]);
                    bmp.SetPixel(x + vertex_map[edge.Item2], y, colorWheel[c % colorWheel.Length]);
                    c++;
                }
                for (int j = 0; j < border_width; j++)
                    bmp.SetPixel(x+net.AggregateNetwork.VertexCount+j, y, Color.White); 
                line++;
            }
            bmp.Save(output_path, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
