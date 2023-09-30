using Microsoft.AspNetCore.Components;
using OpenCvSharp;
using SpawnDev.BlazorJS.JSObjects;
using SpawnDev.BlazorJS.OpenCVSharp4.Services;
using SpawnDev.BlazorJS.Toolbox;
using Timer = System.Timers.Timer;

namespace SpawnDev.BlazorJS.OpenCVSharp4.Demo.Pages
{
    public partial class VideoCaptureDemo : IDisposable
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
        CascadeClassifier? face_cascade = null;
        CascadeClassifier? eyes_cascade = null;
        // Video source
        // https://github.com/intel-iot-devkit/sample-videos
        string TestVideo = "test-videos/face-demographics-walking-and-pause.mp4";
        bool beenInit = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (!beenInit)
            {
                beenInit = true;
                face_cascade = await OpenCVService.LoadCascadeClassifier("haarcascades/haarcascade_frontalface_default.xml");
                eyes_cascade = await OpenCVService.LoadCascadeClassifier("haarcascades/haarcascade_eye.xml");
                canvasSrcEl = new HTMLCanvasElement(canvasSrcRef);
                canvasSrcCtx = canvasSrcEl.Get2DContext();
                videoCapture = new VideoCapture();
                videoCapture.Video.CrossOrigin = "anonymous";   // allows videos from others domains using cors
                timer.Elapsed += Timer_Elapsed;
                timer.Interval = 1000d / 60d;
                timer.Enabled = true;
            }
        }

        // https://github.com/opencv/opencv/tree/master/rgbaBytes/haarcascades
        // https://github.com/VahidN/OpenCVSharp-Samples/blob/master/OpenCVSharpSample15/Program.cs
        // https://www.tech-quantum.com/have-fun-with-webcam-and-opencv-in-csharp-part-2/
        public List<FaceFeature> FaceDetect(Mat image)
        {
            var features = new List<FaceFeature>();
            var faces = DetectFaces(image);
            foreach (var item in faces)
            {
                //Get the region of interest where you can find facial features
                using Mat face_roi = image[item];
                //Detect eyes
                Rect[] eyes = DetectEyes(face_roi);
                //Record the facial features in a list
                features.Add(new FaceFeature()
                {
                    Face = item,
                    Eyes = eyes
                });
            }
            return features;
        }

        public void MarkFeatures(Mat image, List<FaceFeature> features)
        {
            foreach (FaceFeature feature in features)
            {
                Cv2.Rectangle(image, feature.Face, new Scalar(0, 255, 0), thickness: 1);
                using var face_region = image[feature.Face];
                foreach (var eye in feature.Eyes)
                {
                    Cv2.Rectangle(face_region, eye, new Scalar(255, 0, 0), thickness: 1);
                }
            }
        }

        private Rect[] DetectEyes(Mat image)
        {
            Rect[] faces = eyes_cascade == null ? new Rect[0] : eyes_cascade.DetectMultiScale(image, 1.3, 5);
            return faces;
        }

        private Rect[] DetectFaces(Mat image)
        {
            Rect[] faces = face_cascade == null ? new Rect[0] : face_cascade.DetectMultiScale(image, 1.3, 5);
            return faces;
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
            var res = FaceDetect(src);
            MarkFeatures(src, res);
            src.DrawOnCanvas(canvasSrcCtx, true);
        }

        public void Dispose()
        {
            // Dispose OpenCV resources
            if (beenInit)
            {
                beenInit = false;
                face_cascade?.Dispose();
                eyes_cascade?.Dispose();
                videoCapture?.Dispose();
                canvasSrcEl?.Dispose();
                canvasSrcCtx?.Dispose();
            }
        }
    }
}
