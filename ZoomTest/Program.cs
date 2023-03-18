using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace ZoomTest
{
    internal class Program
    {
        static ZoomAuth auth = new ZoomAuth() { access_token = "x", refresh_token = "x"};
        static string basicToken = "x";
        static string meetingId = "00000000000";
        static void Main(string[] args)
        {            
            ZoomRecordingFile recordingFile = null;

            try
            {
                recordingFile = GetZoomRecording(meetingId);

            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("Unauthorized"))
                {
                    auth = GetZoomAuth();

                    if (auth == null) return;
                }
                recordingFile = GetZoomRecording(meetingId);
            }

            if (recordingFile == null) return;

            string filename = string.Format("{0}.{1}", recordingFile.id, recordingFile.file_extension.ToLower());
            DownloadZoomRecordingFile(recordingFile.download_url, filename);
            DeleteZoomRecordingFile();
        }

        static ZoomRecordingFile GetZoomRecording(string meetingId)
        {
            try
            {
                if (!RemoveZoomRecordingPassword()) return null;
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("Unauthorized"))
                {
                    auth = GetZoomAuth();
                    if (auth == null) return null;

                    if (!RemoveZoomRecordingPassword()) return null;
                }
            }

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.access_token);

            HttpResponseMessage response = client.GetAsync("https://api.zoom.us/v2/users/me/recordings").Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseBody = response.Content.ReadAsStringAsync().Result;
                var ret = Newtonsoft.Json.JsonConvert.DeserializeObject<ZoomRecording>(responseBody);

                if (ret.meetings.Count == 0) return null;

                var meeting = ret.meetings.FirstOrDefault();

                if (meeting.recording_files == null) return null;

                var recordingFile = meeting.recording_files.FirstOrDefault();
                return recordingFile;
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new Exception("Unauthorized");
            else return null;
        }

        static ZoomAuth GetZoomAuth()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", basicToken);

            var parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("grant_type", "refresh_token"));
            parameters.Add(new KeyValuePair<string, string>("refresh_token", auth.refresh_token));

            var response = client.PostAsync("https://zoom.us/oauth/token", new FormUrlEncodedContent(parameters)).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseBody = response.Content.ReadAsStringAsync().Result;
                var ret = Newtonsoft.Json.JsonConvert.DeserializeObject<ZoomAuth>(responseBody);

                return ret;
            }

            return null;
        }

        static bool RemoveZoomRecordingPassword()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.access_token);

            var data = new { password = "" };

            var stringContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            string url = string.Format("https://api.zoom.us/v2/meetings/{0}/recordings/settings", meetingId);
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
            {
                Content = stringContent
            };
            var response = client.SendAsync(request).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new Exception("Unauthorized");

            return response.StatusCode == HttpStatusCode.NoContent;
        }

        static bool DownloadZoomRecordingFile(string url, string filename)
        {
            HttpClient client = new HttpClient();
            using (HttpResponseMessage response = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).Result)
            {
                if (response.StatusCode != HttpStatusCode.OK) return false;

                using (Stream contentStream = response.Content.ReadAsStreamAsync().Result,
                    fileStream = new MemoryStream())
                {
                    var totalRead = 0L;
                    var totalReads = 0L;
                    var buffer = new byte[4096];
                    var isMoreToRead = true;
                    do
                    {
                        var read = contentStream.ReadAsync(buffer, 0, buffer.Length).Result;
                        if (read == 0)
                            isMoreToRead = false;
                        else
                        {
                            fileStream.Write(buffer, 0, read);

                            totalRead += read;
                            totalReads += 1;
                        }
                    }
                    while (isMoreToRead);

                    if (File.Exists(filename)) File.Delete(filename);

                    FileStream fs = new FileStream(filename, FileMode.Create);
                    fileStream.Position= 0;
                    fileStream.CopyTo(fs);
                    fs.Flush();
                    fs.Close();
                }

                return true;
            }
        }

        static bool DeleteZoomRecordingFile()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.access_token);

            var response = client.DeleteAsync(string.Format("https://api.zoom.us/v2/meetings/{0}/recordings??action=trash", meetingId)).Result;

            return response.StatusCode == HttpStatusCode.OK;
        }
    }
}
