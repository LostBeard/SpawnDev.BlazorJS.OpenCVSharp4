using OpenCvSharp;
using SpawnDev.BlazorJS.JSObjects;
using SpawnDev.BlazorJS.WebWorkers;
using System.Runtime.InteropServices;

namespace SpawnDev.BlazorJS.OpenCVSharp4.Services
{

    public class OpenCVService : IAsyncBackgroundService, IDisposable
    {
        WebWorkerService _webWorkerService;
        HttpClient _httpClient;
        CascadeClassifier? face_cascade = null;
        CascadeClassifier? eyes_cascade = null;

        public OpenCVService(WebWorkerService webWorkerService, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _webWorkerService = webWorkerService;
        }

        public async Task InitAsync()
        {
            face_cascade = await LoadCascadeClassifier("haarcascades/haarcascade_frontalface_default.xml");
            eyes_cascade = await LoadCascadeClassifier("haarcascades/haarcascade_eye.xml");
        }

        //function getImageRGBABytes(url)
        //{
        //    return new Promise((resolve, reject) =>
        //    {
        //        var img = new Image();
        //        img.onload = () =>
        //        {
        //            var canvas = document.createElement('canvas');
        //            var context = canvas.getContext('2d');
        //            context.drawImage(img, 0, 0, img.width, img.height);
        //            resolve(context.getImageData(0, 0, img.width, img.height).data);
        //        };
        //        img.onerror = (e) => reject(e);
        //        img.src = url;
        //    });
        //}

        //Task<byte[]> GetImageRGBABytes(string url)
        //{
        //    var tcs = new TaskCompletionSource<byte[]>();
        //    var image = new HTMLImageElement();
        //    var imageCallbacks = new CallbackGroup();
        //    image.AddEventListener("load", Callback.Create(() =>
        //    {
        //        imageCallbacks.Dispose();
        //        using var canvas = new HTMLCanvasElement();
        //        using var context = canvas.Get2DContext();
        //        context.DrawImage(image, 0, 0);
        //        using var imageData = context.GetImageData(0, 0, image.Width, image.Height);
        //        using var uint8ClampedArray = imageData.Data;
        //        using var arrayBuffer = uint8ClampedArray.Buffer;
        //        var data = arrayBuffer.ReadBytes();
        //        image.Dispose();
        //        tcs.TrySetResult(data);
        //    }, imageCallbacks));
        //    image.AddEventListener("error", Callback.Create(() =>
        //    {
        //        imageCallbacks.Dispose();
        //        image.Dispose();
        //        tcs.TrySetException(new Exception("Load failed"));
        //    }, imageCallbacks));
        //    image.Src = url;
        //    return tcs.Task;
        //}

        //Task<byte[]> GetImageRGBABytes1(string url)
        //{
        //    var tcs = new TaskCompletionSource<byte[]>();
        //    var image = new HTMLImageElement();
        //    var imageCallbacks = new CallbackGroup();
        //    image.AddEventListener("load", Callback.Create(() =>
        //    {
        //        imageCallbacks.Dispose();
        //        using var canvas = new HTMLCanvasElement();
        //        using var context = canvas.Get2DContext();
        //        context.DrawImage(image, 0, 0);
        //        using var imageData = context.GetImageData(0, 0, image.Width, image.Height);
        //        using var uint8ClampedArray = imageData.Data;
        //        using var arrayBuffer = uint8ClampedArray.Buffer;
        //        var data = arrayBuffer.ReadBytes();
        //        image.Dispose();
        //        tcs.TrySetResult(data);
        //    }, imageCallbacks));
        //    image.AddEventListener("error", Callback.Create(() =>
        //    {
        //        imageCallbacks.Dispose();
        //        image.Dispose();
        //        tcs.TrySetException(new Exception("Load failed"));
        //    }, imageCallbacks));
        //    image.Src = url;
        //    return tcs.Task;
        //}

        //Task<Mat> GetImageAsMat(string url)
        //{
        //    var tcs = new TaskCompletionSource<Mat>();
        //    var image = new HTMLImageElement();
        //    var imageCallbacks = new CallbackGroup();
        //    image.AddEventListener("load", Callback.Create(() =>
        //    {
        //        imageCallbacks.Dispose();
        //        using var canvas = new HTMLCanvasElement();
        //        using var context = canvas.Get2DContext();
        //        canvas.Width = image.Width;
        //        canvas.Height = image.Height;
        //        context.DrawImage(image, 0, 0);
        //        using var imageData = context.GetImageData(0, 0, image.Width, image.Height);
        //        using var uint8ClampedArray = imageData.Data;
        //        using var arrayBuffer = uint8ClampedArray.Buffer;
        //        var rgbaBytes = arrayBuffer.ReadBytes();
        //        var mat = new Mat(new Size(image.Width, image.Height), MatType.CV_8UC4);
        //        Marshal.Copy(rgbaBytes, 0, mat.DataStart, rgbaBytes.Length);
        //        tcs.TrySetResult(mat);
        //        image.Dispose();
        //    }, imageCallbacks));
        //    image.AddEventListener("error", Callback.Create(() =>
        //    {
        //        imageCallbacks.Dispose();
        //        image.Dispose();
        //        tcs.TrySetException(new Exception("Load failed"));
        //    }, imageCallbacks));
        //    image.Src = url;
        //    return tcs.Task;
        //}

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

        async Task<CascadeClassifier> LoadCascadeClassifier(string url)
        {
            var text = await _httpClient.GetStringAsync(url);
            System.IO.File.WriteAllText("tmp.xml", text);
            var cascadeClassifier = new CascadeClassifier("tmp.xml");
            return cascadeClassifier;
        }

        //public async Task WhenReady()
        //{
        //    if (_workerPool.AreWorkersRunning) await _workerPool.WhenWorkerReady();
        //    else while (ProcessFrameRunningLocal) await Task.Delay(5);
        //}
        //public bool IsReady => _workerPool.AreWorkersRunning ? _workerPool.IsReady : ProcessFrameRunningLocal;
        //public int WorkerCount => _workerPool.WorkersRunning;
        //public Task<bool> SetWorkerCount(int count) => _workerPool.SetWorkerCount(count);

        // incoming parameters and return values are checked to see if they are transferable types and are automatically transferred added to the transferable postMessage list


        ////bool ProcessFrameRunningLocal = false;
        ////[return: WorkerTransfer(true)]
        ////public async Task<ProcessFrameResult?> FaceDetection([WorkerTransfer(true)] ArrayBuffer? frameBuffer, int width, int height)
        ////{
        ////    try
        ////    {
        ////        if (_workerPool.AreWorkersRunning)
        ////        {
        ////            var worker = await _workerPool.GetFreeWorkerAsync();
        ////            if (worker != null)
        ////            {
        ////                return await worker.InvokeAsync<OpenCVService, ProcessFrameResult?>(nameof(OpenCVService.FaceDetection), frameBuffer, width, height);
        ////            }
        ////        }
        ////        else
        ////        {
        ////            ProcessFrameRunningLocal = true;
        ////            var ret = await FaceDetectionInternal(frameBuffer, width, height);
        ////            ProcessFrameRunningLocal = false;
        ////            return ret;
        ////        }
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        Console.WriteLine("ProcessFrame: " + ex.ToString());
        ////    }
        ////    return null;
        ////}


        //public async Task<CascadeClassifier?> GetCascadeClassifer(string url)
        //{
        //    CascadeClassifier? ret = null;
        //    if (_cascadeClassifiers.TryGetValue(url, out CascadeClassifierCache tmp)) return tmp.CascadeClassifier;
        //    try
        //    {
        //        var xml = await _httpClient.GetStringAsync(url);
        //        if (!string.IsNullOrEmpty(xml))
        //        {
        //            var tmp1 = new CascadeClassifierCache(xml);
        //            _cascadeClassifiers[url] = tmp1;
        //            ret = tmp1.CascadeClassifier;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //    return ret;
        //}

        //private async Task<ProcessFrameResult?> FaceDetectionInternal(ArrayBuffer? frameBuffer, int width, int height)
        //{
        //    ProcessFrameResult ret = new ProcessFrameResult();
        //    if (frameBuffer == null) return null;
        //    using var srcImage = new Mat(new Size(width, height), MatType.CV_8UC4);
        //    try
        //    {
        //        srcImage.SetRGBAArrayBuffer(frameBuffer);
        //    }
        //    catch
        //    {
        //        return ret;
        //    }
        //    using var grayImage = srcImage.CvtColor(ColorConversionCodes.RGBA2GRAY);
        //    Rect[] faces = face_cascade.DetectMultiScale(grayImage, 1.3, 5);
        //    var red = new Scalar(255, 0, 0, 255);
        //    var blue = new Scalar(0, 0, 255, 255);
        //    foreach (var faceRect in faces)
        //    {
        //        // draw rect aroun dthe face
        //        Cv2.Rectangle(srcImage, faceRect, red, 3);
        //        // find eyes in the face
        //        using var detectedFaceGray = new Mat(grayImage, faceRect);
        //        var nestedObjects = eyes_cascade.DetectMultiScale(
        //            detectedFaceGray,
        //            scaleFactor: 1.1,
        //            minNeighbors: 2,
        //            flags: HaarDetectionTypes.DoRoughSearch | HaarDetectionTypes.ScaleImage,
        //            minSize: new Size(30, 30)
        //            );

        //        foreach (var nestedObject in nestedObjects)
        //        {
        //            var center = new Point
        //            {
        //                X = (int)(Math.Round(nestedObject.X + nestedObject.Width * 0.5, MidpointRounding.ToEven) + faceRect.Left),
        //                Y = (int)(Math.Round(nestedObject.Y + nestedObject.Height * 0.5, MidpointRounding.ToEven) + faceRect.Top)
        //            };
        //            var radius = Math.Round((nestedObject.Width + nestedObject.Height) * 0.25, MidpointRounding.ToEven);
        //            Cv2.Circle(srcImage, center, (int)radius, blue, thickness: 3);
        //        }
        //    }
        //    ret.FacesFound = faces.Count();
        //    ret.ArrayBuffer = srcImage.GetRGBAArrayBuffer();
        //    return ret;
        //}

        public bool IsDisposed { get; private set; }
        public void Dispose()
        {
            if (IsDisposed) return;
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            IsDisposed = true;
            if (disposing)
            {
                face_cascade?.Dispose();
                eyes_cascade?.Dispose();
            }
        }
        ~OpenCVService()
        {
            Dispose(false);
        }
    }
}
