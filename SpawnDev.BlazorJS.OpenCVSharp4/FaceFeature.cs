using OpenCvSharp;

namespace SpawnDev.BlazorJS.OpenCVSharp4.Services
{
    //public class ProcessFrameResult : IDisposable
    //{
    //    // below is unnecessary as by default transferable objects are transferred instead of copied (same for return values, and parameters)
    //    [WorkerTransfer(true)]
    //    public ArrayBuffer? ArrayBuffer { get; set; }
    //    public int FacesFound { get; set; }
    //    public void Dispose()
    //    {
    //        ArrayBuffer?.Dispose();
    //    }
    //}
    public class FaceFeature
    {
        public Rect Face { get; set; }
        public Rect[] Eyes { get; set; } = new Rect[0];
    }
}
