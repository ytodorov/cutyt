var id = '';
var webpage_url = '';

function execute() {
    $('#tbResults').text('');
    var args = $('#tbCliArguments').val();
    var srid = localStorage.getItem('srid');
    fetch("https://api0.datasea.org/exec?cli=yt-dlp&arguments=--print%20%22%()j%22%20" + args)
        .then(function (response) {
            return response.json();
        }).then(function (text) {
            id = text.id;
            webpage_url = text.webpage_url;

            //debugger;
            console.log(text.formats_table);

            //$('#tbResults').text(text.formats_table);
            var htmlToAppend = '';
            var lastResolution = '';
            jQuery.each(text.formats, function () {
                //debugger;
                if (this.resolution != 'audio only' && this.protocol != "mhtml") {

                    // format_note is not reliable DASH video for every facebook

                    if (lastResolution != this.resolution) {
                        //debugger;
                        console.log(this.resolution);

                        htmlToAppend += `<div class="form-check form-check-inline">
  <input class="form-check-input" type="radio" name="inlineRadioOptionsForDownload" id="inlineRadio${this.format_id}" value="${this.format_id}+bestaudio">
  <label class="form-check-label" for="inlineRadio${this.format_id}" title="${this.format_note}">${this.resolution}</label>
</div>`;
                    }
                    lastResolution = this.resolution;
                }
            });

            $("#divOptions").html(htmlToAppend);

            $("#divDownload").removeClass("d-none");
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

    var url = `https://api0.datasea.org/exec?cli=yt-dlp&arguments=-f ${val} "${webpage_url}" --print after_move:filepath --merge-output-format "webm/mp4" --force-overwrites --no-progress -o "${new Date().getUTCMilliseconds()}.%(ext)s`; //

    //debugger;
    fetch(url)
        .then(function (response) {
            return response.text();
        }).then(function (text) {

            var parts = text.split(".");
            var name = parts[0];
            var ext = parts[1];
            $("#divVideo").removeClass("d-none");
            $("#sourceOfVideo").attr("src", text);
           

            //debugger;
            text = text.replace("/app/wwwroot", "https://api0.datasea.org");

            console.log(text);
            var html = `<video width="640" height="480" controls muted preload="none">
        <source src="${text}" type="video/${ext}">
        Your browser does not support the video tag.
    </video>`;

            html = `<video
    id="my-video"
    class="video-js"
    controls
    preload="auto"
    width="640"
    height="264"
    
    data-setup="{}"
  >
    <source src="${text}" type="video/${ext}" />
    <p class="vjs-no-js">
      To view this video please enable JavaScript, and consider upgrading to a
      web browser that
      <a href="https://videojs.com/html5-video-support/" target="_blank"
        >supports HTML5 video</a
      >
    </p>
  </video>`;


            $("#divVideo").html(html);
        });

});
