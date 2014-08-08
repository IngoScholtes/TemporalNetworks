using TemporalNetworks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace TemporalNetworksTest
{    
    
    /// <summary>
    ///This is a test class for TemporalNetworkTest and is intended
    ///to contain all TemporalNetworkTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TemporalNetworkTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestMethod()]
        public void AbsoluteTimeTest()
        {
            TemporalNetwork test = ExampleData.GetTestNetwork3();

            // Default behavior is relative time 
            test.ReduceToTwoPaths();

            Assert.IsTrue(test.TwoPathWeights.ContainsKey("a,c,e") && test.TwoPathWeights["a,c,e"] == 1);

            test = ExampleData.GetTestNetwork3();
            test.ReduceToTwoPaths(false, true);

            Assert.IsFalse(test.TwoPathWeights.ContainsKey("a,c,e"));
        }

        /// <summary>
        ///A test for AddTemporalEdge
        ///</summary>
        [TestMethod()]
        public void AddTemporalEdgeTest()
        {
            TemporalNetwork target = new TemporalNetwork();
            target.AddTemporalEdge(0, "a", "b");
            Assert.IsTrue(target[0].Contains(new Tuple<string,string>("a", "b")));            
        }

        /// <summary>
        ///A test for FromEdgeSequence
        ///</summary>
        [TestMethod()]
        public void FromEdgeSequenceTest()
        {
            List<Tuple<string, string>> sequence = new List<Tuple<string, string>>();

            sequence.Add(new Tuple<string, string>("a", "b"));
            sequence.Add(new Tuple<string, string>("b", "c"));

            TemporalNetwork expected = new TemporalNetwork();
            expected.AddTemporalEdge(0, "a", "b");
            expected.AddTemporalEdge(1, "b", "c");
            TemporalNetwork actual;
            actual = TemporalNetwork.FromEdgeSequence(sequence);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for the handling of temporal edge weights/counts
        /// </summary>
        [TestMethod()]
        public void WeightTest()
        {

            TemporalNetwork net1 = ExampleData.GetTestNetwork_weighted_1();
            TemporalNetwork net2 = ExampleData.GetTestNetwork_weighted_2();

            Assert.AreEqual(net1.TwoPathWeights["a,b,c"], net2.TwoPathWeights["a,b,c"]);
            Assert.AreEqual(net1.TwoPathWeights["a,b,a"], net2.TwoPathWeights["a,b,a"]);
        }

        /// <summary>
        ///A test for Preprocess
        ///</summary>
        [TestMethod()]
        public void ReduceToTwoPathsTest()
        {
            TemporalNetwork test = ExampleData.GetTestNetwork();
            test.ReduceToTwoPaths();

            // There should be 12 two paths
            Assert.AreEqual(test.TwoPathCount, 12);

            // And 21 edges 
            Assert.AreEqual(test.EdgeCount, 21);            

            // The stripped network should contain 20 time steps
            Assert.AreEqual(test.Length, 20);
           
            Assert.AreEqual(test.MaxGranularity, 1);

            Assert.AreEqual(test.MinGranularity, 2);
            
            // The aggregate network should contain 8 weighted edges with a cumulative weight that matches 2 * twoPathCount
            Assert.AreEqual(test.AggregateNetwork.EdgeCount, 8);
            Assert.AreEqual(test.AggregateNetwork.CumulativeWeight, 22);

            // The second-order aggregate network should countain edges representing 6 different two-paths ... 
            Assert.AreEqual(test.SecondOrderAggregateNetwork.EdgeCount, 6);
            // ... with a total weight of 11
            Assert.AreEqual(test.SecondOrderAggregateNetwork.CumulativeWeight, 11);

            Assert.AreEqual(test.SecondOrderAggregateNetwork[new Tuple<string,string>("(c;e)","(e;f)")], 5.5d);
            Assert.AreEqual(test.SecondOrderAggregateNetwork[new Tuple<string, string>("(g;e)", "(e;f)")], 0.5d);
            Assert.AreEqual(test.SecondOrderAggregateNetwork[new Tuple<string, string>("(e;f)", "(f;e)")], 1d);
            Assert.AreEqual(test.SecondOrderAggregateNetwork[new Tuple<string, string>("(f;e)", "(e;b)")], 1d);
            Assert.AreEqual(test.SecondOrderAggregateNetwork[new Tuple<string, string>("(a;e)", "(e;g)")], 2d);
            Assert.AreEqual(test.SecondOrderAggregateNetwork[new Tuple<string, string>("(b;e)", "(e;g)")], 1d);

            // Check whether all the weights are correctly computed ... 
            Assert.AreEqual(test.AggregateNetwork[new Tuple<string, string>("e", "f")], 7, "The weight of egde (e,f) in the aggregate network is wrong!");
            Assert.AreEqual(test.AggregateNetwork[new Tuple<string, string>("e", "b")], 1, "The weight of egde (e,b) in the aggregate network is wrong!");
            Assert.AreEqual(test.AggregateNetwork[new Tuple<string, string>("b", "e")], 1, "The weight of egde (b,e) in the aggregate network is wrong!");
            Assert.AreEqual(test.AggregateNetwork[new Tuple<string, string>("g", "e")], 1d/2d, "The weight of egde (g,e) in the aggregate network is wrong!");
            Assert.AreEqual(test.AggregateNetwork[new Tuple<string, string>("e", "g")], 3, "The weight of egde (e,g) in the aggregate network is wrong!");
            Assert.AreEqual(test.AggregateNetwork[new Tuple<string, string>("f", "e")], 2, "The weight of egde (f,e) in the aggregate network is wrong!");
            Assert.AreEqual(test.AggregateNetwork[new Tuple<string, string>("a", "e")], 2, "The weight of egde (a,e) in the aggregate network is wrong!");
            Assert.AreEqual(test.AggregateNetwork[new Tuple<string, string>("c", "e")], 5 + 1d/2d, "The weight of egde (c,e) in the aggregate network is wrong!");

            Assert.AreEqual(test.AggregateNetwork.MaxWeight, 7, "The maximum weight in the aggregate network is wrong!");
            Assert.AreEqual(test.AggregateNetwork.MinWeight, 0.5d, "The minimum weight in the aggregate network is wrong!");

            test = ExampleData.GetTestNetwork2();
            test.ReduceToTwoPaths();

            Assert.AreEqual(test.TwoPathCount, 7);

            Assert.AreEqual(test.Length, 3);

            Assert.AreEqual(test.MaxGranularity, 1);

            Assert.AreEqual(test.MinGranularity, 3);

            Assert.AreEqual(test.AggregateNetwork[new Tuple<string,string>("a", "b")], 1);
            Assert.AreEqual(test.AggregateNetwork[new Tuple<string, string>("b", "c")], 1d + 1d/6d + 1d/6d + 1d/6d);
            Assert.AreEqual(test.AggregateNetwork[new Tuple<string, string>("d", "c")], 1d / 2d);
            Assert.AreEqual(test.AggregateNetwork[new Tuple<string, string>("c", "d")], 1d / 3d);
            Assert.AreEqual(test.AggregateNetwork[new Tuple<string, string>("c", "b")], 1d / 3d);
            Assert.AreEqual(test.AggregateNetwork[new Tuple<string, string>("c", "a")], 1d / 3d);                        
        }

        /// <summary>
        ///A test for time reversal
        ///</summary>
        [TestMethod()]
        public void TimeReversalTest()
        {
            TemporalNetwork test = ExampleData.GetTestNetwork();
            test.ReduceToTwoPaths(true);

            Assert.AreEqual(test.TwoPathCount, 10, "Number of paths in time-reversed temporal network is incorrect!");


        }

        /// <summary>
        /// A test for the save and load procedures
        /// </summary>
        [TestMethod()]
        public void LoadTemporalNetworkTest()
        {
            TemporalNetwork net = ExampleData.GetTestNetwork();
            TemporalNetwork.SaveToFile("test.net", net);

            TemporalNetwork net2 = TemporalNetwork.ReadFromFile("test.net");
            Assert.AreEqual(net, net2);
        }

        [TestMethod()]
        public void AggregateTimeTest()
        {
            TemporalNetwork net = ExampleData.GetTestNetwork3();
            net.AggregateTime(3);
        }
    }
}
