using System;
using System.IO;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace PdfSharpCore.Utils
{
    public class SkiaSharpImageSource : ImageSource
    {
        public static IImageSource FromBitmap(SKBitmap bitmap, int? quality = 75)
        {
            var name = "*" + Guid.NewGuid().ToString("B");
            return new SkiaSharpImageSourceImpl(name, bitmap, quality ?? 75);
        }

        protected override IImageSource FromBinaryImpl(string name, Func<byte[]> imageSource, int? quality = 75)
        {
            var data = SKData.CreateCopy(imageSource());
            var bitmap = SKBitmap.Decode(data);
            data.Dispose();
            return new SkiaSharpImageSourceImpl(name, bitmap, quality ?? 75);
        }

        protected override IImageSource FromFileImpl(string path, int? quality = 75)
        {
            var bitmap = SKBitmap.Decode(path);
            return new SkiaSharpImageSourceImpl(path, bitmap, quality ?? 75);
        }

        protected override IImageSource FromStreamImpl(string name, Func<Stream> imageStream, int? quality = 75)
        {
            using (var stream = imageStream())
            using (var skStream = new SKManagedStream(stream))
            {
                var bitmap = SKBitmap.Decode(skStream);
                return new SkiaSharpImageSourceImpl(name, bitmap, quality ?? 75);
            }
        }

        private class SkiaSharpImageSourceImpl : IImageSource, IDisposable
        {
            private readonly SKBitmap _bitmap;
            private readonly int _quality;

            public SkiaSharpImageSourceImpl(string name, SKBitmap bitmap, int quality)
            {
                Name = name;
                _bitmap = bitmap;
                _quality = quality;
            }

            public int Width => _bitmap.Width;
            public int Height => _bitmap.Height;
            public string Name { get; }
            public bool Transparent => _bitmap.AlphaType != SKAlphaType.Opaque;

            public void SaveAsJpeg(MemoryStream ms)
            {
                using (var image = SKImage.FromBitmap(_bitmap))
                using (var data = image.Encode(SKEncodedImageFormat.Jpeg, _quality))
                {
                    data.SaveTo(ms);
                }
            }

            public void SaveAsPdfBitmap(MemoryStream ms)
            {
                int width = _bitmap.Width;
                int height = _bitmap.Height;
                int bytesPerPixel = 4;
                int stride = width * bytesPerPixel;
                int fileSize = 54 + stride * height;
                byte[] header = new byte[54];
                header[0] = 0x42; header[1] = 0x4D;
                BitConverter.GetBytes(fileSize).CopyTo(header, 2);
                header[10] = 54;
                header[14] = 40;
                BitConverter.GetBytes(width).CopyTo(header, 18);
                BitConverter.GetBytes(height).CopyTo(header, 22);
                header[26] = 1; header[28] = 32;
                ms.Write(header, 0, header.Length);

                int length = stride * height;
                byte[] pixels = new byte[length];
                Marshal.Copy(_bitmap.GetPixels(), pixels, 0, length);
                for (int y = height - 1; y >= 0; y--)
                {
                    ms.Write(pixels, y * stride, stride);
                }
            }

            public void Dispose()
            {
                _bitmap.Dispose();
            }
        }
    }
}
