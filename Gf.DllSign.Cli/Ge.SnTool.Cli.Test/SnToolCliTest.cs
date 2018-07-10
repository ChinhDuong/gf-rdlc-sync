using Xunit;
using Shouldly;

namespace Gf.SnTool.Cli.Test
{
    public class SnToolCliTest
    {
        [Fact]
        public void Should_Success_When_ProjectIsWithoutFolder()
        {
            string[] input = 
            {
                "-f",
                "TestData\\ProjectWithoutFolder.zip",
                "-o",
                "D:\\temp\\rdlc.signed",
                "-v",
                "1.2.500.0",
            };
            int exitCode = Gf.SnTool.Cli.Program.Main(input);
            exitCode.ShouldBe(0);
        }

        [Fact]
        public void Should_Success_When_NotSignAssemblyKeyInProject()
        {
            string[] input =
            {
                "-f",
                "TestData\\NotSignAssemblyKeyInProject.zip",
                "-o",
                "D:\\temp\\rdlc.signed",
                "-v",
                "1.2.500.0",
            };
            int exitCode = Gf.SnTool.Cli.Program.Main(input);
            exitCode.ShouldBe(0);
        }

        [Fact]
        public void Should_Success_When_ProjectWithFolder()
        {
            string[] input =
            {
                "-f",
                "TestData\\ProjectWithFolder.zip",
                "-o",
                "D:\\temp\\rdlc.signed",
                "-v",
                "1.2.500.0",
            };
            int exitCode = Gf.SnTool.Cli.Program.Main(input);
            exitCode.ShouldBe(0);
        }

        [Fact]
        public void Should_Failed_When_NotFoundPackages()
        {
            string[] input =
            {
                "-f",
                "TestData\\NotFoundPackages.zip",
                "-o",
                "D:\\temp\\rdlc.signed",
                "-v",
                "1.2.500.0",
            };
            int exitCode = Gf.SnTool.Cli.Program.Main(input);
            exitCode.ShouldBe(1);
        }

        [Fact]
        public void Should_Success_When_ProjectRequiredToGenCode128()
        {
            string[] input =
            {
                "-f",
                "TestData\\ProjectRequiredToGenCode128.zip",
                "-o",
                "D:\\temp\\rdlc.signed",
                "-v",
                "1.2.500.0",
            };
            int exitCode = Gf.SnTool.Cli.Program.Main(input);
            exitCode.ShouldBe(0);
        }
    }
}
