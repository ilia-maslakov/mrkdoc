﻿$(document).ready(function () {
    /*
    $(".custom-file-input").on("change", function () {
        var files = $(this)[0].files;
        $("#fbody").empty();
        $(".custom-file-input");
        for (var i = 0; i < files.length; i++) {
            var f = files[i];
            $("#ftable").append("<tr><td>" + f.name + "</td><td>" + f.size + "</td><td>" + f.type + "</td>");
        }

    });
    */
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
