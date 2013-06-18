using TemporalNetworks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace TemporalNetworksTest
{    
    /// <summary>
    ///This is a test class for BetweennessPrefTest and is intended
    ///to contain all BetweennessPrefTest Unit Tests
    ///</summary>
    [TestClass()]
    public class BetweennessPrefTest
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

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for GetBetweennessPrefMatrix
        ///</summary>
        [TestMethod()]
        public void GetBetweennessPrefMatrixTest()
        {
            TemporalNetwork temp_net = ExampleData.GetTestNetwork();
            Dictionary<string, int> index_pred = null;
            Dictionary<string, int> index_succ = null;
                        
            double[,] actual;
            actual = BetweennessPref.GetBetweennessPrefMatrix(temp_net, "e", out index_pred, out index_succ);

            Assert.IsNotNull(index_pred);
            Assert.IsNotNull(index_succ);
            Assert.IsNotNull(actual);

            // Check whether all entries in the normalized betweenness preference matrix returned match the expected values
            Assert.AreEqual(0d,         actual[index_pred["a"], index_succ["b"]]);
            Assert.AreEqual(0d,         actual[index_pred["b"], index_succ["b"]]);
            Assert.AreEqual(0d,         actual[index_pred["c"], index_succ["b"]]);
            Assert.AreEqual(1d / 10d,   actual[index_pred["f"], index_succ["b"]]);
            Assert.AreEqual(0d,         actual[index_pred["g"], index_succ["b"]]);

            Assert.AreEqual(actual[index_pred["a"], index_succ["f"]], 0d);
            Assert.AreEqual(actual[index_pred["b"], index_succ["f"]], 0d);
            Assert.AreEqual(actual[index_pred["c"], index_succ["f"]], 11d / 20d);
            Assert.AreEqual(actual[index_pred["f"], index_succ["f"]], 0d);
            Assert.AreEqual(actual[index_pred["g"], index_succ["f"]], 1d / 20d);

            Assert.AreEqual(actual[index_pred["a"], index_succ["g"]], 2d / 10d);
            Assert.AreEqual(actual[index_pred["b"], index_succ["g"]], 1d / 10d);
            Assert.AreEqual(actual[index_pred["c"], index_succ["g"]], 0d);
            Assert.AreEqual(actual[index_pred["f"], index_succ["g"]], 0d);
            Assert.AreEqual(actual[index_pred["g"], index_succ["g"]], 0d);

            Assert.AreEqual(BetweennessPref.GetBetweennessPref(temp_net, "e"), 1.2954618442383219d);

            actual = BetweennessPref.GetBetweennessPrefMatrix(temp_net, "f", out index_pred, out index_succ);

            Assert.IsNotNull(index_pred);
            Assert.IsNotNull(index_succ);
            Assert.IsNotNull(actual);

            Assert.AreEqual(actual[index_pred["e"], index_succ["e"]], 1d);
        }

        /// <summary>
        ///A test for GetUncorrelatedBetweennessPrefMatrix
        ///</summary>
        [TestMethod()]
        public void GetUncorrelatedBetweennessPrefMatrixTest()
        {
            TemporalNetwork temp_net = ExampleData.GetTestNetwork();
            Dictionary<string, int> index_pred = null;
            Dictionary<string, int> index_succ = null;

            double[,] actual = null;
            actual = BetweennessPref.GetUncorrelatedBetweennessPrefMatrix(temp_net, "e", out index_pred, out index_succ);

            Assert.IsNotNull(index_pred);
            Assert.IsNotNull(index_succ);
            Assert.IsNotNull(actual);

            // Check whether all entries in the normalized betweenness preference matrix returned match the expected values
            Assert.AreEqual(actual[index_pred["a"], index_succ["b"]], (2d / 11d) * (1d / 10d) );
            Assert.AreEqual(actual[index_pred["b"], index_succ["b"]], (1d / 11d) * (1d / 10d) );
            Assert.AreEqual(actual[index_pred["c"], index_succ["b"]], (6d / 11d) * (1d / 10d) );
            Assert.AreEqual(actual[index_pred["f"], index_succ["b"]], (1d / 11d) * (1d / 10d));
            Assert.AreEqual(actual[index_pred["g"], index_succ["b"]], (1d / 11d) * (1d / 10d));

            Assert.AreEqual(actual[index_pred["a"], index_succ["f"]], (2d / 11d) * (6d / 10d));
            Assert.AreEqual(actual[index_pred["b"], index_succ["f"]], (1d / 11d) * (6d / 10d));
            Assert.AreEqual(actual[index_pred["c"], index_succ["f"]], (6d / 11d) * (6d / 10d));
            Assert.AreEqual(actual[index_pred["f"], index_succ["f"]], (1d / 11d) * (6d / 10d));
            Assert.AreEqual(actual[index_pred["g"], index_succ["f"]], (1d / 11d) * (6d / 10d));

            Assert.AreEqual(actual[index_pred["a"], index_succ["g"]], (2d / 11d) * (3d / 10d));
            Assert.AreEqual(actual[index_pred["b"], index_succ["g"]], (1d / 11d) * (3d / 10d));
            Assert.AreEqual(actual[index_pred["c"], index_succ["g"]], (6d / 11d) * (3d / 10d));
            Assert.AreEqual(actual[index_pred["f"], index_succ["g"]], (1d / 11d) * (3d / 10d));
            Assert.AreEqual(actual[index_pred["g"], index_succ["g"]], (1d / 11d) * (3d / 10d));
        }


        [TestMethod()]
        public void NoBWPTest()
        {
            TemporalNetwork net = ExampleData.GetTestNetwork5();

            Dictionary<string, int> index_pred = null;
            Dictionary<string, int> index_succ = null;

            double[,] actual = null;
            actual = BetweennessPref.GetBetweennessPrefMatrix(net, "B", out index_pred, out index_succ);

            double bwp = BetweennessPref.GetBetweennessPref(net, "B");
            Assert.AreEqual(bwp, 0d);
        }

        [TestMethod()]
        public void BWPTest()
        {
            TemporalNetwork net = ExampleData.GetTestNetwork6();

            Dictionary<string, int> index_pred = null;
            Dictionary<string, int> index_succ = null;

            double[,] actual = null;
            actual = BetweennessPref.GetBetweennessPrefMatrix(net, "B", out index_pred, out index_succ);

            double bwp = BetweennessPref.GetBetweennessPref(net, "B");
        }
    }
}
