using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;



namespace RawVideoConverter
{
    class FileIO
    {
        private static byte retryTimesLimit = 3;
        private static int retryTimeInterval = 1000;
        //Read in all the lines in a file with a specific encoding
        //読み込み成功した場合、読み込んだ内容を返す
        //読み込み失敗した場合、nullを返す
        public static string[] readAllLinesOfAFile(string filePath)
        {
            //Retry  when error occurs
            for (int retryTimes = 1; retryTimes <= retryTimesLimit; retryTimes++)
            {
                try
                {
                    return File.ReadAllLines(filePath);
                }
                catch (Exception e)
                {

                    // If it's still within retry times limit
                    if (retryTimes < retryTimesLimit)
                    {
                        //wait a while before starting next retry
                        Thread.Sleep(retryTimeInterval);
                    }

                    //If it has reached the retry limit
                    else
                    {
                        ;

                    }
                }
            }
            //End this function becasue we have reached the limit of retry.
            return null;

        }


        //Write string to the designated file
        //ファイルにデータを出力することが成功した場合、trueを返す。
        //失敗した場合、falseを返す。
        public static bool outputStringsToFile(string outputPath, string outputContent)
        {
            //Retry  when error occurs
            for (int retryTimes = 1; retryTimes <= retryTimesLimit; retryTimes++)
            {
                try
                {

                    File.WriteAllText(outputPath, outputContent);

                    //ファイルにデータを出力することが成功したため、trueを返す。
                    return true;
                }
                catch (Exception e)
                {

                    // If it's still within retry times limit
                    if (retryTimes < retryTimesLimit)
                    {
                        //wait a while before starting next retry
                        Thread.Sleep(retryTimeInterval);
                    }

                    //If it has reached the retry limit
                    else
                    {
                        ;

                    }
                }
            }
            //End this function becasue we have reached the limit of retry.
            return false;
        }
    }
}
