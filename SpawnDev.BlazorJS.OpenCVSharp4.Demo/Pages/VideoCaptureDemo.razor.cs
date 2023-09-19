using Microsoft.AspNetCore.Components;
using OpenCvSharp;
using SpawnDev.BlazorJS.JSObjects;
using SpawnDev.BlazorJS.OpenCVSharp4.Services;
using SpawnDev.BlazorJS.Toolbox;
using Timer = System.Timers.Timer;

namespace SpawnDev.BlazorJS.OpenCVSharp4.Demo.Pages
{
    public partial class VideoCaptureDemo
    {

        [Inject]
        MediaDevicesService MediaDevicesService { get; set; }

        [Inject]
        OpenCVService OpenCVService { get; set; }

        ElementReference canvasSrcRef;
        Timer timer = new Timer();
        VideoCapture? videoCapture;
        HTMLCanvasElement? canvasSrcEl;
        CanvasRenderingContext2D? canvasSrcCtx;
        MediaStream? mediaStream = null;
        Mat? src;
        // Video source
        // https://github.com/intel-iot-devkit/sample-videos
        string TestVideo = "test-videos/face-demographics-walking-and-pause.mp4";

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                canvasSrcEl = new HTMLCanvasElement(canvasSrcRef);
                canvasSrcCtx = canvasSrcEl.Get2DContext();
                videoCapture = new VideoCapture();
                videoCapture.Video.CrossOrigin = "anonymous";   // allows videos from others domains using cors
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
            var res = OpenCVService.FaceDetect(src);
            OpenCVService.MarkFeatures(src, res);
            src.DrawOnCanvas(canvasSrcCtx, true);
        }
    }
}
