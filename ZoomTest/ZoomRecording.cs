using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoomTest
{    public class ZoomRecording
    {
        public string from { get; set; }
        public string to { get; set; }
        public int page_count { get; set; }
        public int page_size { get; set; }
        public int total_records { get; set; }
        public string next_page_token { get; set; }
        public IList<ZoomMeetings> meetings { get; set; }

    }
}
