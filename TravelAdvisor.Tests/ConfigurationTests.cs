using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
// using Microsoft.Extensions.Configuration;

namespace TravelAdvisor.Tests
{
    public class ConfigurationTests : BaseTest
    {
        
        
        [Fact]
        public void CanReadFromConfiguration()
        {
            var apiKeyString = Configuration["GoogleApiKey"];
            Assert.NotNull(apiKeyString);
            Assert.NotEmpty(apiKeyString);
          //  Assert.Equal(apiKeyString, "xxXXB21BHUxh7NP6TXC42QqiWeoPSLtWi2ziztQ");
            
        }
        
        [Fact]
        public void CanMock()
        {
            var mock = new Mock<IConfiguration>();
            mock.Setup(x => x[It.IsNotNull<string>()]).Returns("Dummy Value");
            var configuration = mock.Object;
            var value = configuration["Dummy Key"];
            Assert.NotNull(value);
            Assert.NotEmpty(value);
        }
    }
}