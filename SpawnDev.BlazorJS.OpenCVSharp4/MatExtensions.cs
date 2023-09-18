using OpenCvSharp;
using SpawnDev.BlazorJS.JSObjects;
using System.Runtime.InteropServices;

namespace SpawnDev.BlazorJS.OpenCVSharp4
{
    public static class MatExtensions
    {
        public static void SetRGBAArrayBuffer(this Mat src, ArrayBuffer rgbaArrayBuffer)
        {
            var bytes = rgbaArrayBuffer.ReadBytes();
            src.SetRGBABytes(bytes);
        }
        public static void SetRGBABytes(this Mat src, byte[] rgbaBytes)
        {
            var length = (int)(src.Step() * src.Height);
            if (length != rgbaBytes.Length) throw new Exception("WriteMatRGBABytes: Invalid data size or step size mismatch");
            Marshal.Copy(rgbaBytes, 0, src.DataStart, length);
        }

        public static void SetBytes(this Mat mat, byte[] bytes)
        {
            if (!mat.IsContinuous())
            {
                throw new InvalidOperationException("Mat should be continuous.");
            }
            var length = (int)(mat.DataEnd.ToInt64() - mat.DataStart.ToInt64());
            if (length != bytes.Length) throw new InvalidOperationException("Mat size does not match bytes being set");
            Marshal.Copy(bytes, 0, mat.DataStart, length);
        }

        public static ArrayBuffer GetRGBAArrayBuffer(this Mat mat)
        {
            var bytes = mat.GetRGBABytes();
            using var uint8 = new Uint8Array(bytes);
            return uint8.Buffer;
        }

        public static void DrawOnCanvas(this Mat mat, CanvasRenderingContext2D ctx, bool autoResizeCanvas = false)
        {
            if (autoResizeCanvas)
            {
                try
                {
                    using var canvas = ctx.Canvas;
                    if (canvas.Width != mat.Width || canvas.Height != mat.Height)
                    {
                        canvas.Width = mat.Width;
                        canvas.Height = mat.Height;
                    }
                }
                catch (Exception ex)
                {
                    var tt = true;
                }
            }
            var bytes = mat.GetRGBABytes();
            if (bytes != null)
            {
                ctx.PutImageBytes(bytes, mat.Width, mat.Height);
            }
        }

        public static void DrawOnCanvas(this Mat mat, CanvasRenderingContext2D ctx, int dx, int dy)
        {
            var bytes = mat.GetRGBABytes();
            if (bytes != null)
            {
                ctx.PutImageBytes(bytes, mat.Width, mat.Height, dx, dy);
            }
        }

        public static void SetHomography(this Mat mat, byte[] homographyBytes)
        {
            mat.Create(new Size(3, 3), MatType.CV_64FC1);
            mat.SetBytes(homographyBytes);
        }

        public static byte[] GetBytes(this Mat mat)
        {
            if (!mat.IsContinuous())
            {
                throw new InvalidOperationException("Mat should be continuous.");
            }
            var length = (int)(mat.DataEnd.ToInt64() - mat.DataStart.ToInt64());
            var ret = new byte[length];
            Marshal.Copy(mat.DataStart, ret, 0, length);
            return ret;
        }

        public static byte[] GetRGBABytes(this Mat mat)
        {
            byte[] ret = new byte[0];
            Mat? rgba = null;
            try
            {
                var type = mat.Type();
                if (type == MatType.CV_8UC1)
                {
                    rgba = mat.CvtColor(ColorConversionCodes.GRAY2RGBA);
                }
                else if (type == MatType.CV_8UC3)
                {
                    rgba = mat.CvtColor(ColorConversionCodes.BGR2RGBA);
                }
                else if (type == MatType.CV_8UC4)
                {
                    // Nothing to do.
                    if (!mat.IsContinuous())
                    {
                        throw new InvalidOperationException("RGBA Mat should be continuous.");
                    }
                    var length = (int)(mat.DataEnd.ToInt64() - mat.DataStart.ToInt64());
                    ret = new byte[length];
                    Marshal.Copy(mat.DataStart, ret, 0, length);
                }
                else
                {
                    throw new ArgumentException($"Invalid mat type ({mat.Type()})");
                }
                if (rgba != null)
                {
                    if (!rgba.IsContinuous())
                    {
                        throw new InvalidOperationException("RGBA Mat should be continuous.");
                    }
                    var length = (int)(rgba.DataEnd.ToInt64() - rgba.DataStart.ToInt64());
                    ret = new byte[length];
                    Marshal.Copy(rgba.DataStart, ret, 0, length);
                }
            }
            finally
            {
                rgba?.Dispose();
            }
            return ret;
        }

        public static async Task LoadImageURL(this Mat mat, string url, string? crossOrigin = "anonymous")
        {
            using var image = await HTMLImageElement.CreateFromImageAsync(url, crossOrigin);
            using var canvas = new HTMLCanvasElement();
            using var context = canvas.Get2DContext();
            canvas.Width = image.Width;
            canvas.Height = image.Height;
            context.DrawImage(image, 0, 0);
            using var imageData = context.GetImageData(0, 0, image.Width, image.Height);
            using var uint8ClampedArray = imageData.Data;
            var rgbaBytes = uint8ClampedArray.ReadBytes();
            mat.Create(new Size(image.Width, image.Height), MatType.CV_8UC4);
            Marshal.Copy(rgbaBytes, 0, mat.DataStart, rgbaBytes.Length);
        }
    }
}
