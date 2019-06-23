



var getPostCover = function (index) {
    var cover = $("#cover" + index);
    return cover.attr("data-src");
}


var setPostCover = function (index, data) {
    if (!data) {
        return;
    }
    var cover = $(".cover" + index);
    var image_preview = $('.image-preview', cover);
    var js_upload = $('.js-upload-hover', cover);
    var js_icon_preview = $('.js-icon-preview', cover);
    image_preview.attr("style", "background-image: url('" + data + "'); background-size: 100% 100%;");
    js_upload.attr("data-src", data);
    js_upload.attr("data-origin_src", data);
    js_icon_preview.attr("href", data);
}



function initiateVariables() {
    img = new Image();
    originalWidth = 0;
    originalHeight = 0;
};

var $image = $('#image');
var cropBoxData;
var canvasData;
var modalId = 0;
var postId = getFromQueryString('id');

$(document).ready(function () {


    //上传代码
    $("#btnPostFile").click(function () {
        //laddaFile.start();
        var l = Ladda.create(this);
        l.start();
        $image.cropper('getCroppedCanvas');
        $image.cropper('getCroppedCanvas', {
            width: 160,
            height: 90
        });


        $image.cropper('getCroppedCanvas').toBlob(function (blob) {
            var formData = new FormData();

            formData.append('croppedImage', blob);
            formData.append('modalId', modalId);
            formData.append('postId', postId);

            $.ajax('/admin/coverimage', {
                method: "POST",
                data: formData,
                processData: false,
                contentType: false,
                success: function (data) {
                    l.stop();
                    l.remove();
                    $("#modal-image-cropper").modal("hide");

                    if (!data) {
                        return;
                    }

                    var cover = $(".cover" + modalId);
                    var image_preview = $('.image-preview', cover);
                    var js_upload = $('.js-upload-hover', cover);
                    var js_icon_preview = $('.js-icon-preview', cover);
                    image_preview.attr("style", "background-image: url('" + data + "'); background-size: 100% 100%;");
                    js_upload.attr("data-src", data);
                    js_upload.attr("data-origin_src", data);
                    js_icon_preview.attr("href", data);
                },
                error: function () {
                    l.stop();
                    l.remove();
                    toastr.error("图片上传失败,请重试~");
                }
            });
        });
    });


    $("#btnSourceFile").click(function () {
        //laddaFile.start();
        var l = Ladda.create(this);
        l.start();

        var formData = new FormData();
        var blob = $image.attr("src");
        formData.append('croppedImage', blob);
        formData.append('modalId', modalId);
        formData.append('postId', postId);

        $.ajax('/admin/coverimage', {
            method: "POST",
            data: formData,
            processData: false,
            contentType: false,
            success: function (data) {
                l.stop();
                l.remove();
                $("#modal-image-cropper").modal("hide");

                if (!data) {
                    return;
                }

                var cover = $(".cover" + modalId);
                var image_preview = $('.image-preview', cover);
                var js_upload = $('.js-upload-hover', cover);
                var js_icon_preview = $('.js-icon-preview', cover);
                image_preview.attr("style", "background-image: url('" + data + "'); background-size: 100% 100%;");
                js_upload.attr("data-src", data);
                js_upload.attr("data-origin_src", data);
                js_icon_preview.attr("href", data);
            },
            error: function () {
                l.stop();
                l.remove();
                toastr.error("图片上传失败,请重试~");
            }

        });
    });


    $(".upload-icon").on('click', function () {
        var parent = $(this).parent().parent().parent().parent().parent();
        var js_browse = $("js-browse", this);
        modalId = js_browse.attr('data-num');
        $('#btn_file').click();
    });


    $(".js-browse").on('click', function () {
        $('#btn_file').click();
        modalId = $(this).attr('data-num');
    });

    $('#modal-image-cropper').on('shown.bs.modal', function () {
        $image.cropper({
            autoCropArea: 0.5,
            ready: function () {
                $image.cropper('setCanvasData', canvasData);
                $image.cropper('setCropBoxData', cropBoxData);
            }
        });

        $image.cropper({
            strict: false,
            background: false,
            zoomable: false,
            autoCropArea: 0.8,
            aspectRatio: 16 / 9,
            crop: function (e) {
                // Output the result data for cropping image.
                console.log(e.x);
                console.log(e.y);
                console.log(e.width);
                console.log(e.height);
                console.log(e.rotate);
                console.log(e.scaleX);
                console.log(e.scaleY);
            }
        });
    }).on('hidden.bs.modal', function () {
        cropBoxData = $image.cropper('getCropBoxData');
        canvasData = $image.cropper('getCanvasData');
        $image.cropper('destroy');
    });


    $(".delete-icon").on('click', function () {
        var cropper = $(this).parent().parent();
        var image_preview = $('.image-preview', cropper);
        var js_upload = $('.js-upload-hover', cropper);
        var js_icon_preview = $('.js-icon-preview', cropper);
        image_preview.attr("style", "");
        js_upload.attr("data-src", "");
        js_upload.attr("data-origin_src", "");
        js_upload.css('display', 'none');
        js_icon_preview.attr("href", "javascript:void(0)");
    });



    $(".js-fileapi-wrapper").hover(
        function () {
            var js_hover = $('.js-upload-hover', this);
            if (js_hover.attr('data-src')) {
                js_hover.css('display', 'block');
            }
        },
        function () {
            var js_hover = $('.js-upload-hover', this);
            if (js_hover.attr('data-src')) {
                js_hover.css('display', 'none');
            }
        }
    );




    $('#btn_file').on('change', function (e) {
        e.preventDefault();
        console.log('e');
        var $btn_file = $('#btn_file');
        var file = $btn_file.prop('files')[0];
        var imageType = /image.*/;

        // if file is an image file
        if (file.type.match(imageType)) {
            reader = new FileReader()
            reader.onload = function (event) {

                initiateVariables();

                img.onload = function () {
                    // set image size so it will fit in canvas
                    originalWidth = img.width;
                    originalHeight = img.height;
                }

                img.src = event.target.result;

                $image.attr("src", event.target.result);

                $("#modal-image-cropper").modal({ backdrop: 'static', keyboard: false });

            };
            reader.readAsDataURL(file);

        }

        $btn_file.val('');
    });


});


