// global object
BlogEngine = {

    $: function (id) {
        return document.getElementById(id);
    },
    setFlag: function (iso) {
        if (iso.length > 0)
            BlogEngine.comments.flagImage.src = BlogEngineRes.webRoot + "Content/images/blog/flags/" + iso + ".png";
        else
            BlogEngine.comments.flagImage.src = BlogEngineRes.webRoot + "Content/images/blog/pixel.gif";
    }
    ,

    // Shows the preview of the comment
    showCommentPreview: function () {
        var oPreview = this.$('preview');
        var oCompose = this.$('compose');

        if (oPreview) oPreview.className = 'selected';
        if (oCompose) oCompose.className = '';
        this.$('commentCompose').style.display = 'none';
        this.$('commentPreview').style.display = 'block';
        this.$('commentPreview').innerHTML = '<img src="' + BlogEngineRes.webRoot + 'Content/images/blog/ajax-loader.gif" width="24" height="24" alt="Loading" />';
        var argument = this.$('commentPreview').innerHTML;
        this.addComment(true);
        return false;
    }
    ,
    composeComment: function () {
        var oPreview = this.$('preview');
        var oCompose = this.$('compose');

        if (oPreview) oPreview.className = '';
        if (oCompose) oCompose.className = 'selected';
        if (this.$('commentPreview')) {
            this.$('commentPreview').style.display = 'none';
        }
        if (this.$('commentCompose')) {
            this.$('commentCompose').style.display = 'block';
        }
        return false;
    }
    ,
    endShowPreview: function (arg, context) {
        if (BlogEngine.$('commentPreview')) {
            BlogEngine.$('commentPreview').innerHTML = arg;
        }
    }
    ,
    toggleCommentSavingIndicators: function (bSaving) {
        BlogEngine.$("btnSaveAjax").disabled = bSaving;
        //BlogEngine.$("ajaxLoader").style.display = bSaving ? "inline" : "none";
        BlogEngine.$("status").className = "";
        BlogEngine.$("status").innerHTML = "";
        if (!bSaving) {
            if (BlogEngine.$('commentPreview')) {
                BlogEngine.$('commentPreview').innerHTML = "";
            }
            BlogEngine.composeComment();
        }
    }
    ,
    onCommentError: function (error, context) {
        BlogEngine.toggleCommentSavingIndicators(false);
        error = error || "Unknown error occurred.";
        var iDelimiterPos = error.indexOf("|");
        if (iDelimiterPos > 0) {
            error = error.substr(0, iDelimiterPos);
            // Remove numbers from end of error message.
            while (error.length > 0 && error.substr(error.length - 1, 1).match(/\d/)) {
                error = error.substr(0, error.length - 1);
            }
        }

        if (document.getElementById('recaptcha_response_field')) {
            Recaptcha.reload();
        }
        if (document.getElementById("spnSimpleCaptchaIncorrect")) document.getElementById("spnSimpleCaptchaIncorrect").style.display = "none";

        alert("Sorry, the following error occurred while processing your comment:\n\n" + error);
    }
    ,
    addComment: function (preview) {
        var isPreview = preview == true;
        if (!isPreview) {
            BlogEngine.toggleCommentSavingIndicators(true);
        }
        var author = BlogEngine.comments.nameBox.val();
        var email = BlogEngine.comments.emailBox.val();
        var content = BlogEngine.comments.contentBox.val();
        var captcha = BlogEngine.comments.captcha.val();

        var replyToId = BlogEngine.comments.replyToId ? BlogEngine.comments.replyToId.val() : "";
        var l = Ladda.create(document.getElementById('btnSaveAjax'));
        l.start();
        var formData = $("#comment-form input,textarea").map(function () {
            return ($(this).attr("name") + '=' + $(this).val());
        }).get().join("&");

        $.ajax('/comment', {
            method: "POST",
            data: formData,
            success: function (data) {
                l.stop();
                l.remove();
                var result = data.Result;
                var error = data.Error;
                if (error) {
                    BlogEngine.toggleCommentSavingIndicators(false);
                    BlogEngine.$("status").innerHTML = error;
                    BlogEngine.$("status").className = "warning";
                    return;
                }
                var commentId = data.CommentId;
                var commentCount = data.CommentCount;
                var commentList = $("#commentlist");
                // add comment html to the right place
                var id = BlogEngine.comments.replyToId ? BlogEngine.comments.replyToId.val() : '';
                if (id) {
                    var parentComment = BlogEngine.$('id_' + id);
                    var replies = $('#replies_' + id, parentComment);
                    replies.html(replies.html() + result);
                } else {
                    commentList.html(commentList.html() + result);
                    commentList.css('display', 'block');
                }
                var commentForm = $('#comment-form');
                //var base = $("#comment");
                var commentlist = $("#commentlist");
                commentForm.insertBefore(commentlist);
                //$("body,html").animate({ scrollTop: $('#isso-' + commentId).offset().top }, 1000);
                //shake($('#isso-' + commentId), "notice-red", 3);

                if ($('#cancelReply')) {
                    $('#cancelReply').css('display', 'none');
                }
                $("#captchaImg").click();
                // reset form values
                BlogEngine.comments.contentBox.val("");
                BlogEngine.comments.replyToId.val("")
                BlogEngine.comments.captcha.val("");
                BlogEngine.$("status").innerHTML = "Thank you for the feedback.";
                BlogEngine.$("status").className = "success";
                BlogEngine.toggleCommentSavingIndicators(false);
            },
            error: function (data) {
                l.stop();
                l.remove();
                if (!data) {
                    BlogEngine.$("status").innerHTML = "评论失败,请重试~";
                    BlogEngine.$("status").className = "warning";
                } else {
                    var error = data.Error;
                    BlogEngine.$("status").innerHTML = error;
                    BlogEngine.$("status").className = "warning";
                }
                BlogEngine.toggleCommentSavingIndicators(false);
                $("#captchaImg").click();
            }
        });

        if (!isPreview && typeof (OnComment) != "undefined")
            OnComment(author, email, website, country, content);
    }
    ,
    cancelReply: function () {
        this.replyToComment('');
    }
    ,
    replyToComment: function (id) {
        // set hidden .val()
        BlogEngine.comments.replyToId.val(id);
        // move comment form into position
        var commentForm = BlogEngine.$('comment-form');
        if (!id || id == '' || id == null || id == '00000000-0000-0000-0000-000000000000') {
            // move to after comment list
            var base = BlogEngine.$("comment-box");
            if (base.childNodes.length > 0) {
                base.insertBefore(commentForm, base.childNodes[0])
            } else {
                base.appendChild(commentForm);
            }
            // hide cancel button
            BlogEngine.$('cancelReply').style.display = 'none';
        } else {
            // show cancel
            BlogEngine.$('cancelReply').style.display = '';

            // move to nested position
            var parentComment = BlogEngine.$('id_' + id);
            var replies = BlogEngine.$('replies_' + id);

            // add if necessary
            if (replies == null) {
                replies = document.createElement('div');
                replies.className = 'comment-replies';
                replies.id = 'replies_' + id;
                parentComment.appendChild(replies);
            }
            replies.style.display = '';

            if (replies.childNodes.length > 0) {
                replies.insertBefore(commentForm, replies.childNodes[0]);
            } else {
                replies.appendChild(commentForm);
            }
        }
        BlogEngine.comments.nameBox.focus();
    }
    ,
    appendComment: function (args, context) {
        if (context == "comment") {

            if (document.getElementById('recaptcha_response_field')) {
                Recaptcha.reload();
            }
            if (document.getElementById("spnSimpleCaptchaIncorrect")) document.getElementById("spnSimpleCaptchaIncorrect").style.display = "none";

            if (args == "RecaptchaIncorrect" || args == "SimpleCaptchaIncorrect") {
                if (document.getElementById("spnCaptchaIncorrect")) document.getElementById("spnCaptchaIncorrect").style.display = "";
                if (document.getElementById("spnSimpleCaptchaIncorrect")) document.getElementById("spnSimpleCaptchaIncorrect").style.display = "";
                BlogEngine.toggleCommentSavingIndicators(false);
            }
            else {


                if (document.getElementById("spnCaptchaIncorrect")) document.getElementById("spnCaptchaIncorrect").style.display = "none";
                if (document.getElementById("spnSimpleCaptchaIncorrect")) document.getElementById("spnSimpleCaptchaIncorrect").style.display = "none";

                var commentList = BlogEngine.$("commentlist");
                if (commentList.innerHTML.length < 10)
                    commentList.innerHTML = "<h1 id='comment'>Comment</h1>"

                // add comment html to the right place
                var id = BlogEngine.comments.replyToId ? BlogEngine.comments.replyToId.val() : '';

                if (id != '') {
                    var replies = BlogEngine.$('replies_' + id);
                    replies.innerHTML += args;
                } else {
                    commentList.innerHTML += args;
                    commentList.style.display = 'block';
                }

                // reset form values
                BlogEngine.comments.contentBox.val() = "";
                BlogEngine.comments.contentBox = BlogEngine.$(BlogEngine.comments.contentBox.id);
                BlogEngine.toggleCommentSavingIndicators(false);
                BlogEngine.$("status").className = "success";

                if (!BlogEngine.comments.moderation)
                    BlogEngine.$("status").innerHTML = "commentWasSaved";
                else
                    BlogEngine.$("status").innerHTML = "commentWaitingModeration";

                // move form back to bottom
                var commentForm = BlogEngine.$('comment-form');
                commentList.appendChild(commentForm);
                // reset reply to
                if (BlogEngine.comments.replyToId) BlogEngine.comments.replyToId.val() = '';
                if (BlogEngine.$('cancelReply')) BlogEngine.$('cancelReply').style.display = 'none';

            }
        }

        BlogEngine.$("btnSaveAjax").disabled = false;
    }
    ,
    validateAndSubmitCommentForm: function () {

        if (BlogEngine.comments.nameBox.val().length < 1) {
            BlogEngine.$("status").innerHTML = "Name is required.";
            BlogEngine.$("status").className = "warning";
            BlogEngine.$("UserName").focus();
            return false;
        }
        if (BlogEngine.comments.emailBox.val().length < 1) {
            BlogEngine.$("status").innerHTML = "Email is required.";
            BlogEngine.$("status").className = "warning";
            BlogEngine.$("Email").focus();
            return false;
        }
        if (BlogEngine.comments.contentBox.val().length < 1) {
            BlogEngine.$("status").innerHTML = "Comment is required.";
            BlogEngine.$("status").className = "warning";
            BlogEngine.$("Content").focus();
            return false;
        }

        if (BlogEngine.comments.captcha.val().length < 1) {
            BlogEngine.$("status").innerHTML = "Captcha is required.";
            BlogEngine.$("status").className = "warning";
            BlogEngine.$("Captcha").focus();
            return false;
        }

        BlogEngine.addComment();
        return true;
    }
    ,
   

    addBbCode: function (v) {
        try {
            var contentBox = BlogEngine.comments.contentBox;
            if (contentBox.selectionStart) // firefox
            {
                var pretxt = contentBox.val().substring(0, contentBox.selectionStart);
                var therest = contentBox.val().substr(contentBox.selectionEnd);
                var sel = contentBox.val().substring(contentBox.selectionStart, contentBox.selectionEnd);
                contentBox.val() = pretxt + "[" + v + "]" + sel + "[/" + v + "]" + therest;
                contentBox.focus();
            }
            else if (document.selection && document.selection.createRange) // IE
            {
                var str = document.selection.createRange().text;
                contentBox.focus();
                var sel = document.selection.createRange();
                sel.text = "[" + v + "]" + str + "[/" + v + "]";
            }
        }
        catch (ex) { }

        return;
    }
    ,
    // Searches the blog based on the entered text and
    // searches comments as well if chosen.
    search: function (root, searchfield) {
        if (!searchfield) {
            searchfield = 'searchfield';
        }
        var input = this.$(searchfield);
        var check = this.$("searchcomments");

        var search = "search/" + encodeURIComponent(input.value);
        if (check != null && check.checked)
            search += "&comment=true";

        top.location.href = root + search;

        return false;
    }
    ,
    // Clears the search fields on focus.
    searchClear: function (defaultText, searchfield) {
        if (!searchfield) {
            searchfield = 'searchfield';
        }
        var input = this.$(searchfield);
        if (input.val() == defaultText)
            input.val() = "";
        else if (input.val() == "")
            input.val() = defaultText;
    }
    ,
    rate: function (blogId, id, rating) {
        this.createCallback("rating.axd?id=" + id + "&rating=" + rating, BlogEngine.ratingCallback, blogId);
    }
    ,
    ratingCallback: function (response) {
        var rating = response.substring(0, 1);
        var status = response.substring(1);

        if (status == "OK") {
            if (typeof OnRating != "undefined")
                OnRating(rating);

            alert(BlogEngineRes.i18n.ratingHasBeenRegistered);
        }
        else if (status == "HASRATED") {
            alert(BlogEngineRes.i18n.hasRated);
        }
        else {
            alert("An error occured while registering your rating. Please try again");
        }
    }
    ,
    /// <summary>
    /// Creates a client callback back to the requesting page
    /// and calls the callback method with the response as parameter.
    /// </summary>
    createCallback: function (url, callback, blogId) {
        var http = BlogEngine.getHttpObject();
        http.open("GET", url, true);

        if (blogId && http.setRequestHeader) {
            http.setRequestHeader('x-blog-instance', blogId.toString());
        }

        http.onreadystatechange = function () {
            if (http.readyState == 4) {
                if (http.responseText.length > 0 && callback != null)
                    callback(http.responseText);
            }
        };

        http.send(null);
    }
    ,
    /// <summary>
    /// Creates a XmlHttpRequest object.
    /// </summary>
    getHttpObject: function () {
        if (typeof XMLHttpRequest != 'undefined')
            return new XMLHttpRequest();

        try {
            return new ActiveXObject("Msxml2.XMLHTTP");
        }
        catch (e) {
            try {
                return new ActiveXObject("Microsoft.XMLHTTP");
            }
            catch (e) { }
        }

        return false;
    }
    ,
    // Updates the calendar from client-callback
    updateCalendar: function (args, context) {
        var cal = BlogEngine.$('calendarContainer');
        cal.innerHTML = args;
        BlogEngine.Calendar.months[context] = args;
    }
    ,
    toggleMonth: function (year) {
        var monthList = BlogEngine.$("monthList");
        var years = monthList.getElementsByTagName("ul");
        for (i = 0; i < years.length; i++) {
            if (years[i].id == year) {
                var state = years[i].className == "open" ? "" : "open";
                years[i].className = state;
                break;
            }
        }
    }
    ,
    // Adds a trim method to all strings.
    equal: function (first, second) {
        var f = first.toLowerCase().replace(new RegExp(' ', 'gi'), '');
        var s = second.toLowerCase().replace(new RegExp(' ', 'gi'), '');
        return f == s;
    }
    ,
    /*-----------------------------------------------------------------------------
    XFN HIGHLIGHTER
    -----------------------------------------------------------------------------*/
    xfnRelationships: ['friend', 'acquaintance', 'contact', 'met'
        , 'co-worker', 'colleague', 'co-resident'
        , 'neighbor', 'child', 'parent', 'sibling'
        , 'spouse', 'kin', 'muse', 'crush', 'date'
        , 'sweetheart', 'me']
    ,
    // Applies the XFN tags of a link to the title tag
    hightLightXfn: function () {
        var content = BlogEngine.$('content');
        if (content == null)
            return;

        var links = content.getElementsByTagName('a');
        for (i = 0; i < links.length; i++) {
            var link = links[i];
            var rel = link.getAttribute('rel');
            if (rel && rel != "nofollow") {
                for (j = 0; j < BlogEngine.xfnRelationships.length; j++) {
                    if (rel.indexOf(BlogEngine.xfnRelationships[j]) > -1) {
                        link.title = 'XFN relationship: ' + rel;
                        break;
                    }
                }
            }
        }
    }
    ,

    showRating: function (container, id, raters, rating, blogId) {
        var div = document.createElement('div');
        div.className = 'rating';

        var p = document.createElement('p');
        div.appendChild(p);
        if (raters == 0) {
            p.innerHTML = BlogEngineRes.i18n.beTheFirstToRate;
        }
        else {
            p.innerHTML = BlogEngineRes.i18n.currentlyRated.replace('{0}', new Number(rating).toFixed(1)).replace('{1}', raters);
        }

        var ul = document.createElement('ul');
        ul.className = 'star-rating small-star';
        div.appendChild(ul);

        var li = document.createElement('li');
        li.className = 'current-rating';
        li.style.width = Math.round(rating * 20) + '%';
        li.innerHTML = 'Currently ' + rating + '/5 Stars.';
        ul.appendChild(li);

        for (var i = 1; i <= 5; i++) {
            var l = document.createElement('li');
            var a = document.createElement('a');
            a.innerHTML = i;
            a.href = 'rate/' + i;
            a.className = this.englishNumber(i);
            a.title = BlogEngineRes.i18n.rateThisXStars.replace('{0}', i.toString()).replace('{1}', i == 1 ? '' : 's');
            a.rel = "nofollow";
            a.onclick = function () {
                BlogEngine.rate(blogId, id, this.innerHTML);
                return false;
            };

            l.appendChild(a);
            ul.appendChild(l);
        }

        container.innerHTML = '';
        container.appendChild(div);
        container.style.visibility = 'visible';
    }
    ,

    applyRatings: function () {
        var divs = document.getElementsByTagName('div');
        for (var i = 0; i < divs.length; i++) {
            if (divs[i].className == 'ratingcontainer') {
                var args = divs[i].innerHTML.split('|');
                BlogEngine.showRating(divs[i], args[0], args[1], args[2], args[3]);
            }
        }
    },

    englishNumber: function (number) {
        if (number == 1)
            return 'one-star';

        if (number == 2)
            return 'two-stars';

        if (number == 3)
            return 'three-stars';

        if (number == 4)
            return 'four-stars';

        return 'five-stars';
    }
    ,
    // Adds event to window.onload without overwriting currently assigned onload functions.
    // Function found at Simon Willison's weblog - http://simon.incutio.com/
    addLoadEvent: function (func) {
        var oldonload = window.onload;
        if (typeof window.onload != 'function') {
            window.onload = func;
        }
        else {
            window.onload = function () {
                oldonload();
                func();
            }
        }
    }
    ,
    filterByAPML: function () {
        var width = document.documentElement.clientWidth + document.documentElement.scrollLeft;
        var height = document.documentElement.clientHeight + document.documentElement.scrollTop;
        document.body.style.position = 'static';

        var layer = document.createElement('div');
        layer.style.zIndex = 2;
        layer.id = 'layer';
        layer.style.position = 'absolute';
        layer.style.top = '0px';
        layer.style.left = '0px';
        layer.style.height = document.documentElement.scrollHeight + 'px';
        layer.style.width = width + 'px';
        layer.style.backgroundColor = 'black';
        layer.style.opacity = '.6';
        layer.style.filter += ("progid:DXImageTransform.Microsoft.Alpha(opacity=60)");
        document.body.appendChild(layer);

        var div = document.createElement('div');
        div.style.zIndex = 3;
        div.id = 'apmlfilter';
        div.style.position = (navigator.userAgent.indexOf('MSIE 6') > -1) ? 'absolute' : 'fixed';
        div.style.top = '200px';
        div.style.left = (width / 2) - (400 / 2) + 'px';
        div.style.height = '50px';
        div.style.width = '400px';
        div.style.backgroundColor = 'white';
        div.style.border = '2px solid silver';
        div.style.padding = '20px';
        document.body.appendChild(div);

        var p = document.createElement('p');
        p.innerHTML = BlogEngineRes.i18n.apmlDescription;
        p.style.margin = '0px';
        div.appendChild(p);

        var form = document.createElement('form');
        form.method = 'get';
        form.style.display = 'inline';
        form.action = BlogEngineRes.webRoot;
        div.appendChild(form);

        var textbox = document.createElement('input');
        textbox.type = 'text';
        textbox.val() = BlogEngine.getCookieValue('url') || 'http://';
        textbox.style.width = '320px';
        textbox.id = 'txtapml';
        textbox.name = 'apml';
        textbox.style.background = 'url(' + BlogEngineRes.webRoot + 'Content/images/blog/apml.png) no-repeat 2px center';
        textbox.style.paddingLeft = '16px';
        form.appendChild(textbox);
        textbox.focus();

        var button = document.createElement('input');
        button.type = 'submit';
        button.val() = BlogEngineRes.i18n.filter;
        button.onclick = function () { location.href = BlogEngineRes.webRoot + '?apml=' + encodeURIComponent(BlogEngine.$('txtapml').val()); };
        form.appendChild(button);

        var br = document.createElement('br');
        div.appendChild(br);

        var a = document.createElement('a');
        a.innerHTML = BlogEngineRes.i18n.cancel;
        a.href = 'javascript:void(0)';
        a.onclick = function () { document.body.removeChild(BlogEngine.$('layer')); document.body.removeChild(BlogEngine.$('apmlfilter')); document.body.style.position = ''; };
        div.appendChild(a);
    }
    ,
    getCookieValue: function (name) {
        var cookie = new String(document.cookie);

        if (cookie != null && cookie.indexOf('comment=') > -1) {
            var start = cookie.indexOf(name + '=') + name.length + 1;
            var end = cookie.indexOf('&', start);
            if (end > start && start > -1)
                return cookie.substring(start, end);
        }

        return null;
    }
    ,
    init: function () {
        
        BlogEngine.comments.emailBox = $("#Email");
        BlogEngine.comments.nameBox = $("#UserName");
        BlogEngine.comments.contentBox = $("#Content");
        BlogEngine.comments.captcha = $('#Captcha');
        BlogEngine.comments.replyToId = $("#hiddenReplyTo");
        BlogEngine.comments.contentBox.val("");
        BlogEngine.comments.replyToId.val("")
        BlogEngine.comments.captcha.val("");
    },

    test: function () {
        alert('test');
    }
    ,
    comments: {
        flagImage: null,
        contentBox: null,
        moderation: null,
        checkName: null,
        postAuthor: null,
        nameBox: null,
        emailBox: null,
        websiteBox: null,
        countryDropDown: null,
        captchaField: null,
        controlId: null,
        replyToId: null
    }
};

BlogEngine.init();

BlogEngine.addLoadEvent(BlogEngine.hightLightXfn);

// add this to global if it doesn't exist yet
if (typeof ($) == 'undefined')
    window.$ = BlogEngine.$;

// apply ratings after registerVariables.
BlogEngine.addLoadEvent(BlogEngine.applyRatings);


// add placeholder to newsletter widget.
$("#txtNewsletterEmail").attr("placeholder", "youremail@example.com");

$('[data-toggle="tooltip"]').tooltip();

$(".blog-nav ul").each(function () {
    var $this = $(this);
    if ($this.find("li").length) {
        $this.parent().addClass("has-ul").append('<i class="fa fa-chevron-down nav-item-toggle"></i>');
    }
});

$(".blog-nav-toggle").on("click", function () {
    $(this).toggleClass("is-active");
    $(".blog-nav").slideToggle();
});


$(".nav-item-toggle").on("click", function () {
    $(this).toggleClass("is-active");
    $(this).parent().find("ul").toggleClass("is-active");
});

$(function () {
    // social networks
    var socialNetwork = $(".blog-social li a");
    for (i = 0; i < socialNetwork.length; ++i) {
        link = socialNetwork[i];
        if ($(link).attr("href") != "") {
            $(link).parent().css("display", "block");
        }
    }

    $(window).scroll(function () {
        var scrollerToTop = $('.backTop');
        var scrollerTOC = $('.widget-toc');
        document.documentElement.scrollTop + document.body.scrollTop > 200 ?
            scrollerToTop.fadeIn() :
            scrollerToTop.fadeOut();
        document.documentElement.scrollTop + document.body.scrollTop > 250 ?
            scrollerTOC.addClass("widget-toc-fixed") :
            scrollerTOC.removeClass("widget-toc-fixed");
    });

    var toc = $('.toc');
    // toc config
    toc.toc({
        content: ".post-body",
        headings: "h2,h3,h4,h5"
    });

    if (toc.children().length == 0) $(".widget-toc").hide();

    var tocHieght = toc.height();
    var tocFixedHeight = $(window).height() - 192;
    tocHieght > tocFixedHeight ?
        toc.css('height', tocFixedHeight) :
        toc.css('height', tocHieght)

    $(window).resize(function () {
        var tocFixedHeight = $(this).height() - 192;
        tocHieght > tocFixedHeight ?
            toc.css('height', tocFixedHeight) :
            toc.css('height', tocHieght)
    })

    // add archives year
    var yearArray = new Array();
    $(".archives-item").each(function () {
        var archivesYear = $(this).attr("date");
        yearArray.push(archivesYear);
    });
    var uniqueYear = $.unique(yearArray);
    for (var i = 0; i < uniqueYear.length; i++) {
        var html = "<div class='archives-item fadeInDown animated'>" +
            "<div class='archives-year'>" +
            "<h3><time datetime='" + uniqueYear[i] + "'>" + uniqueYear[i] + "</time></h3>" +
            "</div></div>";
        $("[date='" + uniqueYear[i] + "']:first").before(html);
    }
});


// back up
$("#back-up").on('click', function (e) {
    e.preventDefault();
    $('html,body').animate({
        scrollTop: 0
    }, 700);
});

