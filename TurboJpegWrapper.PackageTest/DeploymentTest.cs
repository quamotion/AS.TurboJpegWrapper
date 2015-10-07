using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace TurboJpegWrapper.PackageTest
{
    [TestFixture]
    public class DeploymentTest
    {
        [Test]
        public void AllLibrariesDeployed()
        {
            var path = Assembly.GetExecutingAssembly().Location;
            var x86Path = Path.Combine(Path.GetDirectoryName(path), "x86");
            var x64Path = Path.Combine(Path.GetDirectoryName(path), "x64");

            var wrapperLocation = typeof (TJCompressor).Assembly.Location;

            Assert.That(File.Exists(wrapperLocation), $"File {wrapperLocation} does not exist");
            Assert.That(Directory.Exists(x86Path), $"Directory {x86Path} does not exist");
            Assert.That(Directory.Exists(x64Path), $"Directory {x64Path} does not exist");
            Assert.That(File.Exists(Path.Combine(x86Path, "turbojpeg.dll")), $"File turbojpeg.dll in the {x86Path} does not exist");
            Assert.That(File.Exists(Path.Combine(x64Path, "turbojpeg.dll")), $"File turbojpeg.dll in the {x64Path} does not exist");
        }
    }
}
