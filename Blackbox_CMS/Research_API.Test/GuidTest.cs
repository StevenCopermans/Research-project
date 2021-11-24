using System;
using System.Globalization;
using Xunit;

namespace ResearchProject.test
{
    public class UnitTests
    {
        [Theory]
        [InlineData("897447B1-CEFE-41A6-BD41-091B24BB3FF7")]
        [InlineData("89744731-CEFE-41A6-BD41-091B24BB3FF7")]
        public void GuidTestSucces(string value)
        {
            Research_API.Controllers.rest controller = new Research_API.Controllers.rest();
            dynamic testData = controller.ConvertType(value, "System.Guid");

            Assert.False(testData == null, value);
        }
        [Theory]
        [InlineData("8974471-CEFE-41A6-BD41-091B24BB3FF7")]
        [InlineData("897447B1-CEFE-41A6-BD41-091B24BB3FG7")]
        public void GuidTestFail(string value)
        {
            Research_API.Controllers.rest controller = new Research_API.Controllers.rest();
            dynamic testData = controller.ConvertType(value, "System.Guid");

            Assert.True(testData == null, value);
        }


        [Theory]
        [InlineData("2147483647")]
        [InlineData("-2147483647")]
        public void Int32TestSucces(string value)
        {
            Research_API.Controllers.rest controller = new Research_API.Controllers.rest();
            dynamic testData = controller.ConvertType(value, "System.Int32");

            Assert.False(testData == null, value);
        }
        [Theory]
        [InlineData("3147483647")]
        [InlineData("-3147483647")]
        public void Int32TestFail(string value)
        {
            Research_API.Controllers.rest controller = new Research_API.Controllers.rest();
            dynamic testData = controller.ConvertType(value, "System.Int32");

            Assert.True(testData == null, value);
        }
    }
}
