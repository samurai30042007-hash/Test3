using System.Diagnostics;
using Test3;

namespace Test3.Tests;

public class StorageTests
{
    [Fact]
    public void TestDb()
    {
        string connectionString = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION_STRING") ?? throw new InvalidOperationException("Test DB is not configured.");

    }
}