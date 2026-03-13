using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DirectShowLib;

namespace WebCamSettings
{
    public static class CameraSettingsDialog
    {
        public static void ShowSettingsDialog(DsDevice camera)
        {
            IBaseFilter filter = null;
            ISpecifyPropertyPages propPages = null;
            DsCAUUID cauuid = new DsCAUUID();

            try
            {
                // Create a filter
                Guid filterGuid = typeof(IBaseFilter).GUID;
                camera.Mon.BindToObject(null, null, ref filterGuid, out object filterObj);
                filter = filterObj as IBaseFilter;

                if (filter == null)
                {
                    throw new Exception("Cannot create camera filter");
                }

                // Get the interface for the properties
                propPages = filter as ISpecifyPropertyPages;
                if (propPages == null)
                {
                    MessageBox.Show("This camera does not support settings dialog.",
                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Get the GUID of the property pages
                int hr = propPages.GetPages(out cauuid);
                if (hr < 0 || cauuid.cElems == 0)
                {
                    MessageBox.Show("No settings available for this camera.",
                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Using ToGuidArray()
                Guid[] guids = cauuid.ToGuidArray();

                // Show a dialog with the camera name in the title
                string dialogTitle = $"Camera Settings - {camera.Name}";
                ShowPropertyDialog(filter, guids, dialogTitle);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Camera Settings",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Free up resources
                if (cauuid.pElems != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(cauuid.pElems);
                }
                if (propPages != null)
                {
                    Marshal.ReleaseComObject(propPages);
                }
                if (filter != null)
                {
                    Marshal.ReleaseComObject(filter);
                }
            }
        }

        private static void ShowPropertyDialog(IBaseFilter filter, Guid[] pageGuids, string dialogTitle)
        {
            IntPtr filterUnknown = Marshal.GetIUnknownForObject(filter);

            try
            {
                IntPtr[] unknownArray = new IntPtr[] { filterUnknown };

                // Call the API to show the dialog
                int hr = OleCreatePropertyFrame(
                    IntPtr.Zero,                    // parent window
                    0, 0,                           // x, y position
                    dialogTitle,                    // dialog title с именем камеры
                    1,                              // number of objects
                    unknownArray,                   // array of object pointers
                    (uint)pageGuids.Length,         // number of pages
                    pageGuids,                      // array of page GUIDs
                    0,                              // locale ID
                    0,                              // reserved
                    IntPtr.Zero);                   // reserved

                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
            }
            finally
            {
                Marshal.Release(filterUnknown);
            }
        }

        [DllImport("oleaut32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int OleCreatePropertyFrame(
            IntPtr hwndOwner,
            int x,
            int y,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszCaption,
            int cObjects,
            IntPtr[] ppUnk,
            uint cPages,
            Guid[] pPageClsID,
            int lcid,
            int dwReserved,
            IntPtr lpvReserved);
    }
}