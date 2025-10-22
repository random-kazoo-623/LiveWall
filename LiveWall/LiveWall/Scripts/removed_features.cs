using LibVLCSharp.Shared;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveWall.Scripts
{
    internal class removed_features
    {
        private string _taskbarstyle;
        private bool _taskbar_default_on_fullscreen;
        private void make_taskbar_invisible(object sender, EventArgs e)
        {
            //make taskbar invisible so yeah
            _taskbarstyle = "invisible";
            IntPtr taskbar_handl = get_taskbar_handl();
            if (taskbar_handl == IntPtr.Zero)
            {
                //if cannot find taskbar
                MessageBox.Show("Error, cannot set the taskbar style due to the taskbar doesn't exist?");
                return;
            }
            //apply the taskbar style
            set_taskbar_style(AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT, Color.Transparent, 0); //this almost worked but it blurs the taskbar not making it visible throughly
            Debug.WriteLine("Set taskbar to invisible");
            return;
        }
        private void make_taskbar_default(object sender, EventArgs e)
        {
            _taskbarstyle = "default";
            IntPtr taskbar_handl = get_taskbar_handl();
            if (taskbar_handl == IntPtr.Zero)
            {
                //if cannot find taskbar
                MessageBox.Show("Error, cannot set the taskbar style due to the taskbar doesn't exist?");
                return;
            }
            //apply the taskbar style
            set_taskbar_style(AccentState.ACCENT_DISABLED);
            Debug.WriteLine("Set taskbar to default");
            return;
        }
        private void make_taskbar_opaque(object sender, EventArgs e)
        {
            _taskbarstyle = "opaque";
            IntPtr taskbar_handl = get_taskbar_handl();
            if (taskbar_handl == IntPtr.Zero)
            {
                //if cannot find taskbar
                MessageBox.Show("Error, cannot set the taskbar style due to the taskbar doesn't exist?");
                return;
            }
            //apply the taskbar style
            set_taskbar_style(AccentState.ACCENT_ENABLE_BLURBEHIND);
            Debug.WriteLine("Set taskbar to opaque");
            return;
        }
        //credits to lively for c# implementation of TranslucentTB, you are the goat! 
        //Well the code hasnt updated in like forever... And the win11 updates broke it...
        //seemed like i have to make sense and translate cpp codes from translucentTB to c#
        //or i could just drop in a copy of translucentTB in here and execute it...
        private IntPtr get_taskbar_handl()
        {
            //find the first taskbar handle
            IntPtr taskbar_handl = FindWindow("Shell_TrayWnd", null);
            if (taskbar_handl != IntPtr.Zero)
            {
                Debug.WriteLine("Found taskbar: {0}", taskbar_handl);
            }
            else
            {
                Debug.WriteLine("Error, cannot find the main taskbar handle for some reason.");
            }
            return taskbar_handl;
        }

        //style helper
        private void set_taskbar_style(AccentState state, Color? tint = null, byte opacity = 0)
        {
            //set the taskbar appearance yes sir
            IntPtr taskbar_handl = get_taskbar_handl();
            if (taskbar_handl == IntPtr.Zero)
            {
                MessageBox.Show("Error, program cannot find the taskbar anywhere. Returning...");
                return;
            }

            AccentPolicy accent = new AccentPolicy();
            accent.AccentState = state;

            //argb
            int color = (opacity << 24) | (tint?.ToArgb() ?? 0);

            accent.GradientColor = color;

            IntPtr accentPtr = Marshal.AllocHGlobal(Marshal.SizeOf(accent));
            Marshal.StructureToPtr(accent, accentPtr, false);

            WindowCompositionAttributeData data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = Marshal.SizeOf(accent);
            data.Data = accentPtr;

            SetWindowCompositionAttribute(taskbar_handl, ref data);

            Marshal.FreeHGlobal(accentPtr);

        }

        private void make_taskbar_default_on_fullscreen(object sender, EventArgs e)
        {
            //NOT IMPLEMENTED
            if (_taskbar_default_on_fullscreen == true)
            {
                //disable it and display the current taskbar style instead
                _taskbar_default_on_fullscreen = false;
            }
            else
            {
                _taskbar_default_on_fullscreen = true;
            }
            return;


        }

        //taskbar ultils

        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string className, string winName);

        private enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
            ACCENT_ENABLE_HOSTBACKDROP = 5,
            ACCENT_INVALID_STATE = 6
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        private enum WindowCompositionAttribute
        {
            WCA_ACCENT_POLICY = 19
        }
    }
}
