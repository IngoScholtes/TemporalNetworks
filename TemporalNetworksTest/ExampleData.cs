using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TemporalNetworks;

namespace TemporalNetworksTest
{

    /// <summary>
    /// This is a class that builds some sample temporal networks that will be used as test cases
    /// </summary>
    class ExampleData
    {

        /// <summary>
        /// Returns a test network
        /// </summary>
        /// <returns></returns>
        public static TemporalNetwork GetTestNetwork()
        {
            TemporalNetwork temporal_net = new TemporalNetwork();

            // Add edges according to two paths
            temporal_net.AddTemporalEdge(1, "c", "e");
            temporal_net.AddTemporalEdge(2, "e", "f");

            temporal_net.AddTemporalEdge(3, "a", "e");
            temporal_net.AddTemporalEdge(4, "e", "g");

            temporal_net.AddTemporalEdge(5, "c", "e");
            temporal_net.AddTemporalEdge(6, "e", "f");

            temporal_net.AddTemporalEdge(7, "a", "e");
            temporal_net.AddTemporalEdge(8, "e", "g");

            temporal_net.AddTemporalEdge(9, "c", "e");
            temporal_net.AddTemporalEdge(10, "e", "f");
            // Note that the next added edge additionally continues a two-path e -> f -> e
            temporal_net.AddTemporalEdge(11, "f", "e");
            temporal_net.AddTemporalEdge(12, "e", "b");

            // An additional edge that should be filtered during preprocessing ...
            temporal_net.AddTemporalEdge(13, "e", "b");

            // And one case where we have multiple edges in a single time step
            temporal_net.AddTemporalEdge(14, "g", "e");
            temporal_net.AddTemporalEdge(14, "c", "e");
            temporal_net.AddTemporalEdge(15, "e", "f");

            temporal_net.AddTemporalEdge(16, "b", "e");
            temporal_net.AddTemporalEdge(17, "e", "g");

            temporal_net.AddTemporalEdge(18, "c", "e");
            temporal_net.AddTemporalEdge(19, "e", "f");

            temporal_net.AddTemporalEdge(20, "c", "e");
            temporal_net.AddTemporalEdge(21, "e", "f");

            return temporal_net;
        }

        /// <summary>
        /// Returns a second test network
        /// </summary>
        /// <returns></returns>
        public static TemporalNetwork GetTestNetwork2()
        {
            TemporalNetwork temporal_net = new TemporalNetwork();

            temporal_net.AddTemporalEdge(1, "a", "b");
            temporal_net.AddTemporalEdge(2, "b", "c");
            temporal_net.AddTemporalEdge(2, "d", "c");
            temporal_net.AddTemporalEdge(3, "c", "a");
            temporal_net.AddTemporalEdge(3, "c", "b");
            temporal_net.AddTemporalEdge(3, "c", "d");

            return temporal_net;
        }

         /// <summary>
        /// Returns a test network suitable for testing the AggregateWindow method
        /// </summary>
        /// <returns></returns>
        public static TemporalNetwork GetTestNetwork3()
        {
            TemporalNetwork temporal_net = new TemporalNetwork();

            temporal_net.AddTemporalEdge(1, "a", "b");
            temporal_net.AddTemporalEdge(2, "a", "c");
            temporal_net.AddTemporalEdge(7, "c", "e");
            temporal_net.AddTemporalEdge(8, "c", "g");
            temporal_net.AddTemporalEdge(9, "g", "f");
            temporal_net.AddTemporalEdge(10, "f", "h");

            return temporal_net;
        }

        /// <summary>
        /// Returns a test network suitable for testing the AggregateWindow method
        /// </summary>
        /// <returns></returns>
        public static TemporalNetwork GetTestNetwork4()
        {
            TemporalNetwork temporal_net = new TemporalNetwork();

            // Create sequence based on edges in edge graph

            // two-paths ending in (a,b)
            temporal_net.AddTemporalEdge(1, "b", "a");
            temporal_net.AddTemporalEdge(2, "a", "b");

            temporal_net.AddTemporalEdge(3, "c", "a");
            temporal_net.AddTemporalEdge(4, "a", "b");

            // two-paths ending in (b,a)
            temporal_net.AddTemporalEdge(5, "a", "b");
            temporal_net.AddTemporalEdge(6, "b", "a");

            temporal_net.AddTemporalEdge(7, "c", "b");
            temporal_net.AddTemporalEdge(8, "b", "c");

            // two-paths ending in (b,c)
            temporal_net.AddTemporalEdge(9, "a", "b");
            temporal_net.AddTemporalEdge(10, "b", "c");

            // two-paths ending in (c,b)
            temporal_net.AddTemporalEdge(11, "b", "c");
            temporal_net.AddTemporalEdge(12, "c", "b");

            temporal_net.AddTemporalEdge(13, "a", "c");
            temporal_net.AddTemporalEdge(14, "c", "a");  

            temporal_net.AddTemporalEdge(15, "c", "b");
            temporal_net.AddTemporalEdge(16, "b", "a");

            // two-paths ending in (a,c)
            temporal_net.AddTemporalEdge(17, "b", "a");
            temporal_net.AddTemporalEdge(18, "a", "c");

            temporal_net.AddTemporalEdge(19, "a", "c");
            temporal_net.AddTemporalEdge(20, "c", "b");

            temporal_net.AddTemporalEdge(21, "c", "a");
            temporal_net.AddTemporalEdge(22, "a", "c");

            // two-paths ending in (c,a)
            temporal_net.AddTemporalEdge(23, "b", "c");
            temporal_net.AddTemporalEdge(24, "c", "a");                      

            return temporal_net;
        }
    }
}
