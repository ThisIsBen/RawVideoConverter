﻿using NReco.VideoConverter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawVideoConverter
{
    class Converter
    {

        //Constructor
        public Converter(MainWindow argMainWindow,string argMode,DateTime argMonth,string argRootDir, string argOutputDir,string argManual_Val, string argAuto_Val)
        {
            mainWindow = argMainWindow;
            mode = argMode;
            pickedDate = argMonth;
            rootDir = argRootDir;
            outputDir = argOutputDir;
            Manual_Val = argManual_Val;
            Auto_Val = argAuto_Val;

        }


        //MainWindow object to display the progress status on the UI
        private static MainWindow mainWindow;

        //Manual Mode or Auto Mode
        private string mode;

        //Selected month for Manual mode
        private DateTime pickedDate; 

        //Input Dir 
        private string rootDir;
        //Output Dir
        private static string outputDir;

        //Manual mode and Auto mode name
        private string Manual_Val;
        private string Auto_Val;
        //The .raw videos to be converted
        private  List<RawVideoItem> rawVideo_List = new List<RawVideoItem>();


        //Video converter
        private static NReco.VideoConverter.FFMpegConverter ffMpeg = new NReco.VideoConverter.FFMpegConverter();



        //Start converting
        public void start()
        {
            //Manual mode
            if(mode== Manual_Val)
            {

                manualMode();
            }
            //Auto mode
            else if (mode == Auto_Val)
            {
                autoMode();
            }
        }





        //Manual Mode Conversion
        private async void manualMode()
        {


            // Get all first level subdirectories
            string[] firstLevelSubDirs = Directory.GetDirectories(rootDir);
            //For each channel folder,
            foreach (string subDir in firstLevelSubDirs)
            {
                //Get all .raw videos' path created in the selected month
                string[] rawVideosInSubDir = Directory.GetFiles(subDir, "*.raw");
                foreach (string rawVideo in rawVideosInSubDir)
                {
                    //Real version
                    //DateTime modification_Date = File.GetLastWriteTime(rawVideo);
                    //For local test
                    DateTime modification_Date = File.GetCreationTime(rawVideo);
                    if (pickedDate.Year== modification_Date.Year && pickedDate.Month==modification_Date.Month)
                    {
                        RawVideoItem item = new RawVideoItem(rawVideo, Path.GetFileName(subDir),modification_Date);
                        rawVideo_List.Add(item);
                    }
                }
            }




            //For each .raw video, convert it to mp4 and copy it to the corresponding output folder
            for(int i=0; i< rawVideo_List.Count;i++)
            {
                
                await Task.Run(() => convert2MP4(rawVideo_List[i].modification_Date, rawVideo_List[i].parentDirName, rawVideo_List[i].path,outputDir));
                
                

            }
            if(rawVideo_List.Count==0)
            {
                mainWindow.logging($"No raw video detected.");
            }


        }


        //Auto Mode Conversion
        private FileSystemWatcher watcher;
        //Channelの宣言
        private ChannelProvider channelObj;
        //Create a producer
        private ChannelProvider.Producer producerObj;



        private void autoMode()
        {


           
            //Use Channel to accumulate .raw video to be converted
            //Create channel object
            channelObj = new ChannelProvider();
            //Start N consumers
            int consumerCount = 1;
            channelObj.startConsumers(outputDir, consumerCount);
            //Start 1 producer
            producerObj = new ChannelProvider.Producer(channelObj.getWriter());



            //---------------------------------------------------------------------
            //Start file watcher to detect newly created .raw video  
            string msg = "Start detecting newly created .raw...";
            mainWindow.logging(msg);
            watcher = new FileSystemWatcher(rootDir);

            watcher.NotifyFilter = NotifyFilters.LastWrite;

            watcher.Changed += OnChanged;
            
            //watcher.Error += OnError;

            watcher.Filter = "*.raw";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            
            




        }

        //When a newly created .raw video is detected do the following process
        private  void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            //Announce that a newly created.raw video is detected
            string msg = $"{e.Name} is detected in {Path.GetDirectoryName(e.FullPath)}\n";
            mainWindow.logging(msg);

            
            //Accumulate the raw video  to be coverted in the channel
            producerObj.writeData(e.FullPath);


            //Convert it to mp4 and copy it to the corresponding output folder
            //Task.Run(()=> convert2MP4(File.GetLastWriteTime(e.FullPath), Path.GetDirectoryName(e.FullPath), e.FullPath, outputDir));
            
        }


        //Convert a .raw video to a .mp4 and copy it to the destination folder
        public static void convert2MP4(DateTime modification_Date,string parentDirName,string inputVideoPath,string outputDir)
        {
            try
            {

            
                //Get .raw modified date
                //Create the corresponding output folder for it 
                string outPutVideoDir = $"{outputDir}\\{modification_Date.Year}\\{modification_Date.Month}\\{parentDirName}\\{modification_Date.Day}";
                Directory.CreateDirectory(outPutVideoDir);

                //The video name after conversion
                string outPutVideoName = $"{parentDirName}_{modification_Date.ToString("yyyy_MM_dd_HH")}.mp4";

                

                //Convert the .raw video to .mp4 and copy it to the output path
                string outputVideoPath = $"{outPutVideoDir}\\{outPutVideoName}";

                //Display progress status
                string msg = $"Converting {Path.GetFileName(Path.GetDirectoryName(inputVideoPath))}\\{Path.GetFileName(inputVideoPath)}\n to {outPutVideoName}";
                mainWindow.logging(msg);

                runConversionCommand( inputVideoPath,  outputVideoPath);
                //ffMpeg.ConvertMedia($"C:\\Users\\Public\\input.raw", $"C:\\Users\\Public\\{outPutVideoName}.mp4", "mp4");
                //ffMpeg.ConvertMedia(newOutputDir, $"{outPutVideoName}.mp4", "mp4");

                //Display progress status
                msg = $"Complete\n";
                mainWindow.logging(msg);
            }
            catch (Exception ex)
            {
                mainWindow.logging($"Error occured: \n{ ex.ToString()}");
            }
        }


        //Run the ffmpeg exe from C# to convert raw video to mp4
        private static void runConversionCommand(string inputVideoPath, string outputVideoPath)
        {
            //If there is already a corresponding mp4 video in the output folder, delete it.
            if (File.Exists(outputVideoPath))
            {
                File.Delete(outputVideoPath);
            }


            //Run the ffmpeg exe to do the conversion
            //ffmpeg - f rawvideo - pix_fmt yuv420p - s:v 1920x1080 - r 25 - i input.raw - c:v libx264 output.mp4
            string command = $"ffmpeg -f rawvideo -pix_fmt yuv420p -s:v 1920x1080 -r 25 -i \"{inputVideoPath}\" -c:v libx264 \"{outputVideoPath}\"";
            ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + command);

            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;

            // wrap IDisposable into using (in order to release hProcess) 
            using (Process process = new Process())
            {
                process.StartInfo = procStartInfo;
                process.Start();

                // Add this: wait until process does its work
                process.WaitForExit();

                // and only then read the result
                string result = process.StandardOutput.ReadToEnd();
                Console.WriteLine(result);
            }
        }

    }
}
