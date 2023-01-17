var json = '';
var webpage_url = '';
function execute() {
    $("button").attr("disabled", "disabled");
    $('#tbResults').text('');
    var args = $('#tbCliArguments').val();
    var srid = localStorage.getItem('srid');
    fetch("https://api0.datasea.org/getyt-dlpjson?url=" + args)
        .then(function (response) {
            return response.json();
        }).then(function (text) {
            console.log(text);
            json = text;
            id = text.id;
            webpage_url = text.webpage_url;

            console.log(text.formats_table);

            var id = '';
            
            var videoUrl = '';

            var htmlToAppend = '';
            var lastResolution = '';
            var lastFileSize = 0;
            var isVideSizeSelected = false;
            // bestaudio -x --audio-format mp3
            htmlToAppend += `<div class="form-check form-check-inline">
  <input class="form-check-input" type="radio" name="inlineRadioOptionsForDownload" checked id="inlineRadioMP3" value="bestaudio -x --audio-format mp3">
  <label class="form-check-label" for="inlineRadioMP3" title="MP3">MP3</label>
</div>`;
            jQuery.each(text.formats, function () {
                if (this.resolution != 'audio only' && this.protocol != "mhtml") {

                    // format_note is not reliable DASH video for every facebook

                    if (lastResolution != this.resolution && this.resolution) {
                        console.log(this.resolution);

                        htmlToAppend += `<div class="form-check form-check-inline">
  <input class="form-check-input" type="radio" name="inlineRadioOptionsForDownload" id="inlineRadio${this.format_id}" value="${this.format_id}+bestaudio">
  <label class="form-check-label" for="inlineRadio${this.format_id}" title="${this.format_note}">${this.resolution}</label>
</div>`;
                        if (!isVideSizeSelected && (this.video_ext == 'mp4' || this.video_ext == 'webm')) {
                            //if (this.filesize > lastFileSize) {
                                videoUrl = this.url;
                            lastFileSize = this.filesize;

                            debugger;
                            if (this.resolution.indexOf(720) != -1) {
                                isVideSizeSelected = true;
                            }
                            //}
                        }
                    }
                    lastResolution = this.resolution;
                }
            });

            $("#divOptions").html(htmlToAppend);

            $("#divDownload").removeClass("d-none");

            fetch('https://vjs.zencdn.net/7.20.3/video.min.js')
                .then(response => response.text())
                .then(script => {
                    const scriptEl = document.createElement('script');
                    scriptEl.textContent = script;
                    document.body.appendChild(scriptEl);

                    insertVideoPlayerInGrid("#divVideoStart", videoUrl);
                    insertVideoPlayerInGrid("#divVideoEnd", videoUrl);

                    $("button").removeAttr("disabled", "disabled");
                });
        });
}


$("#btnCliExecute").click(function () {
    execute();
});
$('#tbCliArguments').keypress(function (event) {
    var keycode = (event.keyCode ? event.keyCode : event.which);
    if (keycode == 13) {
        execute();
    }
});

$("#btnDownload").click(function () {
    var checkedRadion = $("input[type=radio]:checked");
    var val = $(checkedRadion[0]).val();
    console.log(val);
    val = val.replace("+", "%2B");
    webpage_url = webpage_url.replace("?", "%3F");
   
    var ip = $("#ipAddress").val();
    var outputFileName = `/app/wwwroot/output/yt-dlp/${ip}/${Math.floor(Math.random() * 100)}.%(ext)s`;
    var url = `https://api0.datasea.org/exec?cli=yt-dlp&arguments=-f ${val} "${webpage_url}" --external-downloader ffmpeg --external-downloader-args "-ss 00:00:10.00 -to 00:00:20.00" --print after_move:filepath --merge-output-format "webm/mp4" --force-overwrites --no-progress -o ${outputFileName} --restrict-filenames`; //

    fetch(url)
        .then(function (response) {
            return response.text();
        }).then(function (text) {

            text = text.trim();

            var parts = text.split(".");
            var name = parts[0];
            var ext = parts[1];
            
            $("#divVideoStart").removeClass("d-none");
            $("#sourceOfVideo").attr("src", text);

            text = text.replace("/app/wwwroot", "https://api0.datasea.org");

            insertVideoPlayerInGrid("#divResult", text, `video/${ext}`)
            $("#divResult").append(`<a href="${text}" target="_blank" download="myfile.${ext}">Download File - ${json.title}</a>`);
   
            console.log(text);

        });

});

function insertVideoPlayerInGrid(cssSelector, videoUrl, type) {
    var elem = $(cssSelector);
    elem.removeClass("d-none");

    var typeAttr = ``;
    if (type) {
        typeAttr = `type="${type}"`
    }

    var html = `<video
    id="my-video${cssSelector}"
    class="video-js mx-auto d-block vjs-fill"
    controls
    preload="metadata"
    poster="${json.thumbnail}"
  >
    <source src="${videoUrl}" ${typeAttr} />
    <p class="vjs-no-js">
      To view this video please enable JavaScript, and consider upgrading to a
      web browser that
      <a href="https://videojs.com/html5-video-support/" target="_blank"
        >supports HTML5 video</a
      >
    </p>
  </video>`;


    elem.html(html);
}
