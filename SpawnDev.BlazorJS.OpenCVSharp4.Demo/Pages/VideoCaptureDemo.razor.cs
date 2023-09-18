using SpawnDev.BlazorJS.JSObjects;
using OpenCvSharp;
using Timer = System.Timers.Timer;
using Microsoft.AspNetCore.Components;
using SpawnDev.BlazorJS.Toolbox;
using SpawnDev.BlazorJS.OpenCVSharp4.Services;

namespace SpawnDev.BlazorJS.OpenCVSharp4.Demo.Pages
{
    public partial class VideoCaptureDemo
    {

        [Inject]
        MediaDevicesService MediaDevicesService { get; set; }

        [Inject]
        OpenCVService OpenCVService { get; set; }

        ElementReference canvasSrcRef;
        ElementReference canvasDestRef;
        Timer timer = new Timer();
        VideoCapture? videoCapture;
        HTMLCanvasElement? canvasSrcEl;
        CanvasRenderingContext2D? canvasSrcCtx;
        HTMLCanvasElement? canvasDestEl;
        CanvasRenderingContext2D? canvasDestCtx;
        MediaStream? mediaStream = null;
        Mat? src;
        Mat? dest;
        string TestVideo = "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4";

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                canvasSrcEl = new HTMLCanvasElement(canvasSrcRef);
                canvasSrcCtx = canvasSrcEl.Get2DContext();
                canvasDestEl = new HTMLCanvasElement(canvasDestRef);
                canvasDestCtx = canvasDestEl.Get2DContext();
                videoCapture = new VideoCapture();
                videoCapture.Video.CrossOrigin = "anonymous";   // allows videos from others domains using cors ;
                timer.Elapsed += Timer_Elapsed;
                timer.Interval = 1000d / 60d;
                timer.Enabled = true;
            }
        }

        void StopPlaying()
        {
            if (videoCapture == null) return;
            videoCapture.Video.Src = null;
            videoCapture.Video.SrcObject = null;
            if (mediaStream != null)
            {
                mediaStream.Dispose();
                mediaStream = null;
            }
        }

        async Task PlayRemoteVideo()
        {
            try
            {
                StopPlaying();
                videoCapture.Video.Src = TestVideo;
            }
            catch { }
        }

        async Task PlayUserMedia()
        {
            try
            {
                StopPlaying();
                await MediaDevicesService.UpdateDeviceList(true);
                mediaStream = await MediaDevicesService.MediaDevices.GetUserMedia();
                videoCapture.Video.SrcObject = mediaStream;
            }
            catch { }
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (videoCapture == null) return;
            if (src == null) src = new Mat();
            var succ = videoCapture.Read(src);
            if (!succ) return;
            src.DrawOnCanvas(canvasSrcCtx, true);
            if (dest == null) dest = new Mat();
            Cv2.Canny(src, dest, 50, 200);
            dest.DrawOnCanvas(canvasDestCtx, true);


            OpenCVService.FaceDetect(src);
        }
    }
}
