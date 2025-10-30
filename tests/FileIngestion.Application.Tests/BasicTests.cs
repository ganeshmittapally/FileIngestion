using Xunit;

namespace FileIngestion.Application.Tests;

public class BasicTests
{
    [Fact]
    public void Sanity()
    {
        Assert.Equal(2, 1 + 1);
    }
}
