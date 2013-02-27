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
