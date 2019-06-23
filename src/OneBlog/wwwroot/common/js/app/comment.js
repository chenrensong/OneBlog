One = {
    comments: {
        postId: null,
        contentBox: $("#Content"),
        captcha: $('#Captcha'),
        replyToId: $("#hiddenReplyTo")
    },
    cancelReply: function () {
        this.replyToComment('');
    },
    addComment: function (e) {
        var content = One.comments.contentBox.val();
        var captcha = One.comments.captcha.val();
        console.log('test');
        if (!captcha) {
            toastr.error("请填写验证码~");
            return false;
        }
        if (!content) {
            toastr.error("请填写内容~");
            return false;
        }

        var l = Ladda.create(document.getElementById('btnSaveAjax'));
        l.start();
        var formData = $(".isso-postbox input,textarea").map(function () {
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
                    toastr.error(error);
                    return;
                }
                var commentId = data.CommentId;
                var commentCount = data.CommentCount;
                var commentList = $("#isso-root");
                // add comment html to the right place
                var id = One.comments.replyToId ? One.comments.replyToId.val() : '';
                if (id) {
                    var parentComment = $('#isso-' + id);
                    var textWrapper = $('#isso-wrapper-' + id, parentComment);
                    textWrapper.html(textWrapper.html() + result);
                } else {
                    commentList.html(result + commentList.html());
                    commentList.css('display', 'block');
                }
                var commentForm = $('#isso-postbox');
                var base = $("#comment");
                var commentlist = $("#isso-root");
                commentForm.insertBefore(commentlist);
                $("body,html").animate({ scrollTop: $('#isso-' + commentId).offset().top }, 1000);
                shake($('#isso-' + commentId), "notice-red", 3);

                if ($('#cancelReply')) {
                    $('#cancelReply').css('display', 'none');
                }
                $("#captchaImg").click();
                // reset form values
                One.comments.contentBox.val("");
                One.comments.replyToId.val("")
                One.comments.captcha.val("");
            },
            error: function (data) {
                l.stop();
                l.remove();
                if (!data) {
                    toastr.error("评论失败,请重试~");
                } else {
                    var error = data.Error;
                    toastr.error(error);
                }
                $("#captchaImg").click();
            }
        });
        return false;
    },
    replyToComment: function (id) {

        init();

        var replyorclose = $('#reply-' + id).html();

        if (replyorclose == '回复') {
            $('#reply-' + id).html('关闭');
            // set hidden value
            One.comments.replyToId.val(id);
            // move comment form into position
            var commentForm = $('.isso-postbox');
            if (!id || id == '' || id == null || id == '00000000-0000-0000-0000-000000000000') {
                // move to after comment list
                var commentlist = $("#isso-root");
                var commentForm = $('.isso-postbox');
                commentForm.insertBefore(commentlist);
            } else {
                // move to nested position
                var parentComment = $('#isso-' + id);
                var textWrapper = $('#isso-wrapper-' + id, parentComment);

                //var replies = $('#replies_' + id);

                //// add if necessary
                //if (replies == null) {
                //    replies = document.createElement('div');
                //    replies.className = 'comment-replies';
                //    replies.id = 'replies_' + id;
                //    textWrapper.append(replies);
                //}
                //replies.css("display", "block");
                //replies.append(commentForm);
                textWrapper.append(commentForm);
            }
        } else {
            $('#reply-' + id).html('回复');
            // move to after comment list
            var commentlist = $("#isso-root");
            var commentForm = $('.isso-postbox');
            commentForm.insertBefore(commentlist);
        }

    }
};

$(document).ready(function () {
    if (One.comments.postId) {
        setTimeout(function () {
            $.ajax('/postcount', {
                method: "POST",
                data: "id=" + One.comments.postId,
                success: function (data) {
                }
            });
        }, 1000);
    }
});


function init() {
    One.comments.contentBox = $("#Content");
    One.comments.captcha = $('#Captcha');
    One.comments.replyToId = $("#hiddenReplyTo");
    One.comments.contentBox.val("");
    One.comments.replyToId.val("")
    One.comments.captcha.val("");
}


function shake(ele, cls, times) {
    var eleclass = ele.attr("class");
    if (eleclass == 'undefined') {
        eleclass = '';
    }

    var i = 0, t = false, o = eleclass + " ", c = "", times = times || 2;
    if (t) return;
    t = setInterval(function () {
        i++;
        c = i % 2 ? o + cls : o;
        ele.attr("class", c);
        if (i == 2 * times) {
            clearInterval(t);
            ele.removeClass(cls);
        }
    }, 200);
};
