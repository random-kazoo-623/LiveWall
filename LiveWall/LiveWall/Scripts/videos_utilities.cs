using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File = System.IO.File;

namespace LiveWall.Scripts
{
    internal class videos_utilities
    {
        //get the current global values
        private static string _videolink = Properties.Settings.Default.video_link;
        private static string _videofolder = Properties.Settings.Default.video_folder;
        private static string _rendermode = Properties.Settings.Default.render_mode;
        /// <summary>
        /// ask the user for a file and return the file path
        /// </summary>
        /// <returns>string file_path</returns>
        public static string get_videos()
        {
            //returns a string
            //checks if the current render mode is singular or multiple
            if (_rendermode == "")
            {
                _rendermode = "single";
            }

            switch (_rendermode)
            {
                case "single":
                    if (!File.Exists(_videolink) || string.IsNullOrEmpty(_videolink))
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
                        //get the file itself
                        string result = file_utilities.get_file();
                        if (string.IsNullOrEmpty(result))
                        {
                            return "";
                        }
                        else
                        {
                            _videolink = result;
                        }
                    }
                    return _videolink;
                case "multiple":
                    // checks if the video folder is saved
                    if (!Directory.Exists(_videofolder))
                    {
                        //ask the user for the video folder and save it
                        return file_utilities.get_folder();
                    }
                    else
                    {
                        return _videofolder;
                    }
                default:
                    Debug.WriteLine("Unexpected value of render type, reseting the property...");
                    _rendermode = "single";
                    configs_utilities.Save(rendermode: _rendermode);
                    return "unexpected rendermode";
            }
        }

        /// <summary>
        /// generates a randomised playlist with videos fetched from a folder path, returns an empty list if unsuccessful
        /// </summary>
        /// <param name="video_folder_optional"></param>
        /// <returns>List string video_list</returns>
        public static List<string> generate_playlist(string video_folder_optional = "")
        {
            //return a list of video

            //checks if the optional string is inputted
            if (!string.IsNullOrEmpty(video_folder_optional))
            {
                _videofolder = video_folder_optional;
            }

            //checks if playlist folder exist
            if (!Directory.Exists(_videofolder) || _videofolder == "")
            {
                //if folder not exist, ask the user for one
                string result = file_utilities.get_folder();
                if (string.IsNullOrEmpty(result))
                {
                    return new List<string>();
                }
                else
                {
                    _videofolder = result;
                }
            }

            Debug.WriteLine("video folder path: {0}", _videofolder);

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
                        Debug.WriteLine("Added {0}", file);
                        video_files.Add(file);
                        break;
                    case ".avi":
                        Debug.WriteLine("Added {0}", file);
                        video_files.Add(file);
                        break;
                    case ".mkv":
                        Debug.WriteLine("Added {0}", file);
                        video_files.Add(file);
                        break;
                    case ".mov":
                        Debug.WriteLine("Added {0}", file);
                        video_files.Add(file);
                        break;
                    case "gif":
                        Debug.WriteLine("Added {0}", file);
                        video_files.Add(file);
                        break;
                    default:
                        Debug.WriteLine("Did not add {0}", file);
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
                Debug.WriteLine(random_order[i].ToString());
                playlist.Add(files[random_order[i]]);
                Debug.WriteLine("added " + files[random_order[i]].ToString());
            }

            Debug.WriteLine("Video counts: {0}", playlist.Count());

            return playlist;
        }

        /// <summary>
        /// fetchs and get the video duration of a video file and return them in seconds
        /// </summary>
        /// <param name="video_path"></param>
        /// <returns>double video_duration</returns>
        public static double get_video_duration(string video_path)
        {
            //new new method since old one got bugged out smh

            //checks if ffprobe is installed:
            string ffprobe_abs_path = $"C:\ffmpeg\bin\ffprobe.exe";
            if (File.Exists(ffprobe_abs_path))
            {
                //if ffprobe is installed
                var psi = new ProcessStartInfo
                {
                    FileName = ffprobe_abs_path,
                    Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{video_path}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    string result = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    if (double.TryParse(result, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out double seconds))
                    {
                        return seconds;
                    }
                }
            }
            else Debug.WriteLine("ffprobe is not installed");

            //if ffprobe is not installed, use taglib
            using (var file = TagLib.File.Create(video_path))
            {
                return file.Properties.Duration.TotalSeconds;
            }
        }
    }
}
