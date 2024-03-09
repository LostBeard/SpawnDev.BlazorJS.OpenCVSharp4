using OpenCvSharp;
using SpawnDev.BlazorJS.JSObjects;

namespace SpawnDev.BlazorJS.OpenCVSharp4
{
    public class VideoCapture : IDisposable
    {
        public HTMLVideoElement Video { get; private set; }
        HTMLCanvasElement _cameraCanvasEl;
        CanvasRenderingContext2D _cameraCanvasElCtx;
        public Size SourceVideoFrameSize { get; private set; } = new Size(0, 0);
        public event Action OnInputFrameResized;

        public VideoCapture(HTMLVideoElement source)
        {
            Video = source;
            _cameraCanvasEl = new HTMLCanvasElement();
            _cameraCanvasElCtx = _cameraCanvasEl.Get2DContext(new ContextAttributes2D { WillReadFrequently = true });
        }

        public VideoCapture()
        {
            Video = new HTMLVideoElement();
            Video.AutoPlay = true;
            _cameraCanvasEl = new HTMLCanvasElement();
            _cameraCanvasElCtx = _cameraCanvasEl.Get2DContext(new ContextAttributes2D { WillReadFrequently = true });
        }

        private void VideoSizeChangedCheck()
        {
            var w = Video.VideoWidth;
            var h = Video.VideoHeight;
            var videoFrameSize = w == 0 || h == 0 ? new Size() : new Size(w, h);
            if (SourceVideoFrameSize != videoFrameSize)
            {
                _cameraCanvasEl.Width = w;
                _cameraCanvasEl.Height = h;
                var cw = _cameraCanvasEl.Width;
                var ch = _cameraCanvasEl.Height;
                if (cw == w && ch == h)
                {
                    SourceVideoFrameSize = videoFrameSize;
                    OnInputFrameResized?.Invoke();
                }
            }
        }

        public ImageData? ReadImageData(out Size frameSize)
        {
            ImageData? ret = null;
            VideoSizeChangedCheck();
            frameSize = new Size(SourceVideoFrameSize.Width, SourceVideoFrameSize.Height);
            if (SourceVideoFrameSize.Width > 0)
            {
                _cameraCanvasElCtx.DrawImage(Video);
                ret = _cameraCanvasElCtx.GetImageData(0, 0, SourceVideoFrameSize.Width, SourceVideoFrameSize.Height);
            }
            return ret;
        }

        public ImageData? ReadImageData()
        {
            ImageData? ret = null;
            VideoSizeChangedCheck();
            if (SourceVideoFrameSize.Width > 0)
            {
                _cameraCanvasElCtx.DrawImage(Video);
                ret = _cameraCanvasElCtx.GetImageData(0, 0, SourceVideoFrameSize.Width, SourceVideoFrameSize.Height);
            }
            return ret;
        }

        public ArrayBuffer? ReadArrayBuffer(out Size frameSize)
        {
            ArrayBuffer? ret = null;
            VideoSizeChangedCheck();
            frameSize = new Size(SourceVideoFrameSize.Width, SourceVideoFrameSize.Height);
            if (SourceVideoFrameSize.Width > 0)
            {
                _cameraCanvasElCtx.DrawImage(Video);
                using var srcRGBA = _cameraCanvasElCtx.GetImageData(0, 0, SourceVideoFrameSize.Width, SourceVideoFrameSize.Height);
                using var data = srcRGBA.Data;
                ret = data.Buffer;
            }
            return ret;
        }

        public Mat? Read()
        {
            var ret = new Mat();
            if (Read(ret)) return ret;
            ret.Dispose();
            return null;
        }

        public bool Read(Mat image)
        {
            var ret = false;
            VideoSizeChangedCheck();
            if (SourceVideoFrameSize.Width > 0)
            {
                _cameraCanvasElCtx.DrawImage(Video);
                var srcRGBA = _cameraCanvasElCtx.GetImageBytes();
                if (srcRGBA != null)
                {
                    if (image.Type() != MatType.CV_8UC4 || image.Size() != SourceVideoFrameSize)
                    {
                        image.Create(SourceVideoFrameSize, MatType.CV_8UC4);
                    }
                    image.SetRGBABytes(srcRGBA);
                    ret = true;
                }
            }
            return ret;
        }

        public void Dispose()
        {
            _cameraCanvasElCtx.Dispose();
            _cameraCanvasEl.Dispose();
            Video.Dispose();
        }
    }
}
