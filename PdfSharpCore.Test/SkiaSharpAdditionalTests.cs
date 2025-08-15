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
    public class SkiaSharpAdditionalTests
    {
        public SkiaSharpAdditionalTests()
        {
            ImageSource.ImageSourceImpl = new SkiaSharpImageSource();
        }

        [Fact]
        public void FromBinaryLoadsImage()
        {
            var path = PathHelper.GetInstance().GetAssetPath("lenna.png");
            var bytes = File.ReadAllBytes(path);
            var src = ImageSource.FromBinary("lenna", () => bytes);
            try
            {
                src.Width.Should().BeGreaterThan(0);
                src.Height.Should().BeGreaterThan(0);
            }
            finally
            {
                (src as IDisposable)?.Dispose();
            }
        }

        [Fact]
        public void SaveAsJpegQualityAffectsSize()
        {
            using var bmp = new SKBitmap(new SKImageInfo(50, 50));
            using (var canvas = new SKCanvas(bmp))
            {
                canvas.DrawColor(SKColors.Orange);
                canvas.Flush();
            }

            var low = SkiaSharpImageSource.FromBitmap(bmp.Copy(), 10);
            var high = SkiaSharpImageSource.FromBitmap(bmp.Copy(), 90);
            try
            {
                using var lowMs = new MemoryStream();
                using var highMs = new MemoryStream();
                low.SaveAsJpeg(lowMs);
                high.SaveAsJpeg(highMs);
                highMs.Length.Should().BeGreaterThan(lowMs.Length);
            }
            finally
            {
                (low as IDisposable)?.Dispose();
                (high as IDisposable)?.Dispose();
            }
        }

        [Fact]
        public void XImageFromImageSourceHasPixelDimensions()
        {
            using var bmp = new SKBitmap(new SKImageInfo(40, 50, SKColorType.Bgra8888, SKAlphaType.Premul));
            var src = SkiaSharpImageSource.FromBitmap(bmp.Copy());
            try
            {
                using var img = XImage.FromImageSource(src);
                img.PixelWidth.Should().Be(40);
                img.PixelHeight.Should().Be(50);
            }
            finally
            {
                (src as IDisposable)?.Dispose();
            }
        }

        [Fact]
        public void FromFileInitializesSkiaSharpImageSource()
        {
            var previous = ImageSource.ImageSourceImpl;
            try
            {
                ImageSource.ImageSourceImpl = null;
                var path = PathHelper.GetInstance().GetAssetPath("lenna.png");
                using var img = XImage.FromFile(path);
                ImageSource.ImageSourceImpl.Should().BeOfType<SkiaSharpImageSource>();
                img.PixelWidth.Should().BeGreaterThan(0);
            }
            finally
            {
                ImageSource.ImageSourceImpl = previous ?? new SkiaSharpImageSource();
            }
        }
    }
}
