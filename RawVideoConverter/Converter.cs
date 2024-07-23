//using NReco.VideoConverter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawVideoConverter
{
    class Converter
    {

        //Constructor
        public Converter(MainWindow argMainWindow,string argMode,string argTargetExt,DateTime argMonth,string argRootDir, string argOutputDir,string argManual_Val, string argAuto_Val)
        {
            mainWindow = argMainWindow;
            mode = argMode;
            targetExt = argTargetExt;
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

        //The target files' extension
        private string targetExt;

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


        //C# package Video converter
        //private static NReco.VideoConverter.FFMpegConverter ffMpeg = new NReco.VideoConverter.FFMpegConverter();



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
            //string[] firstLevelSubDirs = Directory.GetDirectories(rootDir);
            //For each channel folder,
            //foreach (string subDir in firstLevelSubDirs)
            //{
                //Get all .raw videos' path created in the selected month
                //string[] rawVideosInSubDir = Directory.GetFiles(subDir, $"*.{targetExt}");
                


                //Get all .raw videos' path created in the selected month in the current folder               
                //Error occurs when there are 10K videos in the input folder.
                        //string[] videosInSubDir = Directory.GetFiles(rootDir, $"*.{targetExt}");
                        //foreach (string videoPath in videosInSubDir)
                foreach (string videoPath in Directory.EnumerateFiles(rootDir, $"*.{targetExt}", SearchOption.AllDirectories))
                {
                    
                    //Get last write time from file ingo
                    //DateTime video_Date = File.GetLastWriteTime(rawVideo);
                    //string channelName = Path.GetFileName(subDir);

                    //Get Info from video file name
                    (string channelName, DateTime video_Date) = getInfoFromVideoName(videoPath);
                    
                    if (pickedDate.Year== video_Date.Year && pickedDate.Month== video_Date.Month)
                    {
                        //RawVideoItem item = new RawVideoItem(rawVideo, Path.GetFileName(subDir), video_Date);
                        RawVideoItem item = new RawVideoItem(videoPath, channelName, video_Date);
                        rawVideo_List.Add(item);
                    }
                }
            //}




            //For each .raw video, convert it to mp4 and copy it to the corresponding output folder
            for(int i=0; i< rawVideo_List.Count;i++)
            {
                
                await Task.Run(() => convert2MP4(rawVideo_List[i].video_Date, rawVideo_List[i].channelName, rawVideo_List[i].path,outputDir));
                
                

            }
            if(rawVideo_List.Count==0)
            {
                mainWindow.logging($"No raw video detected.");
            }

            //The conversion has finished, so we enable the start button
            mainWindow.EnableStartBtn();

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
            int consumerCount = 2;
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
            
            watcher.Error += OnError;

            watcher.Filter = $"*.{targetExt}";
            //watcher.IncludeSubdirectories = true;
            watcher.IncludeSubdirectories = false;
            watcher.EnableRaisingEvents = true;

            
            




        }
        private void OnError(object sender, ErrorEventArgs e)
        {
            ;
        }

        // LastWrite of a file will be triggered 3 times because there is writing the content, writing the last access time, etc.
        // As a result, we need to use a hashset to record the triggered files to prevent this.
        private static HashSet<string> OnChangedFile_Set = new HashSet<string>();
        // When a newly created .raw video is detected do the following process
        private  void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            //Record the triggered files to prevent handling the OnChange event several times for the same file
            if(OnChangedFile_Set.Contains(e.Name))
            {
                return;
            } 
            OnChangedFile_Set.Add(e.Name);

            //Announce that a newly created.raw video is detected
            string msg = $"{e.Name} is detected in {Path.GetDirectoryName(e.FullPath)}\n";
            mainWindow.logging(msg);

            
            //Accumulate the raw video  to be coverted in the channel
            producerObj.writeData(e.FullPath);


            //Convert it to mp4 and copy it to the corresponding output folder
            //Task.Run(()=> convert2MP4(File.GetLastWriteTime(e.FullPath), Path.GetDirectoryName(e.FullPath), e.FullPath, outputDir));
            
        }



        //Get Info from video file name
        public static (string, DateTime) getInfoFromVideoName(string videoPath)
        {
            string[] videoName_Parts = Path.GetFileName(videoPath).Split("_");
            string channelName = videoName_Parts[0];
            string video_Date_str = videoName_Parts[1];
            DateTime video_Date = DateTime.ParseExact(video_Date_str, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            return (channelName, video_Date);
        }




        //Convert a .raw video to a .mp4 and copy it to the destination folder
        public static void convert2MP4(DateTime video_Date,string channelName,string inputVideoPath,string outputDir)
        {
            try
            {

            
                //Get .raw modified date
                //Create the corresponding output folder for it 
                string outPutVideoDir = $"{outputDir}\\{video_Date.Year}\\{video_Date.Month}\\{channelName}\\{video_Date.Day}";
                Directory.CreateDirectory(outPutVideoDir);

                //For video format conversion
                //The video name after conversion
                //string outputVideoName = $"{channelName}_{video_Date.ToString("yyyy_MM_dd_HH")}.mp4";

                //For copy the video to the new folder structure
                //string ext = Path.GetExtension(inputVideoPath);
                //string outputVideoName = $"{channelName}_{video_Date.ToString("yyyy_MM_dd_HH")}.{ext}";
                string videoName = Path.GetFileName(inputVideoPath);
                string outputVideoName = $"{videoName}";



                //Convert the .raw video to .mp4 and copy it to the output path
                string outputVideoPath = $"{outPutVideoDir}\\{outputVideoName}";

                //Display progress status
                
                //Get short outputVideoDir for display.e.g.,2024/7/Channel15/13
                int startIndex = outputDir.Length+1;
                string short_outputVideoDir = outPutVideoDir.Substring(startIndex);

                string msg = $"#Converting {Path.GetFileName(Path.GetDirectoryName(inputVideoPath))}\\{Path.GetFileName(inputVideoPath)} to {short_outputVideoDir}";
                mainWindow.logging(msg);

                runConversionCommand( inputVideoPath,  outputVideoPath);
                //ffMpeg.ConvertMedia($"C:\\Users\\Public\\input.raw", $"C:\\Users\\Public\\{outPutVideoName}.mp4", "mp4");
                //ffMpeg.ConvertMedia(newOutputDir, $"{outputVideoName}.mp4", "mp4");


            }
            catch (Exception ex)
            {
                mainWindow.logging($"Error occured: \n{ ex.ToString()}");
            }
            finally
            {
                //Remove the handled file name from the hashset
                OnChangedFile_Set.Remove($"{channelName}\\{Path.GetFileName(inputVideoPath)}");
            }
        }


        //Run the ffmpeg exe from C# to convert raw video to mp4
        private static void runConversionCommand(string inputVideoPath, string outputVideoPath)
        {
            //If there is already a corresponding mp4 video in the output folder,
            //skip the process for this video.
            if (File.Exists(outputVideoPath))
            {
                //Display progress status
                string msg = $"#Result:  Skipped\n Already exists in the output folder.\n Ready to process next video...\n";
                mainWindow.logging(msg);
                return;
            }

            //For real execution
            //Run the ffmpeg exe to do the conversion
            //ffmpeg - f rawvideo - pix_fmt yuv420p - s:v 1920x1080 - r 25 - i input.raw - c:v libx264 output.mp4
            //string command = $"ffmpeg -f rawvideo -pix_fmt yuv420p -s:v 1920x1080 -r 25 -i \"{inputVideoPath}\" -c:v libx264 \"{outputVideoPath}\"";

            //For copy .raw to the new folder structure
            string command = $"copy  \"{inputVideoPath}\"  \"{outputVideoPath}\"";
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

                //Display progress status
                string msg = $"#Result:  Completed\n Ready to process next video...\n";
                mainWindow.logging(msg);
            }
        }

    }
}
