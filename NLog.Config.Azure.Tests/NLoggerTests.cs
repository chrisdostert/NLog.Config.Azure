using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using NLog.Targets;

namespace NLog.Config.Azure.Tests
{
    [TestClass]
    public class NLoggerTests
    {
        #region Ctors

        [TestInitialize]
        public void Initialize()
        {
            new AzureLoggingConfiguration().Init();

            _logger = LogManager.GetCurrentClassLogger();
        }

        #endregion

        #region Fields

        private Logger _logger;

        #endregion

        #region Methods

        [TestMethod]
        public void ThrowExceptionsTest()
        {
            #region Arrange

            var willThrowExceptions = LogManager.ThrowExceptions;
            
            #endregion

            #region Act

            #endregion

            #region Assert

            Assert.IsTrue(willThrowExceptions);

            #endregion
        }

        #endregion
    }
}
