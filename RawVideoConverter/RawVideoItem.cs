using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawVideoConverter
{
    class RawVideoItem
    {

        //raw video path
        public string path { set; get; }

        //The channel that the raw video belongs to
        public string channelName { set; get; }

        //raw video modification date
        public  DateTime video_Date { set; get; }

        //Constructor
        public RawVideoItem(string argPath, string argChannelName, DateTime argVideo_Date)
        {
            path = argPath;
            channelName = argChannelName;
            video_Date = argVideo_Date;

        }

    }
}
