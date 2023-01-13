using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Cutyt.Core.ViewModels
{
    public class YtDlpJsonViewModel
    {
        public string id { get; set; }
        public string title { get; set; }
        public List<Format> formats { get; set; }
        public List<Thumbnail> thumbnails { get; set; }
        public string thumbnail { get; set; }
        public string description { get; set; }
        public string uploader { get; set; }
        public string uploader_id { get; set; }
        public string uploader_url { get; set; }
        public string channel_id { get; set; }
        public string channel_url { get; set; }
        public double? duration { get; set; }
        public double? view_count { get; set; }
        public object average_rating { get; set; }
        public double? age_limit { get; set; }
        public string webpage_url { get; set; }
        public List<string> categories { get; set; }
        public List<string> tags { get; set; }
        public bool? playable_in_embed { get; set; }
        public string live_status { get; set; }
        public object release_timestamp { get; set; }
        public List<string> _format_sort_fields { get; set; }
        public AutomaticCaptions automatic_captions { get; set; }
        public Subtitles subtitles { get; set; }
        public object comment_count { get; set; }
        public List<Chapter> chapters { get; set; }
        public double? like_count { get; set; }
        public string channel { get; set; }
        public double? channel_follower_count { get; set; }
        public string upload_date { get; set; }
        public string availability { get; set; }
        public string original_url { get; set; }
        public string webpage_url_basename { get; set; }
        public string webpage_url_domain { get; set; }
        public string extractor { get; set; }
        public string extractor_key { get; set; }
        public object playlist { get; set; }
        public object playlist_index { get; set; }
        public string display_id { get; set; }
        public string fulltitle { get; set; }
        public string duration_string { get; set; }
        public bool? is_live { get; set; }
        public bool? was_live { get; set; }
        public object requested_subtitles { get; set; }
        public object _has_drm { get; set; }
        public List<RequestedFormat> requested_formats { get; set; }
        public string format { get; set; }
        public string format_id { get; set; }
        public string ext { get; set; }
        public string protocol { get; set; }
        public object language { get; set; }
        public string format_note { get; set; }
        public double? filesize_approx { get; set; }
        public double? tbr { get; set; }
        public double? width { get; set; }
        public double? height { get; set; }
        public string resolution { get; set; }

        /// <summary>
        /// Yordan custom
        /// </summary>
        public List<string> resolutions
        {
            get
            {
                var res = formats.Select(s => s.resolution).Distinct().ToList();
                return res;

            }
        }
        public double? fps { get; set; }
        public string dynamic_range { get; set; }
        public string vcodec { get; set; }
        public double? vbr { get; set; }
        public object stretched_ratio { get; set; }
        public double? aspect_ratio { get; set; }
        public string acodec { get; set; }
        public double? abr { get; set; }
        public double? asr { get; set; }
        public double? audio_channels { get; set; }
        public double? epoch { get; set; }
        public string _filename { get; set; }
        public string filename { get; set; }
        public string urls { get; set; }
        public string formats_table { get; set; }
        public string thumbnails_table { get; set; }
        public object subtitles_table { get; set; }
        public object automatic_captions_table { get; set; }
        public double? autonumber { get; set; }
        public double? video_autonumber { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class AutomaticCaptions
    {
    }

    public class Chapter
    {
        public double? start_time { get; set; }
        public double? end_time { get; set; }
        public string title { get; set; }
    }

    public class DownloaderOptions
    {
        public double? http_chunk_size { get; set; }
    }

    public class Format
    {
        public string format_id { get; set; }
        public string format_note { get; set; }
        public string ext { get; set; }
        public string protocol { get; set; }
        public string acodec { get; set; }
        public string vcodec { get; set; }
        public string url { get; set; }
        public double? width { get; set; }
        public double? height { get; set; }
        public double? fps { get; set; }
        public double? rows { get; set; }
        public double? columns { get; set; }
        public List<Fragment> fragments { get; set; }
        public string audio_ext { get; set; }
        public string video_ext { get; set; }
        public string format { get; set; }
        public string resolution { get; set; }
        public double? aspect_ratio { get; set; }
        public HttpHeaders http_headers { get; set; }
        public string manifest_url { get; set; }
        public double? tbr { get; set; }
        public double? asr { get; set; }
        public object language { get; set; }
        public double? filesize { get; set; }
        public string container { get; set; }
        public string dynamic_range { get; set; }
        public string fragment_base_url { get; set; }
        public double? manifest_stream_number { get; set; }
        public double? quality { get; set; }
        public double? abr { get; set; }
        public double? source_preference { get; set; }
        public double? audio_channels { get; set; }
        public bool? has_drm { get; set; }
        public double? language_preference { get; set; }
        public double? preference { get; set; }
        public DownloaderOptions downloader_options { get; set; }
        public double? vbr { get; set; }
        public double? filesize_approx { get; set; }
    }

    public class Fragment
    {
        public string url { get; set; }
        public double? duration { get; set; }
        public string path { get; set; }
    }

    public class HttpHeaders
    {
        [JsonProperty("User-Agent")]
        public string UserAgent { get; set; }
        public string Accept { get; set; }

        [JsonProperty("Accept-Language")]
        public string AcceptLanguage { get; set; }

        [JsonProperty("Sec-Fetch-Mode")]
        public string SecFetchMode { get; set; }
    }

    public class RequestedFormat
    {
        public string format_id { get; set; }
        public string manifest_url { get; set; }
        public string ext { get; set; }
        public double? width { get; set; }
        public double? height { get; set; }
        public double? tbr { get; set; }
        public double? asr { get; set; }
        public double? fps { get; set; }
        public object language { get; set; }
        public string format_note { get; set; }
        public double? filesize { get; set; }
        public string container { get; set; }
        public string vcodec { get; set; }
        public string acodec { get; set; }
        public string dynamic_range { get; set; }
        public string url { get; set; }
        public string fragment_base_url { get; set; }
        public List<Fragment> fragments { get; set; }
        public string protocol { get; set; }
        public double? manifest_stream_number { get; set; }
        public double? quality { get; set; }
        public string video_ext { get; set; }
        public string audio_ext { get; set; }
        public double? vbr { get; set; }
        public string format { get; set; }
        public string resolution { get; set; }
        public double? aspect_ratio { get; set; }
        public double? filesize_approx { get; set; }
        public HttpHeaders http_headers { get; set; }
        public double? source_preference { get; set; }
        public double? audio_channels { get; set; }
        public bool? has_drm { get; set; }
        public double? language_preference { get; set; }
        public object preference { get; set; }
        public double? abr { get; set; }
        public DownloaderOptions downloader_options { get; set; }
    }


    public class Subtitles
    {
    }

    public class Thumbnail
    {
        public string url { get; set; }
        public double? preference { get; set; }
        public string id { get; set; }
        public double? height { get; set; }
        public double? width { get; set; }
        public string resolution { get; set; }
    }


}
