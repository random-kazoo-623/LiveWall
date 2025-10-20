using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System;
using Shell32;
using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Xamarin.Forms.StyleSheets;
namespace LiveWall
{
    public partial class Form1 : Form
    {
        //restructure some variables
        private LibVLC _libvlc;
        private MediaPlayer _player;
        private VideoView _videoview;

        private ContextMenuStrip _menuStrip;

        private string _videolink = Properties.Settings.Default.video_link;
        private string _videofolder = Properties.Settings.Default.video_folder;
        private string _rendermode = Properties.Settings.Default.render_mode;
        private int _videoloopmaxduration = Properties.Settings.Default.video_loop_max_duration;


        private List<string> _videolist = new List<string>();
        private int _videolistorder = 0;
        private int _videoplaybackcount = 0;

        public Form1()
        {
            InitializeComponent();

            /*
             Planned work
            fully implement the workerw get
            implement a clear taskbar look
            add support for gif video files and scene based live wallpapers

             */

            _videoview = new VideoView
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(_videoview);

            //init vlc
            _libvlc = new LibVLC();
            _player = new MediaPlayer(_libvlc);

            //will pick 1 videos only and save its link to a text file
            var result = get_videos();


            if (!Directory.Exists(result))
            {
                //single file
                _videolink = result;
            }
            else
            {
                //folder
                _videofolder = result;
            }

            // then lets find the workerw handle layer
            IntPtr workerw = get_workerw();
            if (workerw != IntPtr.Zero)
            {
                //get current screen resolution
                int screen_width = Screen.PrimaryScreen.Bounds.Width;
                int screen_height = Screen.PrimaryScreen.Bounds.Height;
                Debug.WriteLine("Current resolution: {0} x {1}", screen_width, screen_height);

                //full screen
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                this.Bounds = Screen.PrimaryScreen.Bounds;
                SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, screen_width, screen_height, SetWindowsPosFlags.SWP_NOZORDER);

                //re parent
                SetParent(this.Handle, workerw);
                SetParent(_videoview.Handle, workerw);

                //init system tray icon for iteractions
                init_system_tray_icon();

                //check render mode to correctly reflect the previous saved application state
                if (_rendermode == "multiple")
                {
                    _rendermode = "single";
                    change_render_mode(null, null);
                }

                //i have no idea how to name this variable
                bool temp = start_playing();
                if (!temp)
                {
                    MessageBox.Show("Error, cannot play wallpaper due to user not obeying orders, terminating...");
                    return;
                }

                //timer loop to check whenether a full screen application is in effect
                var timer = new System.Windows.Forms.Timer { Interval = 300 };
                timer.Tick += (sender, e) =>
                {
                    //Debug.WriteLine("Checking for apps...");
                    bool is_fsc = is_full_screen(workerw);


                    if (is_fsc)
                    {
                        // if already paused
                        if (!_player.IsPlaying)
                        {
                            return;
                        }
                        _player.SetPause(true);
                    }
                    else
                    {
                        // if not paused
                        if (_player.IsPlaying)
                        {
                            return;
                        }
                        _player.SetPause(false);
                    }
                };
                timer.Start();
            }
            else
            {
                Debug.WriteLine("WorkerW not found.");

            }
        }

        private IntPtr get_workerw()
        {
            IntPtr progman = FindWindow("Progman", null);

            IntPtr result = SendMessageTimeout(progman,
                                0x052c,
                                IntPtr.Zero,
                                IntPtr.Zero,
                                0x0000,
                                1000,
                                0);
            //Debug.WriteLine(result);
            if (result != IntPtr.Zero)
            {
                Debug.WriteLine("spawned sheldll.");
            }
            else return IntPtr.Zero;
            // find shell_defview and workerW
            IntPtr workerw = IntPtr.Zero;
            IntPtr shell_def = IntPtr.Zero;

            EnumWindows(new EnumWindowsProc((tophandle, topparamhandle) =>
            {
                IntPtr p = FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", IntPtr.Zero);

                if (p != IntPtr.Zero)
                {
                    Debug.WriteLine("Found SHELLDLL_DefView, process {0}", p);
                    workerw = FindWindowEx(IntPtr.Zero, tophandle, "WorkerW", IntPtr.Zero);
                    if (workerw == IntPtr.Zero)
                    {
                        //if nothing found then find the top handle instead
                        Debug.WriteLine("Cannot find WorkerW, falling back to find top handle instead.");
                        workerw = FindWindowEx(tophandle, IntPtr.Zero, "WorkerW", IntPtr.Zero);
                        if (workerw == IntPtr.Zero)
                        {
                            Debug.WriteLine("Failed to find WorkerW.");

                        }
                        else Debug.WriteLine("Found WorkerW, process {0}", workerw);
                    }
                    else Debug.WriteLine("Found WorkerW, process {0}", workerw);
                }
                return true;
            }), IntPtr.Zero);

            //try one more time on progman if WorkerW still hasnt been found
            if (workerw == IntPtr.Zero)
            {
                Debug.WriteLine("Trying 1 more time just to be sure");
                workerw = FindWindowEx(progman, IntPtr.Zero, "WorkerW", IntPtr.Zero);
                Debug.WriteLine("Found WorkerW? Process {0}", workerw);

            }

            //METHOD TREE, YES TREE.
            //credits to lively maybe
            if (workerw == IntPtr.Zero)
            {
                workerw = get_worker_w_slave();
            }
            return workerw;
        }

        private IntPtr get_worker_w_slave()
        {
            //yes yes very controversial name
            IntPtr result = IntPtr.Zero;

            //combination of params to try every method available ig
            var combinations = new[]
            {
                new {wParam = new IntPtr(0), lParam = new IntPtr(0)},
                new {wParam = new IntPtr(0xD),lParam = new IntPtr(0x1)},
                new {wParam = new IntPtr(0),lParam = new IntPtr(1)},
                new {wParam = new IntPtr(1),lParam = new IntPtr(0)},
            };

            //get progman
            IntPtr progman = FindWindow("Progman", null);

            foreach (var comb in combinations)
            {
                SendMessageTimeout(progman, 0x052C, comb.wParam, comb.lParam, 0x0000, 1000, result);


                Thread.Sleep(100);

                //now find workerw
                IntPtr workerw = FindWindowEx(progman, IntPtr.Zero, "WorkerW", IntPtr.Zero);
                if (workerw != IntPtr.Zero)
                {
                    //if found
                    return workerw;
                }
            }

            //if not found then brute force by interating all the windows, creds to shittim canvas for this ig
            //not now
            return IntPtr.Zero;
        }

        private string get_videos()
        {
            //checks if the current render mode is singular or multiple
            if (_rendermode == "")
            {
                _rendermode = "single";
            }

            switch (_rendermode)
            {
                case "single":
                    if (!File.Exists(_videolink) && _videolink != "")
                    {
                        //if file does not exist
                        MessageBox.Show("Error, video file/wallpaper not found, is the file deleted?");
                    }
                    else
                    {
                        return _videolink;
                    }

                    if (_videolink == "" || !File.Exists(_videolink))
                    {
                        //ask the user for the video path and then writes it to text_path
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        openFileDialog.Filter = "Video files (*.mp4)|*.mp4";
                        openFileDialog.CheckFileExists = true;
                        openFileDialog.Title = "Choose a live wallpaper";

                        try
                        {
                            DialogResult result = openFileDialog.ShowDialog();
                            if (result == DialogResult.OK)
                            {
                                _videolink = openFileDialog.FileName;
                                save_configs();
                                return _videolink;
                            }
                            else if (result == DialogResult.Cancel)
                            {
                                MessageBox.Show("User cancelled file input, returning...");
                                return "";
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Error: could not read file from disk or something went horribly wrong. " + e);
                        }
                    }
                    return _videolink;
                case "multiple":
                    generate_playlist();
                    break;
                default:
                    Debug.WriteLine("Unexpected value of render type, reseting the property...");
                    _rendermode = "";
                    return "";
            }
            //just in case
            return "";
        }

        private bool start_playing()
        {
            //checks for null video files
            if (_rendermode == "single" && string.IsNullOrEmpty(_videolink))
            {
                //if single and no video is found
                //tries to read from the settings
                _videolink = Properties.Settings.Default.video_link;
                if (string.IsNullOrEmpty(_videolink))
                {
                    //if not then ask the user for 1
                    MessageBox.Show("Error, video file/wallpaper not found, is the file deleted?");
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "Video files (*.mp4)|*.mp4";
                    openFileDialog.CheckFileExists = true;
                    openFileDialog.Title = "Choose a live wallpaper";

                    try
                    {
                        DialogResult result = openFileDialog.ShowDialog();
                        if (result == DialogResult.OK)
                        {
                            _videolink = openFileDialog.FileName;
                            save_configs();
                        }
                        else if (result == DialogResult.Cancel)
                        {
                            MessageBox.Show("User cancelled file input, returning...");
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Error: could not read file from disk or something went horribly wrong. " + e);
                    }
                }
            }

            //if after asking
            if (_rendermode == "single" && string.IsNullOrEmpty(_videolink))
            {
                return false;
            }

            //if render mode is multiple and nothing gets fetched
            if (_rendermode == "multiple" && string.IsNullOrEmpty(_videofolder) &&  _videolist.Count == 0)
            {
                //tries to read from setting
                _videofolder = Properties.Settings.Default.video_folder;
                generate_playlist();
            }

            Media media;
            //use vlc to play a video repetively
            if (_rendermode == "single")
            {
                media = new Media(_libvlc, _videolink, FromType.FromPath);
            }
            else if (_rendermode == "multiple")
            {
                //just generate another randomised playlist
                generate_playlist();
                media = new Media(_libvlc, _videolist[_videolistorder], FromType.FromPath);
                //calculate time
                double duration = get_video_duration(_videolist[_videolistorder]);
                //Debug.WriteLine("duration {0}", duration);
                //convert to seconds
                int time = Convert.ToInt32(duration);
                //Debug.WriteLine("length {0}", time);
                _videoplaybackcount = get_repeat_count(time);
                BeginInvoke(new Action(() => _player.Play(media)));
                _videolistorder = 0;
            }
            else
            {
                _rendermode = "single";
                media = new Media(_libvlc, _videolink, FromType.FromPath);
            }

            Debug.WriteLine("Playing video");
            _player.Play(media);
            _videoview.MediaPlayer = _player;

            _player.EndReached += (sender, e) =>
            {
                //looping
                Debug.WriteLine("Video ended");

                //checks render mode
                if (_rendermode == "single")
                {
                    BeginInvoke(new Action(() => _player.Play(media)));
                }
                else if (_rendermode == "multiple")
                {
                    if (_videoplaybackcount > 0)
                    {
                        //Debug.WriteLine("the video still needs to contiue");
                        media = new Media(_libvlc, _videolist[_videolistorder], FromType.FromPath);
                        BeginInvoke(new Action(() => _player.Play(media)));
                        _videoplaybackcount--;

                    }
                    else
                    {
                        //Debug.WriteLine("the video is finished, entirely.");
                        _videolistorder ++;
                        if (_videolistorder == _videolist.Count)
                        {
                            _videolistorder = 0;
                        }
                        media = new Media(_libvlc, _videolist[_videolistorder], FromType.FromPath);
                        //calculate time
                        double duration = get_video_duration(_videolist[_videolistorder]);
                        //Debug.WriteLine("duration {0}", duration);
                        //convert to seconds
                        int time = Convert.ToInt32(duration);
                        //Debug.WriteLine("length {0}", time);
                        _videoplaybackcount = get_repeat_count(time);
                        BeginInvoke(new Action(() => _player.Play(media)));
                    }
                }
                else
                {
                    //if rendermode bugs out
                    BeginInvoke(new Action(() => _player.Play(media)));
                    _rendermode = "single";
                }
            };



            return true;
        }

        private void generate_playlist()
        {
            //checks if playlist folder exist
            if (!Directory.Exists(_videofolder) || _videofolder == "")
            {
                //if folder not exist, ask the user for one
                MessageBox.Show("Please choose the folder containing the wallpapers.");
                FolderBrowserDialog folder_browser_dialog = new FolderBrowserDialog();
                folder_browser_dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                folder_browser_dialog.Description = "Select the wallpaper video folder:";

                if (folder_browser_dialog.ShowDialog() == DialogResult.OK)
                {
                    _videofolder = folder_browser_dialog.SelectedPath;
                }
            }

            //now fetchs all files
            string[] files = Directory.GetFiles(_videofolder);
            List<string> video_files = new List<string>();
            //change to list for items manipulation
            foreach (string file in files)
            {
                //Debug.WriteLine("checking extension {0}...", Path.GetExtension(file));
                switch (Path.GetExtension(file))
                {
                    case ".mp4":
                        //Debug.WriteLine("Added {0}", file);
                        video_files.Add(file);
                        break;
                    case ".avi":
                        //Debug.WriteLine("Added {0}", file);
                        video_files.Add(file);
                        break;
                    case ".mkv":
                        //Debug.WriteLine("Added {0}", file);
                        video_files.Add(file);
                        break;
                    case ".mov":
                        //Debug.WriteLine("Added {0}", file);
                        video_files.Add(file);
                        break;
                    default:
                        //Debug.WriteLine("Did not add {0}", file);
                        break;
                }
            }

            //now that every file should be a playable media
            Random r = new Random();
            List<int> random_order = new List<int>();
            int count = 0;
            while (count < video_files.Count)
            {
                int number = r.Next(video_files.Count);
                bool ispresent = random_order.Contains(number);
                if (ispresent)
                {
                    //Debug.WriteLine($"loser {number}");
                    continue;
                }
                else
                {
                    //Debug.WriteLine($"winner {number}");
                    random_order.Add(number);
                    count++;
                }
                if (count == video_files.Count)
                {
                    break;
                }
            }

            List<string> playlist = new List<string>();
            for (int i = 0; i < video_files.Count; i++)
            {
                //Debug.WriteLine(ramdon[i].ToString());
                playlist.Add(files[random_order[i]]);
                //Debug.WriteLine("added " + files[random_order[i]].ToString());
            }
            _videolist = playlist;
        }

        private double get_video_duration(string video_path)
        {
            //new method since old one got bugged out smh
            var shell = new Shell();
            var folder = shell.NameSpace(System.IO.Path.GetDirectoryName(video_path));
            var item = folder.ParseName(System.IO.Path.GetFileName(video_path));

            string duration_str = folder.GetDetailsOf(item, 27);

            if (TimeSpan.TryParse(duration_str, out var duration))
            {
                return duration.TotalSeconds;
            }

            if (TimeSpan.TryParseExact(duration_str, @"h\:mm\:ss", null, out duration))
                return duration.TotalSeconds;

            return 0;
        }

        private bool init_system_tray_icon()
        {
            //LOL GIT DID NOT SAVE FOR ME AAAA
            _menuStrip = new ContextMenuStrip();
            _menuStrip.Items.Add("Reload Wallpaper(s)", null, reload_wallpaper);
            _menuStrip.Items.Add("Change Wallpaper(s)", null, choose_wallpaper);
            _menuStrip.Items.Add("Change render mode", null, change_render_mode);
            _menuStrip.Items.Add(new ToolStripSeparator());
            _menuStrip.Items.Add("Open Wallpaper Folder", null, open_wallpaper_location);
            _menuStrip.Items.Add(new ToolStripSeparator());
            _menuStrip.Items.Add("Exit", null, exit_wallpaper);


            NotifyIcon tray_icon = new NotifyIcon
            {
                Visible = true,
                Text = "Live Wallpaper",
                ContextMenuStrip = _menuStrip,
                Icon = Properties.Resources.logo,
            };


            return true;
        }

        private void reload_wallpaper(object sender, EventArgs e)
        {
            if (_rendermode == "single")
            {
                if (string.IsNullOrEmpty(_videolink))
                {
                    get_videos();
                }
                Media media = new Media(_libvlc, _videolink, FromType.FromPath);
                BeginInvoke(new Action(() => _player.Play(media)));
            }
            else
            {
                //re generate playlist
                generate_playlist();
                _videolistorder = 0;
                Media media = new Media(_libvlc, _videolist[_videolistorder], FromType.FromPath);
                //calculate time
                double duration = get_video_duration(_videolist[_videolistorder]);
                //Debug.WriteLine("duration {0}", duration);
                //convert to seconds
                int time = Convert.ToInt32(duration);
                //Debug.WriteLine("length {0}", time);
                _videoplaybackcount = get_repeat_count(time);
                BeginInvoke(new Action(() => _player.Play(media)));
            }
            return;
        }

        private void choose_wallpaper(object sender, EventArgs e)
        {
            if (_rendermode == "single")
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Video files (*.mp4)|*.mp4";
                openFileDialog.CheckFileExists = true;
                openFileDialog.Title = "Choose a live wallpaper";
                DialogResult result = openFileDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    _videolink = openFileDialog.FileName;
                    save_configs();
                    reload_wallpaper(null, null);
                }
                else if (result == DialogResult.Cancel)
                {
                    MessageBox.Show("User cancelled file input, returning...");
                    return;
                }
            }
            else if (_rendermode == "multiple")
            {
                MessageBox.Show("Please choose the folder containing the wallpapers.");
                FolderBrowserDialog folder_browser_dialog = new FolderBrowserDialog();
                folder_browser_dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                folder_browser_dialog.Description = "Select the wallpaper video folder:";

                if (folder_browser_dialog.ShowDialog() == DialogResult.OK)
                {
                    _videofolder = folder_browser_dialog.SelectedPath;
                    save_configs();
                    reload_wallpaper(null, null);
                }
                else if (folder_browser_dialog.ShowDialog() == DialogResult.Cancel)
                {
                    MessageBox.Show("User cancelled folder input, returning...");
                    return;
                }
            }

        }

        private void change_render_mode(object sender, EventArgs e)
        {
            //change the render mode so ye
            if (_rendermode == "single")
            {
                // next wallpaper
                _rendermode = "multiple";
                ToolStripItem item = new ToolStripMenuItem();
                item.Text = "Next Wallpaper";
                item.Click += (sender, e) => skip_wallpaper();
                _menuStrip.Items.Insert(3, item);

                ToolStripItem item_2 = new ToolStripMenuItem();
                item_2.Text = "Change Video Duration";
                item_2.Click += (sender, e) => change_loop_duration();
                _menuStrip.Items.Insert(4, item_2);
            }
            else
            {
                _rendermode = "single";
                _menuStrip.Items.RemoveAt(3);
                _menuStrip.Items.RemoveAt(3);
            }
            reload_wallpaper(null, null);
            return;
        }

        private void exit_wallpaper(object sender, EventArgs e)
        {
            save_configs();
            Application.Exit();
        }

        private void skip_wallpaper()
        {
            _videolistorder++;
            if (_videolistorder == _videolist.Count)
            {
                _videolistorder = 0;
            }
            reload_wallpaper(null, null);
            Debug.WriteLine("Skipping wallpaper");
        }

        private void change_loop_duration()
        {
            int duration = 0;
            while (true)
            {
                try
                {
                    string userinput = Interaction.InputBox("Change the loop duration", "Enter Value", _videoloopmaxduration.ToString());
                    duration = Convert.ToInt32(userinput);
                    if (duration <= 0)
                    {
                        MessageBox.Show("Error, please enter a valid number (bigger than 0, whole number)");
                        continue;
                    }
                    break;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error, please enter a valid number (bigger than 0, whole number)");
                }
            }

            _videoloopmaxduration = duration;

            if (_videoloopmaxduration <= 0)
            {
                //make sure the user dont enter something invalid
                _videoloopmaxduration = 180;
            }
            save_configs();
            return;
        }
        private void open_wallpaper_location(object sender, EventArgs e)
        {
            if (_rendermode == "single")
            {
                Process.Start("explorer.exe", Path.GetDirectoryName(_videolink));
            }
            else
            {
                Process.Start("explorer.exe", _videofolder);
            }
            return;
        }

        private bool is_full_screen(IntPtr workerw)
        {
            IntPtr foreground_window = GetForegroundWindow();
            //checks if nothing returns or the appication is from the app itself
            if (foreground_window == IntPtr.Zero || foreground_window == _videoview.MediaPlayer.Hwnd || foreground_window == this.Handle || foreground_window == workerw)
            {
                return false;
            }

            string process_name = get_process_name(foreground_window);
            GetWindowRect(foreground_window, out RECT rect);

            if (process_name == "explorer" || process_name == "" || process_name == this.ProductName)
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

        private string get_process_name(IntPtr hWnd)
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

        private int get_repeat_count(int time)
        {
            int result = 0;

            result = (_videoloopmaxduration / time);
            Debug.WriteLine("Repeat count {0}", result);
            //just in case
            result++;
            return result;
        }

        private void save_configs()
        {
            Properties.Settings.Default.render_mode = _rendermode;
            Properties.Settings.Default.video_folder = _videofolder;
            Properties.Settings.Default.video_link = _videolink;
            Properties.Settings.Default.video_loop_max_duration = _videoloopmaxduration;
            Properties.Settings.Default.Save();
            return;
        }

        private void form_closing(object sender, FormClosingEventArgs e)
        {
            save_configs();
            Thread.Sleep(300);
            return;
        }
        //DLLs
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string className, string winName);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageTimeout(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, uint fuFlage, uint timeout, IntPtr result);


        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc proc, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string className, IntPtr winName);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hwnd, IntPtr parentHwnd);

        //set windows
        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int Width, int Height, SetWindowsPosFlags uFlags);
        [Flags]
        public enum SetWindowsPosFlags : uint
        {
            SWP_NOSIZE = 0x0001,
            SWP_NOMOVE = 0X0002,
            SWP_NOZORDER = 0X0004,
            SWP_NOACTIVATE = 0X0010,
            SWP_SHOWWINDOW = 0X0040
        }

        //flags for no alt tab
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                //prevent alt+tab
                cp.ExStyle |= 0x80;
                cp.ExStyle |= 0x20;
                cp.ExStyle |= 0x8000000;
                return cp;
            }
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
    }
}

