$(document).ready(function () {

    $(".custom-file-input").on("change", function () {
        var files = $(this)[0].files;
        $("#fbody").empty();
        $(".custom-file-input");
        for (var i = 0; i < files.length; i++) {
            var f = files[i];
            $("#ftable").append("<tr><td>" + f.name + "</td><td>" + f.size + "</td><td>" + f.type + "</td>");
        }
    });
});
