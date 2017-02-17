using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PlayoutTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var playRange = new PlayRange(TimeSpan.Zero, TimeSpan.Zero);

            Assert.IsTrue(playRange==new PlayRange());
        }
    }
}
