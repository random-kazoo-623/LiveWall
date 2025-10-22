using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveWall.Scripts
{
    internal class file_utilities
    {
        /// <summary>
        /// Ask the user for a file and then returns a file path, this method uses configs_utilities.Save() but not configs_utilities.Sync()
        /// </summary>
        /// <returns>string file_path</returns>
        public static string get_file()
        {
            //ask the user for the video path and then save it
            string _videolink;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Video files (*.mp4)|*.mp4| GIF files (*.gif)|*.gif| MOV files (*.mov)|*.mov| MKV files (*.mkv)|*.mkv| AVI files (*.avi)|*.avi| PKG files (*.pkg)|*.pkg";
            openFileDialog.CheckFileExists = true;
            openFileDialog.Title = "Choose a live wallpaper";

            try
            {
                DialogResult result = openFileDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    _videolink = openFileDialog.FileName;
                    //save to config
                    configs_utilities.Save(videolink: _videolink);
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
            //just in case
            return "";
        }

        /// <summary>
        /// ask the user for the folder path and returns that folder path, this method uses configs_utilities.Save() but not configs_utilities.Sync()
        /// </summary>
        /// <returns>string folder_path</returns>
        public static string get_folder()
        {
            //get folder and return the path
            string _videofolder;
            MessageBox.Show("Please choose the folder containing the wallpapers.");
            FolderBrowserDialog folder_browser_dialog = new FolderBrowserDialog();
            folder_browser_dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            folder_browser_dialog.Description = "Select the wallpaper video folder:";

            try
            {
                if (folder_browser_dialog.ShowDialog() == DialogResult.OK)
                {
                    _videofolder = folder_browser_dialog.SelectedPath;
                    //save to config
                    configs_utilities.Save(videofolder: _videofolder);
                    return _videofolder;
                }
                else if (folder_browser_dialog.ShowDialog() == DialogResult.Cancel)
                {
                    MessageBox.Show("User cancelled file input, returning...");
                    return "";
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error: could not read file from disk or something went horribly wrong. " + e);
            }
            //just in case
            return "";
        }
    }
}
