function execute() {
    $('#tbResults').text('');
    var args = $('#tbCliArguments').val();
    var srid = localStorage.getItem('srid');
    fetch("https://api0.datasea.org/exec?cli=yt-dlp&arguments=--print%20%22%()j%22%20" + args)
        .then(function (response) {
            return response.json();
        }).then(function (text) {
            console.log(text.formats_table);

            //$('#tbResults').text(text.formats_table);
            var lastResolution = '';
            jQuery.each(text.formats, function () {
                //debugger;
                if (this.resolution != 'audio only' && this.protocol != "mhtml") {
                    if (lastResolution != this.resolution) {
                        debugger;
                        console.log(this.resolution);                       
                    }
                    lastResolution = this.resolution;
                }
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