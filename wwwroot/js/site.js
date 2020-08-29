$(document).ready(function () {
    $("#_Content").blur(function () {
        alert($("#_Content").html);
        $("#Content").val($("#_Content:after").html);

    });
});
