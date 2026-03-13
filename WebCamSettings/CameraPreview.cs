using System;
using System.Drawing;
using System.Windows.Forms;
using DirectShowLib;
using System.Runtime.InteropServices;

namespace WebCamSettings
{
    public class CameraPreview : IDisposable
    {
        private IFilterGraph2 _filterGraph;
        private IMediaControl _mediaControl;
        private IVideoWindow _videoWindow;
        private IBaseFilter _cameraFilter;
        private IBasicVideo _basicVideo;
        private bool _isRunning = false;
        private Control _previewControl;
        private Size _videoSize = Size.Empty;

        public Size VideoSize => _videoSize;

        public void StartPreview(DsDevice camera, Control previewControl)
        {
            StopPreview();

            if (previewControl == null || previewControl.IsDisposed)
                throw new ArgumentException("Preview control is invalid");

            _previewControl = previewControl;

            try
            {
                // 1. Create a graph
                _filterGraph = (IFilterGraph2)new FilterGraph();

                // 2. Create Capture Graph Builder
                ICaptureGraphBuilder2 captureGraphBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
                int hr = captureGraphBuilder.SetFiltergraph(_filterGraph);
                DsError.ThrowExceptionForHR(hr);

                // 3. Get the camera filter
                Guid filterGuid = typeof(IBaseFilter).GUID;
                camera.Mon.BindToObject(null, null, ref filterGuid, out object filterObj);
                _cameraFilter = (IBaseFilter)filterObj;

                // 4. Add a filter to the graph
                hr = _filterGraph.AddFilter(_cameraFilter, "Video Camera");
                DsError.ThrowExceptionForHR(hr);

                // 5. Render the preview stream
                hr = captureGraphBuilder.RenderStream(
                    PinCategory.Preview,
                    MediaType.Video,
                    _cameraFilter,
                    null,
                    null);
                DsError.ThrowExceptionForHR(hr);

                // 6. Getting interfaces
                _mediaControl = (IMediaControl)_filterGraph;
                _videoWindow = (IVideoWindow)_filterGraph;
                _basicVideo = (IBasicVideo)_filterGraph;

                // 7. Get the video size
                if (_basicVideo != null)
                {
                    hr = _basicVideo.GetVideoSize(out int width, out int height);
                    if (hr == 0) // S_OK
                    {
                        _videoSize = new Size(width, height);
                        Console.WriteLine($"Video size: {width}x{height}");
                    }
                }

                // 8. Setting up the video window
                if (_videoWindow != null)
                {
                    // Make sure the control has a handle
                    if (!_previewControl.IsHandleCreated)
                    {
                        _previewControl.CreateControl();
                    }

                    // Set the parent window
                    hr = _videoWindow.put_Owner(_previewControl.Handle);
                    DsError.ThrowExceptionForHR(hr);

                    // Set the style as a child window
                    hr = _videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipChildren);
                    DsError.ThrowExceptionForHR(hr);

                    // Set the position and size while maintaining proportions
                    UpdateVideoPosition();

                    // Make it visible
                    hr = _videoWindow.put_Visible(OABool.True);
                    DsError.ThrowExceptionForHR(hr);
                }

                // 9. Start playback
                hr = _mediaControl.Run();
                DsError.ThrowExceptionForHR(hr);

                _isRunning = true;

                // Release captureGraphBuilder
                Marshal.ReleaseComObject(captureGraphBuilder);
            }
            catch (Exception ex)
            {
                StopPreview();
                throw new Exception($"Failed to start preview: {ex.Message}", ex);
            }
        }

        private void UpdateVideoPosition()
        {
            if (_videoWindow == null || _previewControl == null ||
                _previewControl.IsDisposed || !_previewControl.IsHandleCreated)
                return;

            Rectangle clientRect = _previewControl.ClientRectangle;

            if (_videoSize.Width > 0 && _videoSize.Height > 0)
            {
                // Calculate the size while maintaining proportions
                Rectangle videoRect = CalculateAspectRatioFit(
                    _videoSize.Width,
                    _videoSize.Height,
                    clientRect.Width,
                    clientRect.Height);

                // Center
                int x = clientRect.Left + (clientRect.Width - videoRect.Width) / 2;
                int y = clientRect.Top + (clientRect.Height - videoRect.Height) / 2;

                _videoWindow.SetWindowPosition(x, y, videoRect.Width, videoRect.Height);
            }
            else
            {
                // If the video size is unknown, use the entire client area
                _videoWindow.SetWindowPosition(
                    clientRect.Left,
                    clientRect.Top,
                    clientRect.Width,
                    clientRect.Height);
            }
        }

        private Rectangle CalculateAspectRatioFit(int srcWidth, int srcHeight, int maxWidth, int maxHeight)
        {
            double ratio = Math.Min((double)maxWidth / srcWidth, (double)maxHeight / srcHeight);
            int width = (int)(srcWidth * ratio);
            int height = (int)(srcHeight * ratio);

            return new Rectangle(0, 0, width, height);
        }

        public void StopPreview()
        {
            if (_isRunning)
            {
                try
                {
                    if (_mediaControl != null)
                    {
                        _mediaControl.Stop();
                    }

                    if (_videoWindow != null)
                    {
                        _videoWindow.put_Visible(OABool.False);
                        _videoWindow.put_Owner(IntPtr.Zero);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error stopping preview: {ex.Message}");
                }
                finally
                {
                    _isRunning = false;
                    _videoSize = Size.Empty;
                    _previewControl = null;
                }
            }
        }

        public void UpdatePreviewSize()
        {
            if (_isRunning && _videoWindow != null && _previewControl != null &&
                !_previewControl.IsDisposed && _previewControl.IsHandleCreated)
            {
                try
                {
                    UpdateVideoPosition();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating preview size: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            StopPreview();

            ReleaseComObject(_basicVideo);
            ReleaseComObject(_videoWindow);
            ReleaseComObject(_mediaControl);
            ReleaseComObject(_cameraFilter);
            ReleaseComObject(_filterGraph);

            _basicVideo = null;
            _videoWindow = null;
            _mediaControl = null;
            _cameraFilter = null;
            _filterGraph = null;
            _previewControl = null;

            GC.SuppressFinalize(this);
        }

        private void ReleaseComObject(object comObj)
        {
            if (comObj != null)
            {
                try
                {
                    Marshal.ReleaseComObject(comObj);
                }
                catch { }
            }
        }
    }
}