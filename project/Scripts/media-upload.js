
$(document).ready(function () {
    $(document).on("click", "#fileUpload", beginUpload);
    $("#progressBar").progressbar(30);
});

var beginUpload = function () {
    var fileControl = document.getElementById("selectFile");
    if (fileControl.files.length > 0) {
            uploadmp3(fileControl.files);       
    }
}


