using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TurboJpegWrapper.Tests
{
   static class TestUtils
    {
        public static IEnumerable<Bitmap> GetTestImages(string searchPattern)
        {
            var path = Assembly.GetExecutingAssembly().Location;
            var imagesDir = Path.Combine(Path.GetDirectoryName(path), "images");

            foreach (var file in Directory.EnumerateFiles(imagesDir, searchPattern))
            {
                Bitmap bmp;
                try
                {
                    bmp = (Bitmap)Image.FromFile(file);
                    System.Diagnostics.Debug.WriteLine($"Input file is {file}");
                }
                catch (OutOfMemoryException)
                {
                    continue;
                }
                catch (IOException)
                {
                    continue;
                }
                yield return bmp;
            }
        }

        public static IEnumerable<byte[]> GetTestImagesData(string searchPattern)
        {
            var path = Assembly.GetExecutingAssembly().Location;
            var imagesDir = Path.Combine(Path.GetDirectoryName(path), "images");

            foreach (var file in Directory.EnumerateFiles(imagesDir, searchPattern))
            {
                System.Diagnostics.Debug.WriteLine($"Input file is {file}");
                yield return File.ReadAllBytes(file);
            }
        }
    }
}
