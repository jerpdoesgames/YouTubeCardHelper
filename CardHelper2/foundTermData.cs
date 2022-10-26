using System;
using System.Collections.Generic;
using System.Text;

namespace CardHelper2
{

    class termTimeData
    {
        private TimeSpan m_Time;
        private string m_Context;
        private string m_Path;

        public TimeSpan time { get { return m_Time;  } }
        public string context { get { return m_Context; } }
        public string path { get { return m_Path; } }

        public termTimeData(TimeSpan aTime, string aContext, string aPath)
        {
            m_Time = aTime;
            m_Context = aContext;
            m_Path = aPath;
        }
    }

    class foundTermData
    {
        private string m_Term;
        public int foundCount = 0;
        public List<termTimeData> foundTimes;
        public int foundInOtherLists = 0;
        public int foundInSameListOtherVideo = 0;

        public string term { get { return m_Term; } }

        public foundTermData(string aTerm)
        {
            m_Term = aTerm;
            foundTimes = new List<termTimeData>();
        }
    }
}
