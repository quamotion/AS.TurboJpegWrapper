using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;

namespace TurboJpegWrapper.PerformanceTests
{
    static class Program
    {
        private const int Quality = 50;
        private static volatile bool _stop;
        private static readonly ManualResetEventSlim Wait = new ManualResetEventSlim();
        static void Main()
        {

            _stop = false;
            new Thread(SystemDrawingCompression).Start();
            Console.WriteLine("Press [Enter] to continue");
            if (Console.ReadKey().Key == ConsoleKey.Enter)
            {
                _stop = true;
                Wait.Wait();
            }
            _stop = false;
            Wait.Reset();
            new Thread(JpegTurboCompression).Start();
            Console.WriteLine("Press [Enter] to continue");
            if (Console.ReadKey().Key == ConsoleKey.Enter)
            {
                _stop = true;
                Wait.Wait();
            }

            Console.WriteLine("Press [Enter] to exit");
            while (Console.ReadKey().Key != ConsoleKey.Enter)
            {
                Thread.Sleep(1);
            }
        }

        private static void JpegTurboCompression()
        {
            var sw = new Stopwatch();
            var sourceImage = (Bitmap)Image.FromFile(@"D:\Downloads\1.jpg");
            var width = sourceImage.Width;
            var height = sourceImage.Height;
            long average = 0;
            var iterations = 0;
            
            var compressor = new TJCompressor();

            try
            {
                Console.WriteLine("Using libjpeg turbo");
                while (!_stop)
                {
                    sw.Restart();

                    compressor.Compress(sourceImage, TJSubsamplingOptions.TJSAMP_GRAY, Quality, TJFlags.BOTTOMUP);

                    sw.Stop();
                    var sleepValue = 1000 / 25.0 - sw.ElapsedMilliseconds;
                    if (sleepValue < 0)
                        sleepValue = 0;
                    average += sw.ElapsedMilliseconds;
                    iterations++;
                    Thread.Sleep((int)sleepValue);
                }

                Console.WriteLine("Average compression time for image {0}x{1} is {2:f3} ms. Iterations count {3}", width, height, (double)average / iterations, iterations);
            }
            finally
            {
                compressor.Dispose();
                sourceImage.Dispose();
                Wait.Set();
            }
            
        }

        private static void SystemDrawingCompression()
        {
            var sw = new Stopwatch();
            var sourceImage = (Bitmap)Image.FromFile(@"D:\Downloads\1.jpg");
            var width = sourceImage.Width;
            var height = sourceImage.Height;
            long average = 0;
            var iterations = 0;
            Console.WriteLine("Using System.Drawing");
            while (!_stop)
            {
                sw.Restart();
                using (var memoryStream = new MemoryStream())
                {
                    var encoder1 = GetEncoder(ImageFormat.Jpeg);
                    var encoder2 = Encoder.Quality;
                    var encoderParams = new EncoderParameters(1);
                    var encoderParameter = new EncoderParameter(encoder2, (long)Quality);
                    encoderParams.Param[0] = encoderParameter;
                    sourceImage.Save(memoryStream, encoder1, encoderParams);
                }
                sw.Stop();
                var sleepValue = 1000 / 25.0 - sw.ElapsedMilliseconds;
                if (sleepValue < 0)
                    sleepValue = 0;

                average += sw.ElapsedMilliseconds;
                iterations++;

                Thread.Sleep((int)sleepValue);
            }
            Console.WriteLine("Average compression time for image {0}x{1} is {2:f3} ms. Iterations count {3}", width, height, (double)average / iterations, iterations);
            sourceImage.Dispose();
            Wait.Set();
        }
        

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo imageCodecInfo = ImageCodecInfo.GetImageDecoders().FirstOrDefault(c => c.FormatID == format.Guid);
            if (imageCodecInfo == null)
                throw new InvalidOperationException("Unsupported ImageFormat " + format);
            return imageCodecInfo;
        }
    }
}
