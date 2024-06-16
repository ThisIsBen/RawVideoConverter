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

        //The folder that contains the raw video
        public string parentDirName { set; get; }

        //raw video modification date
        public  DateTime modification_Date { set; get; }

        //Constructor
        public RawVideoItem(string argPath, string argParentDirName, DateTime argModification_Date)
        {
            path = argPath;
            parentDirName = argParentDirName;
            modification_Date = argModification_Date;

        }

    }
}
