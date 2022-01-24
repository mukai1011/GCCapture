﻿using System.Drawing;
using OpenCvSharp;

public class GCCapture : IDisposable
{
    private VideoCapture _videoCapture;
    private bool _showImage;
    private Thread _threadUpdateFrame;
    private bool _updating = true;
    private Mat _frame = new Mat();
    
    private bool _disposed = false;

    public GCCapture(int index, System.Drawing.Size size) : this(index, size, true) {}
    public GCCapture(int index, System.Drawing.Size size, bool showImage)
    {
        this._showImage = showImage; 

        try
        {
            this._videoCapture = new VideoCapture(index);
            if (!_videoCapture.IsOpened())
            {
                _videoCapture.Release();
                throw new Exception("Cannot allocate the VideoCapture specified.");
            }

            _videoCapture.FrameWidth = size.Width;
            _videoCapture.FrameHeight = size.Height;
        }
        catch
        {
            throw new Exception("Cannot allocate the VideoCapture specified.");
        }

        this._threadUpdateFrame = new Thread(new ThreadStart(this.UpdateFrame));
        _threadUpdateFrame.Start();
    }

    /// <summary>
    /// キャプチャデバイスから画像を取得します。
    /// </summary>
    /// <returns>Matオブジェクト</returns>
    protected Mat GetImage()
    {
        lock (_frame)
        {
            return _frame;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _updating = false;
                _threadUpdateFrame.Join();
                _videoCapture.Dispose();
            }
            _disposed = true;
        }
    }

    private void UpdateFrame()
    {
        Mat resized = new Mat();
        Window? window = _showImage ? new Window("GCCapture") : null;

        while (_updating)
        {
            lock (_frame)
            {
                if (!_videoCapture.Read(_frame))
                {
                    throw new Exception("Cannot grab frame from the VideoCapture specified.");
                }
                if (window != null)
                {
                    Cv2.Resize(_frame, resized, new OpenCvSharp.Size(640, 480));
                    window.ShowImage(resized);
                    Cv2.WaitKey(1);
                }
            }
        }
        if (window != null) window.Dispose();
    }
}