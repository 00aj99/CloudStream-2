﻿using Plugin.LocalNotifications;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace CloudStreamForms
{


    public partial class App : Application
    {
        public const string baseM3u8Name = @"mirrorlist.m3u8";
        public const string baseSubtitleName = @"subtitles.srt";

        public interface IPlatformDep
        {
            void PlayVlc(string url, string name, string subtitleLoc);
            void PlayVlc(List<string> url, List<string> name, string subtitleLoc);
            void ShowToast(string message, double duration);
            string DownloadFile(string file, string fileName, bool mainPath, string extraPath);
            string DownloadUrl(string url, string fileName, bool mainPath, string extraPath, string toast = "", bool isNotification = false, string body = "");
            bool DeleteFile(string path);
            void DownloadUpdate(string update);
            string GetDownloadPath(string path, string extraFolder);
            StorageInfo GetStorageInformation(string path = "");
            int ConvertDPtoPx(int dp);
            string GetExternalStoragePath();
            void HideStatusBar();
            void ShowStatusBar();
            void UpdateStatusBar();
            void UpdateBackground();
        }

        public class StorageInfo
        {
            public long TotalSpace = 0;
            public long AvailableSpace = 0;
            public long FreeSpace = 0;
            public long UsedSpace { get { return TotalSpace - AvailableSpace; } }
            /// <summary>
            /// From 0-1
            /// </summary>
            public double UsedProcentage { get { return ConvertBytesToGB(UsedSpace, 4) / ConvertBytesToGB(TotalSpace, 4); } }
        }

        public static void OnDownloadProgressChanged(string path, DownloadProgressChangedEventArgs progress)
        {
            // Main.print("PATH: " + path + " | Progress:" + progress.ProgressPercentage);
        }


        public static IPlatformDep platformDep;

        public static void UpdateStatusBar()
        {
            platformDep.UpdateStatusBar();
        }

        public static void UpdateBackground()
        {
            platformDep.UpdateBackground();
        }

        public static void HideStatusBar()
        {
            platformDep.HideStatusBar();
        }
        public static void ShowStatusBar()
        {
            platformDep.ShowStatusBar();
        }

        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        public static int ConvertDPtoPx(int dp)
        {
            return platformDep.ConvertDPtoPx(dp);
        }

        public static StorageInfo GetStorage()
        {
            return platformDep.GetStorageInformation();
        }

        public static double ConvertBytesToGB(long bytes, int digits = 2)
        {
            return ConvertBytesToAny(bytes, digits, 3);
        }

        public static double ConvertBytesToAny(long bytes, int digits = 2, int steps = 3)
        {
            int div = GetSizeOfJumpOnSystem();
            return Math.Round((bytes / Math.Pow(div, steps)), digits);
        }

        public static int GetSizeOfJumpOnSystem()
        {
            return Device.RuntimePlatform == Device.UWP ? 1024 : 1000;
        }

        public static bool DeleteFile(string path)
        {
            return platformDep.DeleteFile(path);
        }
        public static void PlayVLCWithSingleUrl(string url, string name = "", string subtitleLoc = "")
        {
            //PlayVlc?.Invoke(null, url);
            platformDep.PlayVlc(url, name, subtitleLoc);
        }

        public static void ShowToast(string message, double duration = 2.5)
        {
            platformDep.ShowToast(message, duration);
        }

        public static string GetBuildNumber()
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            return v.Major + "." + v.Minor + "." + v.Build;
        }

        public static void DownloadNewGithubUpdate(string update)
        {
            platformDep.DownloadUpdate(update);
        }

        public static string GetDownloadPath(string path, string extraFolder)
        {
            return platformDep.GetDownloadPath(path, extraFolder);
        }

        public static void PlayVLCWithSingleUrl(List<string> url, List<string> name, string subtitleLoc = "")
        {
            //PlayVlc?.Invoke(null, url);
            platformDep.PlayVlc(url, name, subtitleLoc);
        }

        static string GetKeyPath(string folder, string name = "")
        {
            string _s = ":" + folder + "-";
            if (name != "") {
                _s += name + ":";
            }
            return _s;
        }

        public static void SetKey(string folder, string name, object value)
        {
            string path = GetKeyPath(folder, name);
            string _set = ConvertToString(value);
            if (myApp.Properties.ContainsKey(path)) {
                CloudStreamCore.print("CONTAINS KEY");
                myApp.Properties[path] = _set;
            }
            else {
                CloudStreamCore.print("ADD KEY");
                myApp.Properties.Add(path, _set);
            }
        }

        public static T GetKey<T>(string folder, string name, T defVal)
        {
            string path = GetKeyPath(folder, name);
            return GetKey<T>(path, defVal);
        }

        public static void RemoveFolder(string folder)
        {
            List<string> keys = App.GetKeysPath(folder);
            for (int i = 0; i < keys.Count; i++) {
                RemoveKey(keys[i]);
            }
        }

        public static T GetKey<T>(string path, T defVal)
        {
            try {
                if (myApp.Properties.ContainsKey(path)) {
                    CloudStreamCore.print("GETKEY::" + myApp.Properties[path]);
                    CloudStreamCore.print("GETKEY::" + typeof(T).ToString() + "||" + ConvertToObject<T>(myApp.Properties[path] as string, defVal));
                    return (T)ConvertToObject<T>(myApp.Properties[path] as string, defVal);
                }
                else {
                    return defVal;
                }
            }
            catch (Exception) {
                return defVal;
            }

        }

        public static List<T> GetKeys<T>(string folder)
        {
            List<string> keyNames = GetKeysPath(folder);

            List<T> allKeys = new List<T>();
            foreach (var key in keyNames) {
                allKeys.Add((T)myApp.Properties[key]);
            }

            return allKeys;
        }

        public static int GetKeyCount(string folder)
        {
            return GetKeysPath(folder).Count;
        }


        public static List<string> GetKeysPath(string folder)
        {
            List<string> keyNames = myApp.Properties.Keys.Where(t => t.StartsWith(GetKeyPath(folder))).ToList();
            return keyNames;
        }

        public static bool KeyExists(string folder, string name)
        {
            string path = GetKeyPath(folder, name);
            return KeyExists(path);
        }
        public static bool KeyExists(string path)
        {
            return (myApp.Properties.ContainsKey(path));
        }
        public static void RemoveKey(string folder, string name)
        {
            string path = GetKeyPath(folder, name);
            RemoveKey(path);
        }
        public static void RemoveKey(string path)
        {
            if (myApp.Properties.ContainsKey(path)) {
                myApp.Properties.Remove(path);
            }
        }
        static Application myApp { get { return Application.Current; } }

        static public T ConvertToObject<T>(string str, T defValue)
        {
            try {
                return FromByteArray<T>(Convert.FromBase64String(str));

            }
            catch (Exception) {
                return defValue;
            }
        }

        static public T FromByteArray<T>(byte[] rawValue)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(rawValue)) {
                return (T)bf.Deserialize(ms);
            }
        }

        static string ConvertToString(object o)
        {
            return Convert.ToBase64String(ToByteArray(o));
        }

        static byte[] ToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream()) {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }


        public static void ShowNotification(string title, string body)
        {
            CrossLocalNotifications.Current.Show(title, body);
        }

        public static void ShowNotification(string title, string body, int id, int sec)
        {
            CrossLocalNotifications.Current.Show(title, body, id, DateTime.Now.AddSeconds(sec));
        }
        public static void CancelNotifaction(int id)
        {
            CrossLocalNotifications.Current.Cancel(id);
        }

        private static ISettings AppSettings =>
    CrossSettings.Current;
        public static ImageSource GetImageSource(string inp)
        {
            return ImageSource.FromResource("CloudStreamForms.Resource." + inp, Assembly.GetExecutingAssembly());
        }

        public static string DownloadUrl(string url, string fileName, bool mainPath = true, string extraPath = "", string toast = "", bool isNotification = false, string body = "")
        {
            return platformDep.DownloadUrl(url, fileName, mainPath, extraPath, toast, isNotification, body);
        }
        public static string DownloadFile(string file, string fileName, bool mainPath = true, string extraPath = "")
        {
            return platformDep.DownloadFile(file, fileName, mainPath, extraPath);
        }

        public static string ConvertPathAndNameToM3U8(List<string> path, List<string> name, bool isSubtitleEnabled = false, string beforePath = "")
        {
            string _s = "#EXTM3U";
            if (isSubtitleEnabled) {
                _s += "\n";
                _s += "\n";
              //  _s += "#EXT-X-MEDIA:TYPE=SUBTITLES,GROUP-ID=\"subs\",NAME=\"English\",DEFAULT=YES,AUTOSELECT=YES,FORCED=NO,LANGUAGE=\"en\",CHARACTERISTICS=\"public.accessibility.transcribes-spoken-dialog, public.accessibility.describes-music-and-sound\",URI=" + beforePath + baseSubtitleName + "\"";
                _s += "#EXTVLCOPT:sub-file=" + beforePath + baseSubtitleName;
                _s += "\n";
            }
            for (int i = 0; i < path.Count; i++) {
                _s += "\n#EXTINF:" + ", " + name[i].Replace("-", "").Replace("  ", " ") + "\n" + path[i]; //+ (isSubtitleEnabled ? ",SUBTITLES=\"subs\"" : "");
            }
            return _s;
        }

        public static byte[] ConvertPathAndNameToM3U8Bytes(List<string> path, List<string> name, bool isSubtitleEnabled = false, string beforePath = "")
        {
            return Encoding.ASCII.GetBytes(ConvertPathAndNameToM3U8(path, name, isSubtitleEnabled, beforePath));
        }

        public static void OpenBrowser(string url)
        {
            CloudStreamCore.print("Trying to open: " + url);
            if (CloudStreamCore.CheckIfURLIsValid(url)) {
                try {
                    Launcher.OpenAsync(new Uri(url));
                }
                catch (Exception) {
                    CloudStreamCore.print("BROWSER LOADED ERROR, SHOULD NEVER HAPPEND!!");
                }
            }
        }
        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }


}
