using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoomTest
{
    public class ZoomMeetings
    {
        public string uuid { get; set; }
        public string id { get; set; }
        public string account_id { get; set; }
        public string host_id { get; set; }
        public string topic { get; set; }
        public int type { get; set; }
        public DateTime start_time { get; set; }
        public string timezone { get; set; }
        public int duration { get; set; }
        public int total_size { get; set; }
        public int recording_count { get; set; }
        public string share_url { get; set; }
        public IList<ZoomRecordingFile> recording_files { get; set; }

    }
}
