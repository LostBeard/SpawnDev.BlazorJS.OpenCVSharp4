using OpenCvSharp;

namespace SpawnDev.BlazorJS.OpenCVSharp4.Services
{

    public class OpenCVService : IAsyncBackgroundService, IDisposable
    {
        HttpClient _httpClient;
        CascadeClassifier? face_cascade = null;
        CascadeClassifier? eyes_cascade = null;

        public OpenCVService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task InitAsync()
        {
            face_cascade = await LoadCascadeClassifier("haarcascades/haarcascade_frontalface_default.xml");
            eyes_cascade = await LoadCascadeClassifier("haarcascades/haarcascade_eye.xml");
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

        public async Task<CascadeClassifier> LoadCascadeClassifier(string url)
        {
            var text = await _httpClient.GetStringAsync(url);
            System.IO.File.WriteAllText("tmp.xml", text);
            var cascadeClassifier = new CascadeClassifier("tmp.xml");
            return cascadeClassifier;
        }

        public void Dispose()
        {
            face_cascade?.Dispose();
            eyes_cascade?.Dispose();
        }
    }
}
