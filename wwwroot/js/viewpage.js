$(document).ready(function () {
    $("#view-rendered").ready(function () {
        var el = $("#view-rendered");
        var text = el[0].dataset.searchtext;
        if (text != '') {
            var scrollTop = $('#view-rendered').scrollTop();
            var element = $("#view-rendered :contains('" + text + "')").eq(0);
            var position = element.position();
            var offset = scrollTop + position.top;

            element.css('border-left-width', '6px');
            element.css('border-left-style', 'solid');
            element.css('border-left-color', 'coral');

            $(window).scrollTop(offset);
            
        }
    });
});