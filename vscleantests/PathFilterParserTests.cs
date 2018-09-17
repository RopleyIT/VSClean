using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vscleanlib;

namespace vscleantests
{
    [TestClass]
    public class PathFilterParserTests
    {
        [TestMethod]
        public void CanCreateFromString()
        {
            var pfp = new PathFilterParser("**/fred.txt\r\n*.joe");
            Assert.IsInstanceOfType(pfp, typeof(PathFilterParser));
        }

        [DataTestMethod]
        [DataRow("**/fred.txt", "fred.txt", false)]
        [DataRow("**/fred.txt", "joe/fred.txt", false)]
        [DataRow("**/fred.txt", "/bill/joe/fred.txt", false)]
        [DataRow("fred.txt", "fred.txt", false)]
        [DataRow("fred.txt", "joe/fred.txt", false)]
        [DataRow("fred.txt", "/bill/joe/fred.txt", false)]
        [DataRow("/fred.txt", "fred.txt", false)]
        [DataRow("/fred.txt", "joe/fred.txt", true)]
        [DataRow("/fred.txt", "/bill/joe/fred.txt", true)]
        [DataRow("*.txt", "fred.txt", false)]
        [DataRow("*.txt", "joe/bill.txt", false)]
        [DataRow("*.txt", "/fred/joe/bill.txt", false)]
        [DataRow("/*.txt", "fred.txt", false)]
        [DataRow("/*.txt", "joe/fred.txt", true)]
        [DataRow("/*.txt", "/bill/joe/fred.txt", true)]
        [DataRow("**/fred.t?t", "fred.txt", false)]
        [DataRow("**/fred.t?t", "joe/fred.tst", false)]
        [DataRow("**/fred.t?t", "/bill/joe/fred.tpt", false)]
        [DataRow("**/fred.t[0-4]t", "fred.txt", true)]
        [DataRow("**/fred.t[0-4]t", "joe/fred.t0t", false)]
        [DataRow("**/fred.t[0-4]t", "/bill/joe/fred.t4t", false)]
        [DataRow("**/fred.t[0x4]t", "fred.txt", false)]
        [DataRow("**/fred.t[0x4]t", "joe/fred.t0t", false)]
        [DataRow("**/fred.t[0x4]t", "/bill/joe/fred.t3t", true)]
        [DataRow("**/fred.t[!0-4]t", "fred.txt", false)]
        [DataRow("**/fred.t[!0-4]t", "joe/fred.t0t", true)]
        [DataRow("**/fred.t[!0-4]t", "/bill/joe/fred.t4t", true)]
        [DataRow("**/fred.t[!0x4]t", "fred.txt", true)]
        [DataRow("**/fred.t![0x4]t", "joe/fred.t0t", true)]
        [DataRow("**/fred.t[!0x4]t", "/bill/joe/fred.t3t", false)]
        [DataRow("joe/**/fred.txt", "joe/fred.txt", false)]
        [DataRow("joe/**/fred.txt", "joe/bill/fred.txt", false)]
        [DataRow("joe/**/fred.txt", "joe/bill/tom/fred.txt", false)]
        [DataRow("joe/**/fred.txt", "fred.txt", true)]
        [DataRow("joe/**/fred.txt", "bill/fred.txt", true)]
        [DataRow("joe/**/fred.txt", "/joe/bill/tom/fred.txt", false)]
        [DataRow("joe/**", "joe/fred", false)]
        [DataRow("joe/**", "joe/fred/bill", false)]
        [DataRow("joe/**", "joe", true)]
        public void PatternTests(string glob, string fnInc, bool included)
        {
            var pfp = new PathFilterParser(glob);
            Assert.IsTrue(pfp.Accepts(fnInc, false) == included);
        }

        [DataTestMethod]
        [DataRow("joe/", "joe", true, false)]
        [DataRow("joe/", "joe", false, true)]
        [DataRow("joe", "joe", true, false)]
        [DataRow("joe", "joe", false, false)]
        public void DirectoryPatternTests(string glob, string fnInc, bool isDir, bool included)
        {
            var pfp = new PathFilterParser(glob);
            Assert.IsTrue(pfp.Accepts(fnInc, isDir) == included);
        }

        [DataTestMethod]
        [DataRow("joe/*.txt\r\n!joe/*x.txt\r\njoe/xx.txt", "joe/y.txt", false)]
        [DataRow("joe/*.txt\r\n!joe/*x.txt\r\njoe/xx.txt", "joe/x.txt", true)]
        [DataRow("joe/*.txt\r\n!joe/*x.txt\r\njoe/xx.txt", "joe/yx.txt", true)]
        [DataRow("joe/*.txt\r\n!joe/*x.txt\r\njoe/xx.txt", "joe/xx.txt", false)]
        public void PatternLists(string glob, string fnInc, bool included)
        {
            var pfp = new PathFilterParser(glob);
            Assert.IsTrue(pfp.Accepts(fnInc, false) == included);
        }
    }
}
