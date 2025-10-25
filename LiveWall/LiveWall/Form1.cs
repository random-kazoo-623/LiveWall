using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using LiveWall.Scripts;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using TagLib;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using OpenTK.Core;
using File = System.IO.File;
namespace LiveWall
{
    public partial class Form1 : Form
    {

        //restructure some variables
        private LibVLC _libvlc;
        private MediaPlayer _player;
        private VideoView _videoview;

        private ContextMenuStrip _menuStrip;
        private NotifyIcon _trayicon;

        //load settings
        private string _videolink = Properties.Settings.Default.video_link;
        private string _videofolder = Properties.Settings.Default.video_folder;
        private string _rendermode = Properties.Settings.Default.render_mode;
        private int _videoloopmaxduration = Properties.Settings.Default.video_loop_max_duration;
        private string _taskbarstyle = Properties.Settings.Default.taskbar_style;
        //private bool _taskbar_default_on_fullscreen = Properties.Settings.Default.taskbar_default_on_fullscreen; NOT IMPLEMENTED

        private bool is_scene_pkg = false;


        private List<string> _videolist = new List<string>();
        private int _videolistorder = 0;
        private int _videoplaybackcount = 0;

        public Form1()
        {
            //main
            InitializeComponent();

            /*
             Planned work
            fully implement the workerw get
            add support for gif video files and scene based live wallpapers
            add support for pictures based wallpapers
            implement a transition animation between wallpapers
            add a dynamic resolution adjustion for when the user changes their monitor resolution.
            multi monitor setup???
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
            var result = videos_utilities.get_videos();


            if (!Directory.Exists(result))
            {
                //single file
                _videolink = result;
            }
            else if (result == "")
            {
                MessageBox.Show("User not want to set wallpaper(s), exiting...");
                this.Close();
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

                _videolink = """C:\Users\nongn\Desktop\2967775993_VSTHEMES-ORG\scene.pkg""";
                play_scene();
                Application.Exit();
                return;
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
                    //Debug.WriteLine("Checking for full screen...");
                    bool is_fsc = other_utilities.is_full_screen(workerw, _videoview, this.Handle, this.ProductName);

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
                Application.Exit();
            }
        }
        #region WorkerW
        private IntPtr get_workerw()
        {
            IntPtr progman = FindWindow("Progman", "");

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

            //METHOD TREE, YES TREE.
            //credits to lively maybe
            if (workerw == IntPtr.Zero)
            {
                Debug.WriteLine("Trying every params to find WorkerW");
                workerw = get_worker_w_force();
            }
            return workerw;
        }
        private IntPtr get_worker_w_force()
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
                    Debug.WriteLine("Found WorkerW, process {0}", workerw);
                    return workerw;
                }
            }

            //if not found then brute force by iterating all the windows, creds to shittim canvas for this ig
            //not now
            return IntPtr.Zero;
        }

        #endregion WorkerW
        private bool start_playing()
        {
            //checks for null video files
            if (_rendermode == "single" && !File.Exists(_videolink))
            {
                //if single and no video is found
                //tries to read from the settings
                _videolink = Properties.Settings.Default.video_link;
                if (string.IsNullOrEmpty(_videolink))
                {
                    string result = file_utilities.get_file();
                    if (!string.IsNullOrEmpty(result))
                    {
                        _videolink = result;
                    }
                }
            }

            //if after asking
            if (_rendermode == "single" && string.IsNullOrEmpty(_videolink))
            {
                return false;
            }

            //if render mode is multiple and nothing gets fetched
            if (_rendermode == "multiple" && string.IsNullOrEmpty(_videofolder) && _videolist.Count == 0)
            {
                //tries to read from setting
                _videofolder = Properties.Settings.Default.video_folder;
                _videolist.Clear();
                _videolist = videos_utilities.generate_playlist(_videofolder);
            }

            //if after asking
            if (_rendermode == "multiple" && string.IsNullOrEmpty(_videofolder) && _videolist.Count == 0)
            {
                return false;
            }

            //checks the video if it is a pkg file
            if (_rendermode == "single" && Path.GetExtension(_videolink) == ".pkg")
            {
                //redirect to scene based renderer and hides the current player
                _videoview.Visible = false;
                bool result = play_scene();
                if (!result)
                {
                    return false;
                }
                else return true;
            }

            Media media;
            //use vlc to play a video repetively
            if (_rendermode == "single")
            {
                media = new Media(_libvlc, _videolink, FromType.FromPath);
                if (_videoview.Visible = false)
                {
                    _videoview.Visible = true;
                }
            }
            else if (_rendermode == "multiple")
            {
                //just generate another randomised playlist
                _videolist.Clear();
                _videolist = videos_utilities.generate_playlist();
                media = new Media(_libvlc, _videolist[_videolistorder], FromType.FromPath);
                //calculate time
                double duration = videos_utilities.get_video_duration(_videolist[_videolistorder]);
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
                    //creates a new media just in case
                    media = new Media(_libvlc, _videolink, FromType.FromPath);
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
                        _videolistorder++;
                        if (_videolistorder == _videolist.Count)
                        {
                            _videolistorder = 0;
                        }
                        media = new Media(_libvlc, _videolist[_videolistorder], FromType.FromPath);
                        //calculate time
                        double duration = videos_utilities.get_video_duration(_videolist[_videolistorder]);
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

        /// <summary>
        /// Play the scene pkg using renderers
        /// </summary>
        /// <returns>bool result</returns>
        private bool play_scene()
        {
            //display a notification
            NotifyIcon progress = new NotifyIcon();
            _trayicon.ShowBalloonTip(2000, "Setting scene Wallpaper...", "Unpacking and setting up the scene wallpaper, please be patient...", ToolTipIcon.Warning);

            progress.Dispose();


            //extract the pkg file
            string  result = other_utilities.extract_pkg(_videolink);
            if (string.IsNullOrEmpty(result))
            {
                return false;
            }

            //we will have the directory

            var scene_json = result + "gifscene.json";
            if (!File.Exists(scene_json))
            {
                //if gifscene.json not found
                scene_json = result + """\scene.json""";
                if (File.Exists(scene_json))
                {
                    //if nothing found after extracting
                    Debug.WriteLine("Error: main scene json not found in directory {0}", result);
                    return false;
                }
            }

            //read the scene json
            var root = JsonSerializer.Deserialize<SceneClass.GifScene>(File.ReadAllText(scene_json));

            return false;
        }

        #region tray icon
        private bool init_system_tray_icon()
        {
            //LOL GIT DID NOT SAVE FOR ME AAAA
            _menuStrip = new ContextMenuStrip();
            _menuStrip.Items.Add("Reload Wallpaper(s)", null, reload_wallpaper);
            _menuStrip.Items.Add("Change Wallpaper(s)", null, choose_wallpaper);
            _menuStrip.Items.Add("Change render mode", null, change_render_mode);
            _menuStrip.Items.Add(new ToolStripSeparator());
            _menuStrip.Items.Add("Open Wallpaper Folder", null, open_wallpaper_location);

            //NOT IMPLEMENTED/REMOVED FEATURES
            ////create taskbar traymenu dropdown box
            //var taskbar_menu = new ToolStripMenuItem("Taskbar Options");
            //taskbar_menu.DropDownItems.Add("Make taskbar invisible (fully)", null, make_taskbar_invisible);
            //taskbar_menu.DropDownItems.Add("Make taskbar opaque", null, make_taskbar_opaque);
            //taskbar_menu.DropDownItems.Add("Make taskbar solid", null, make_taskbar_default);
            //taskbar_menu.DropDownItems.Add("Turn normal on full screen", null, make_taskbar_default_on_fullscreen);

            ////add the traymenu
            //_menuStrip.Items.Add(new ToolStripSeparator());
            //_menuStrip.Items.Add(taskbar_menu);

            _menuStrip.Items.Add(new ToolStripSeparator());
            _menuStrip.Items.Add("Exit", null, exit_wallpaper);


            _trayicon = new NotifyIcon
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
                Media media;
                if (string.IsNullOrEmpty(_videolink) || !File.Exists(_videolink))
                {
                    videos_utilities.get_videos();
                    _videolink = configs_utilities.Sync(videolink: "load");
                }

                //if the file is a pkg file
                if (Path.GetExtension(_videolink) == ".pkg")
                {
                    _videoview.Visible = false;
                    play_scene();
                    return;
                }

                try
                {
                    media = new Media(_libvlc, _videolink, FromType.FromPath);
                }
                catch (Exception ex)
                {
                    //if for some reason _videolink IS STILL EMPTY
                    //load video link from setting file
                    _videolink = Properties.Settings.Default.video_link;
                    media = new Media(_libvlc, _videolink, FromType.FromPath);
                }

                Debug.WriteLine("playing video {0}", _videolink);
                BeginInvoke(new Action(() => _player.Play(media)));
            }
            else
            {
                //re generate playlist
                _videolist.Clear();
                _videolist = videos_utilities.generate_playlist();
                _videolistorder = 0;
                Media media = new Media(_libvlc, _videolist[_videolistorder], FromType.FromPath);
                //calculate time
                double duration = videos_utilities.get_video_duration(_videolist[_videolistorder]);
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
                _videolink = videos_utilities.get_videos();
                configs_utilities.Save(videolink: _videolink);
                reload_wallpaper(null, null);
            }
            else if (_rendermode == "multiple")
            {
                _videolist.Clear();
                _videolist = videos_utilities.generate_playlist();
                reload_wallpaper(null, null);

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
            configs_utilities.Save(rendermode: _rendermode, videofolder: _videofolder, videolink: _videolink, videoloopmaxduration: _videoloopmaxduration, taskbarstyle: _taskbarstyle);
            Application.Exit();
        }

        private void skip_wallpaper()
        {
            _videolistorder++;
            if (_videolistorder == _videolist.Count)
            {
                _videolistorder = 0;
            }
            //re load the wallpaper

            Media media = new Media(_libvlc, _videolist[_videolistorder], FromType.FromPath);
            //calculate time
            double duration = videos_utilities.get_video_duration(_videolist[_videolistorder]);
            //Debug.WriteLine("duration {0}", duration);
            //convert to seconds
            int time = Convert.ToInt32(duration);
            //Debug.WriteLine("length {0}", time);
            _videoplaybackcount = get_repeat_count(time);
            BeginInvoke(new Action(() => _player.Play(media)));
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
            configs_utilities.Save(videoloopmaxduration: _videoloopmaxduration);
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

        private int get_repeat_count(int time)
        {
            int result = 0;

            result = (_videoloopmaxduration / time);
            Debug.WriteLine("Repeat count {0}", result);
            //just in case
            result++;
            return result;
        }

        private void form_closing(object sender, FormClosingEventArgs e)
        {
            configs_utilities.Save(rendermode: _rendermode, videofolder: _videofolder, videolink: _videolink, videoloopmaxduration: _videoloopmaxduration, taskbarstyle: _taskbarstyle);
            Thread.Sleep(300);
            return;
        }

        #endregion tray icon

        #region DLL imports
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
        #endregion DLL imports
    }
}

