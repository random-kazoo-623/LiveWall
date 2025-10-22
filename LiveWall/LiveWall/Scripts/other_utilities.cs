using LibVLCSharp.Forms.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LiveWall.Scripts
{
    internal class other_utilities
    {
        public static bool is_full_screen(IntPtr workerw, LibVLCSharp.WinForms.VideoView _videoview, IntPtr current_handl, string prod_name)
        {
            IntPtr foreground_window = GetForegroundWindow();
            //checks if nothing returns or the appication is from the app itself
            if (foreground_window == IntPtr.Zero || foreground_window == _videoview.MediaPlayer.Hwnd || foreground_window == current_handl || foreground_window == workerw)
            {
                return false;
            }

            string process_name = get_process_name(foreground_window);
            GetWindowRect(foreground_window, out RECT rect);

            if (process_name == "explorer" || process_name == "" || process_name == prod_name)
            {
                //if no process found or it is explorer
                //Debug.WriteLine("Not sus process {0}", process_name);
                return false;
            }

            int screen_width = GetSystemMetrics(SM_CXSCREEN);
            int screen_height = GetSystemMetrics(SM_CYSCREEN);

            //debug
            //Debug.WriteLine("width : {0}", screen_width);
            //Debug.WriteLine("height: {0}", screen_height);
            //Debug.WriteLine("process: {0}", process_name);

            //Debug.WriteLine(rect.Left.ToString());
            //Debug.WriteLine(rect.Right.ToString());
            //Debug.WriteLine(rect.Top.ToString());
            //Debug.WriteLine(rect.Bottom.ToString());

            //checks if the focused process dimension surpass a certain value

            if (rect.Left <= 0 && rect.Top <= 0 && rect.Right >= screen_width && rect.Bottom >= screen_height - 100)
            {
                //Debug.WriteLine("Pausing video");
                return true;
            }
            //Debug.WriteLine("Resuming video");
            return false;
        }

        public static string get_process_name(IntPtr hWnd)
        {
            uint process_id;
            GetWindowThreadProcessId(hWnd, out process_id);

            //if found a valid process 
            if (process_id != 0)
            {
                try
                {
                    Process process = Process.GetProcessById((int)process_id);
                    return process.ProcessName;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to retrive process name of {0}, error code: {1}.", process_id, e);
                    return "";
                }
            }
            //if nothing found
            return "";

        }

        //DLLs for window size detection

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }


        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /// <summary>
        /// Use a raised Mipmap limit version of RePKG to unpack pkg file into usable files
        /// </summary>
        /// <param name="file_path"></param>
        public static string extract_pkg(string file_path)
        {
            //file path must exists
            if (!File.Exists(file_path))
            {
                return "";
            }
            //get the extracted file directory
            string file_directory_name = "";
            Debug.WriteLine(Path.GetDirectoryName(file_path));

            for (int i = (Path.GetDirectoryName(file_path).Length) - 1; i >= 0; i-- )
            {
                if (Char.Equals(file_path[i], ("""\""")[0]) && i != Path.GetDirectoryName(file_path).Length)
                {
                    //Debug.WriteLine(file_path[i]);
                    break;
                }
                else
                {
                    //Debug.WriteLine(file_path[i]);
                    file_directory_name += file_path[i];
                }
            }
            char[] temp = file_directory_name.ToCharArray();
            Array.Reverse(temp);
            file_directory_name = new string(temp);
            string extracted_file_dir = """.\scenes\""" + file_directory_name + " ";

            Debug.WriteLine($"{extracted_file_dir}");

            if (Directory.Exists(extracted_file_dir))
            {
                return extracted_file_dir;
            }
            else
            {
                Directory.CreateDirectory(extracted_file_dir);
            }

            //now that it exists
            string RePKG_path = """.\RePKG\RePKG.exe""";
            string command_arguments = "extract -o " + extracted_file_dir + file_path;
            //Debug.WriteLine("command: {0}", command_arguments);
            RePKG_path = Path.GetFullPath(RePKG_path);
            var psi = Process.Start(RePKG_path, command_arguments);
            psi.WaitForExit();
            Debug.WriteLine($"extracted pkg file to path: {extracted_file_dir.ToString()}");
            return extracted_file_dir;
        }
    }
}
