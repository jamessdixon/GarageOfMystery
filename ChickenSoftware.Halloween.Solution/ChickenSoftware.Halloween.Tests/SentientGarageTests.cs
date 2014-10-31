using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChickenSoftware.Halloween.Tests
{
    [TestClass]
    public class SentientGarageTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            SentientGarage garage = new SentientGarage();
            garage.initializeKinect();
        }
    }
}
