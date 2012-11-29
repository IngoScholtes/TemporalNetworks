using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TempNet
{

    /// <summary>
    /// Some simple tools for implementing a command line version of the TemporalNetworks tools
    /// </summary>
    class CmdlTools
    {
        /// <summary>
        /// Display dialog to ask whether to overwrite file, if output file already exists
        /// </summary>
        /// <param name="out_file">The file that shall be overwritten</param>
        /// <returns>Whether or not to overwrite the file</returns>
        public static bool PromptExistingFile(string out_file)
        {
            if (System.IO.File.Exists(out_file))
            {
                Console.WriteLine("Warning: Output file \"{0}\" already exists.", out_file);
                Console.Write("Overwrite? [y/n] ");
                if (Console.ReadKey().Key != ConsoleKey.Y)
                {
                    Console.WriteLine();
                    return false;
                }
                System.IO.File.Delete(out_file);
                Console.WriteLine();
            }
            return true;
        }
    }
}
