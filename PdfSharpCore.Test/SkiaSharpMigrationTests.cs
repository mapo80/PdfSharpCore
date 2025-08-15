using System;
using System.IO;
using FluentAssertions;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using PdfSharpCore.Test.Helpers;
using PdfSharpCore.Utils;
using SkiaSharp;
using Xunit;

namespace PdfSharpCore.Test
{
    public class SkiaSharpMigrationTests
    {
        public SkiaSharpMigrationTests()
        {
            ImageSource.ImageSourceImpl = new SkiaSharpImageSource();
        }

        [Fact]
        public void FromBitmapReturnsCorrectSize()
        {
            using var bmp = new SKBitmap(new SKImageInfo(10, 20, SKColorType.Bgra8888, SKAlphaType.Premul));
            var src = SkiaSharpImageSource.FromBitmap(bmp);
            try
            {
                src.Width.Should().Be(10);
                src.Height.Should().Be(20);
                src.Transparent.Should().BeTrue();
            }
            finally
            {
                (src as IDisposable)?.Dispose();
            }
        }

        [Fact]
        public void FromFileLoadsImage()
        {
            var path = PathHelper.GetInstance().GetAssetPath("lenna.png");
            var img = ImageSource.FromFile(path);
            try
            {
                img.Width.Should().BeGreaterThan(0);
                img.Height.Should().BeGreaterThan(0);
            }
            finally
            {
                (img as IDisposable)?.Dispose();
            }
        }

        [Fact]
        public void FromStreamLoadsImage()
        {
            var path = PathHelper.GetInstance().GetAssetPath("lenna.png");
            var src = ImageSource.FromStream("lenna", () => File.OpenRead(path));
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
        public void SaveAsJpegProducesReadableImage()
        {
            using var bmp = new SKBitmap(30, 30, true);
            var src = SkiaSharpImageSource.FromBitmap(bmp);
            try
            {
                using var ms = new MemoryStream();
                src.SaveAsJpeg(ms);
                ms.Length.Should().BeGreaterThan(0);
                ms.Position = 0;
                using var decoded = SKBitmap.Decode(ms);
                decoded.Width.Should().Be(30);
                decoded.Height.Should().Be(30);
            }
            finally
            {
                (src as IDisposable)?.Dispose();
            }
        }

        [Fact]
        public void SaveAsPdfBitmapContainsBmpHeader()
        {
            using var bmp = new SKBitmap(5, 5, true);
            var src = SkiaSharpImageSource.FromBitmap(bmp);
            try
            {
                using var ms = new MemoryStream();
                src.SaveAsPdfBitmap(ms);
                ms.Length.Should().BeGreaterThan(0);
                var bytes = ms.ToArray();
                bytes[0].Should().Be((byte)'B');
                bytes[1].Should().Be((byte)'M');
            }
            finally
            {
                (src as IDisposable)?.Dispose();
            }
        }

        [Fact]
        public void TransparentFlagReflectsAlpha()
        {
            using var opaque = new SKBitmap(new SKImageInfo(1,1, SKColorType.Bgra8888, SKAlphaType.Opaque));
            var opSrc = SkiaSharpImageSource.FromBitmap(opaque);
            try
            {
                opSrc.Transparent.Should().BeFalse();
            }
            finally
            {
                (opSrc as IDisposable)?.Dispose();
            }

            using var alpha = new SKBitmap(new SKImageInfo(1,1, SKColorType.Bgra8888, SKAlphaType.Premul));
            var alphaSrc = SkiaSharpImageSource.FromBitmap(alpha);
            try
            {
                alphaSrc.Transparent.Should().BeTrue();
            }
            finally
            {
                (alphaSrc as IDisposable)?.Dispose();
            }
        }
    }
}
