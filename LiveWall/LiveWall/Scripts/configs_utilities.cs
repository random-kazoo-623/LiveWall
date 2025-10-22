using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveWall.Scripts
{
    internal class configs_utilities
    {
        public static void Save(string rendermode = "", string videofolder = "", string videolink = "", int videoloopmaxduration = -1, string taskbarstyle = "")
        {
            //if optional settings are not eneterd
            if (string.IsNullOrEmpty(rendermode))
            {
                rendermode = Properties.Settings.Default.render_mode;
            }
            if (string.IsNullOrEmpty(videofolder))
            {
                videofolder = Properties.Settings.Default.video_folder;
            }
            if (string.IsNullOrEmpty(videolink))
            {
                videolink = Properties.Settings.Default.video_link;
            }
            if (videoloopmaxduration == -1)
            {
                videoloopmaxduration = Properties.Settings.Default.video_loop_max_duration;
            }
            if (string.IsNullOrEmpty(taskbarstyle))
            {
                taskbarstyle = Properties.Settings.Default.taskbar_style;
            }
            Properties.Settings.Default.render_mode = rendermode;
            Properties.Settings.Default.video_folder = videofolder;
            Properties.Settings.Default.video_link = videolink;
            Properties.Settings.Default.render_mode = rendermode;
            Properties.Settings.Default.taskbar_style = taskbarstyle;
            Properties.Settings.Default.video_loop_max_duration = videoloopmaxduration;
            Properties.Settings.Default.Save();
            return;
        }

        public static dynamic Sync(string rendermode = "", string videofolder = "", string videolink = "", int videoloopmaxduration = -1, string taskbarstyle = "")
        {
            //get a setting from settings and return it, only 1 value is returned.
            //if optional settings are not eneterd
            if (!string.IsNullOrEmpty(rendermode))
            {
                return Properties.Settings.Default.render_mode;
            }
            if (!string.IsNullOrEmpty(videofolder))
            {
                return Properties.Settings.Default.video_folder;
            }
            if (!string.IsNullOrEmpty(videolink))
            {
                return Properties.Settings.Default.video_link;
            }
            if (videoloopmaxduration != -1)
            {
                return Properties.Settings.Default.video_loop_max_duration;
            }
            if (!string.IsNullOrEmpty(taskbarstyle))
            {
                return Properties.Settings.Default.taskbar_style;
            }

            //if nothing is entered, return the whole thing instead
            return Properties.Settings.Default;
        }
    }
}
