using System;
using System.IO;
using FluentAssertions;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using PdfSharpCore.Drawing;
using PdfSharpCore.Test.Helpers;
using PdfSharpCore.Utils;
using SkiaSharp;
using Xunit;

namespace PdfSharpCore.Test
{
    public class SkiaSharpComprehensiveTests
    {
        public SkiaSharpComprehensiveTests()
        {
            ImageSource.ImageSourceImpl = new SkiaSharpImageSource();
        }

        [Fact]
        public void FromBitmapGeneratesNameWithAsterisk()
        {
            using var bmp = new SKBitmap(10, 10, true);
            var src = SkiaSharpImageSource.FromBitmap(bmp);
            try
            {
                src.Name.Should().StartWith("*");
            }
            finally
            {
                (src as IDisposable)?.Dispose();
            }
        }

        [Fact]
        public void SaveAsJpegOutputsJpegHeader()
        {
            using var bmp = new SKBitmap(12, 12, true);
            var src = SkiaSharpImageSource.FromBitmap(bmp);
            try
            {
                using var ms = new MemoryStream();
                src.SaveAsJpeg(ms);
                var bytes = ms.ToArray();
                bytes[0].Should().Be(0xFF);
                bytes[1].Should().Be(0xD8);
            }
            finally
            {
                (src as IDisposable)?.Dispose();
            }
        }

        [Fact]
        public void SaveAsPdfBitmapWritesCorrectDimensionsAndSize()
        {
            using var bmp = new SKBitmap(11, 7, true);
            var src = SkiaSharpImageSource.FromBitmap(bmp);
            try
            {
                using var ms = new MemoryStream();
                src.SaveAsPdfBitmap(ms);
                ms.Length.Should().Be(54 + 11 * 7 * 4);
                var bytes = ms.ToArray();
                BitConverter.ToInt32(bytes, 18).Should().Be(11);
                BitConverter.ToInt32(bytes, 22).Should().Be(7);
            }
            finally
            {
                (src as IDisposable)?.Dispose();
            }
        }

        [Fact]
        public void DisposeTwiceDoesNotThrow()
        {
            var bmp = new SKBitmap(5, 5, true);
            var src = SkiaSharpImageSource.FromBitmap(bmp);
            var disposable = (src as IDisposable)!;
            disposable.Dispose();
            Action act = () => disposable.Dispose();
            act.Should().NotThrow();
        }

        [Fact]
        public void XImageFromStreamInitializesSkiaSharpImageSource()
        {
            var previous = ImageSource.ImageSourceImpl;
            try
            {
                ImageSource.ImageSourceImpl = null;
                var path = PathHelper.GetInstance().GetAssetPath("lenna.png");
                using var img = XImage.FromStream(() => File.OpenRead(path));
                ImageSource.ImageSourceImpl.Should().BeOfType<SkiaSharpImageSource>();
                img.PixelWidth.Should().BeGreaterThan(0);
                img.PixelHeight.Should().BeGreaterThan(0);
            }
            finally
            {
                ImageSource.ImageSourceImpl = previous ?? new SkiaSharpImageSource();
            }
        }

        [Fact]
        public void FromBitmapDefaultQualityIsBetweenLowAndHigh()
        {
            using var bmp = new SKBitmap(new SKImageInfo(30, 30));
            using (var canvas = new SKCanvas(bmp))
            {
                canvas.DrawColor(SKColors.Blue);
                canvas.Flush();
            }

            var low = SkiaSharpImageSource.FromBitmap(bmp.Copy(), 10);
            var def = SkiaSharpImageSource.FromBitmap(bmp.Copy());
            var high = SkiaSharpImageSource.FromBitmap(bmp.Copy(), 90);
            try
            {
                using var lowMs = new MemoryStream();
                using var defMs = new MemoryStream();
                using var highMs = new MemoryStream();
                low.SaveAsJpeg(lowMs);
                def.SaveAsJpeg(defMs);
                high.SaveAsJpeg(highMs);
                lowMs.Length.Should().BeLessThan(defMs.Length);
                defMs.Length.Should().BeLessThan(highMs.Length);
            }
            finally
            {
                (low as IDisposable)?.Dispose();
                (def as IDisposable)?.Dispose();
                (high as IDisposable)?.Dispose();
            }
        }

        [Fact]
        public void FromBitmapGeneratedNamesAreUnique()
        {
            using var bmp1 = new SKBitmap(4, 4, true);
            using var bmp2 = new SKBitmap(4, 4, true);
            var src1 = SkiaSharpImageSource.FromBitmap(bmp1);
            var src2 = SkiaSharpImageSource.FromBitmap(bmp2);
            try
            {
                src1.Name.Should().NotBe(src2.Name);
            }
            finally
            {
                (src1 as IDisposable)?.Dispose();
                (src2 as IDisposable)?.Dispose();
            }
        }
    }
}
