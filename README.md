# SpawnDev.BlazorJS.OpenCVSharp4.Demo

[Live Demo](https://lostbeard.github.io/SpawnDev.BlazorJS.OpenCVSharp4/)

Demonstrates the use of the Nuget packages [OpenCVSharp4](https://www.nuget.org/packages/OpenCvSharp4) and [OpenCvSharp4.runtime.wasm](https://www.nuget.org/packages/OpenCvSharp4.runtime.wasm/) in Blazor WebAssembly. 

### Demos  
- Canny edge detection on an image.
- Haar cascade face and eye detection with video and webcam sources.

[OpenCVSharp4 repo]([OpenCVSharp4](https://github.com/shimat/opencvsharp))

# SpawnDev.BlazorJS.OpenCVSharp4

Includes tools for working with OpenCVSharp4 in Blazor WebAssembly including Mat extension methods, and a VideoCapture class for working with ```<video>``` elements.

In the below Canny edge detection example the Mat extension method LoadImageURL loads an image from a URL into the Mat. And the Mat extension method DrawOnCanvas draws a Mat onto a canvas 2D context.

```html
@using SpawnDev.BlazorJS.JSObjects
@using OpenCvSharp

<canvas style="zoom: 50%; border: 1px solid grey;" @ref=canvasSrcRef></canvas>
<canvas style="zoom: 50%; border: 1px solid grey;" @ref=canvasDestRef></canvas>
```
```cs
@code {
    ElementReference canvasSrcRef;
    ElementReference canvasDestRef;
    string TestImage = "https://i.imgur.com/WOZagma.jpeg";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            using var canvasSrcEl = new HTMLCanvasElement(canvasSrcRef);
            using var canvasSrcCtx = canvasSrcEl.Get2DContext();
            using var canvasDestEl = new HTMLCanvasElement(canvasDestRef);
            using var canvasDestCtx = canvasDestEl.Get2DContext();
            using var src = new Mat();
            await src.LoadImageURL(TestImage);
            src.DrawOnCanvas(canvasSrcCtx, true);
            using var dst = new Mat();
            Cv2.Canny(src, dst, 50, 200);
            dst.DrawOnCanvas(canvasDestCtx, true);
        }
    }
}
```
