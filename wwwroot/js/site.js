$(document).ready(function () {

    $(".custom-file-input").on("change", function() {
        var fileName = $(this).val().split("\\").pop();
        $(this).siblings(".custom-file-label").addClass("selected").html(fileName);
        console.log(fileName);
    });

    $("#Content").scroll(function () {
        $('#renderedMD').scrollTop(this.scrollTop / this.scrollHeight * $('#renderedMD').prop('scrollHeight')); 
    });
    
    $(".filelist").click(function() {
        insertAtCaret($("#Content").get(0), this.dataset.filename);
    });

    $("#renderedMD img").click(function() {
        console.log("#renderedMD img");

        if (this.style.width == "100%") {
            console.log("zoom = 1  #renderedMD img");
            this.style.width = "100px";
        } else {
            console.log("zoom = 0  #renderedMD img");
            this.style.width = "100%";
            this.style.height = "100%";
        }
    });

    $("#view-rendered").ready(function() {
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

    function insertAtCaret(element, text) {
        console.log("insertAtCaret");
        if (document.selection) {  
            console.log("document.selection");
            element.focus();  
            var sel = document.selection.createRange();  
            sel.text = text;  
            element.focus();  
        } else if (element.selectionStart || element.selectionStart === 0) {  
            console.log("element.selectionStart " + element.selectionStart );
            console.log("element.selectionEnd " + element.selectionEnd);
            
            var startPos = element.selectionStart;  
            var endPos = element.selectionEnd;  
            var scrollTop = element.scrollTop;  
            element.value = element.value.substring(0, startPos) + ' ' + text + ' ' +   
                            element.value.substring(endPos, element.value.length);  
            element.focus();  
            element.selectionStart = startPos + text.length;  
            element.selectionEnd = startPos + text.length;  
            element.scrollTop = scrollTop;  
        } else {  
            console.log("else");
            element.value += ' ' + text + ' ';  
            element.focus();  
        }  
    }

    var options = {
        accept: "span.placeholder",       
        drop: function(ev, ui) {
            insertAtCaret($("#Content").get(0), ui.draggable.eq(0).text());
        }
    };

    $("span.placeholder").draggable({
        helper:'clone',
        start: function(event, ui) {
            var txta = $("#Content");
            console.log("span.placeholder.draggable");
            $("div#pseudodroppable").css({
                position:"absolute",
                top:txta.position().top,
                left:txta.position().left,
                width:txta.width(),
                height:txta.height()
            }).droppable(options).show();
        },
        stop: function(event, ui) {
            console.log("stop draggable");
            insertAtCaret($("#Content").get(0), "eeeeeee wqewerwqer weqrwq erqwer");
            $("div#pseudodroppable").droppable('destroy').hide();
        }
    });

});
