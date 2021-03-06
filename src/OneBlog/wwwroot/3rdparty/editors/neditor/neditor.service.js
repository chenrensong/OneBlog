/**
 * 自定义上传接口
 * 由于所有Neditor请求都通过editor对象的getActionUrl方法获取上传接口，可以直接通过复写这个方法实现自定义上传接口
 * @param {String} action 匹配neditor.config.js中配置的xxxActionName
 * @returns 返回自定义的上传接口
 */
UE.Editor.prototype._bkGetActionUrl = UE.Editor.prototype.getActionUrl;
UE.Editor.prototype.getActionUrl = function (action) {
    /* 按config中的xxxActionName返回对应的接口地址 */
    if (action == 'uploadimage' || action == 'uploadscrawl' ||
        action == 'uploadvideo' || action == 'catchimage') {
        return '/ueditor/upload?action=' + action;
    } else {
        return this._bkGetActionUrl.call(this, action);
    }
}

/**
 * 图片上传service
 * @param {Object} context UploadImage对象 图片上传上下文
 * @param {Object} editor  编辑器对象
 * @returns imageUploadService 对象
 */
window.UEDITOR_CONFIG['imageUploadService'] = function (context, editor) {
    return {
        /** 
         * 触发fileQueued事件时执行
         * 当文件被加入队列以后触发，用来设置上传相关的数据 (比如: url和自定义参数)
         * @param {Object} file 当前选择的文件对象
         */
        setUploadData: function (file) {
            return file;
        },
        /**
         * 触发uploadBeforeSend事件时执行
         * 在文件上传之前触发，用来添加附带参数
         * @param {Object} object 当前上传对象
         * @param {Object} data 默认的上传参数，可以扩展此对象来控制上传参数
         * @param {Object} headers 可以扩展此对象来控制上传头部
         * @returns 上传参数对象
         */
        setFormData: function (object, data, headers) {
            return data;
        },
        /**
         * 触发startUpload事件时执行
         * 当开始上传流程时触发，用来设置Uploader配置项
         * @param {Object} uploader
         * @returns uploader
         */
        setUploaderOptions: function (uploader) {
            return uploader;
        },
        /**
         * 触发uploadSuccess事件时执行
         * 当文件上传成功时触发
         * @param {Object} res 上传接口返回的response
         * @returns {Boolean} 上传接口返回的response成功状态条件 (比如: res.code == 200)
         */
        getResponseSuccess: function (res) {
            return res.code == 200;
        },
        /* 指定上传接口返回的response中图片路径的字段，默认为 url */
        imageSrcField: 'url'
    }
};

/**
 * 视频上传service
 * @param {Object} context UploadVideo对象 视频上传上下文
 * @param {Object} editor  编辑器对象
 * @returns videoUploadService 对象
 */
window.UEDITOR_CONFIG['videoUploadService'] = function (context, editor) {
    return {
        /** 
         * 触发fileQueued事件时执行
         * 当文件被加入队列以后触发，用来设置上传相关的数据 (比如: url和自定义参数)
         * @param {Object} file 当前选择的文件对象
         */
        setUploadData: function (file) {
            return file;
        },
        /**
         * 触发uploadBeforeSend事件时执行
         * 在文件上传之前触发，用来添加附带参数
         * @param {Object} object 当前上传对象
         * @param {Object} data 默认的上传参数，可以扩展此对象来控制上传参数
         * @param {Object} headers 可以扩展此对象来控制上传头部
         * @returns 上传参数对象
         */
        setFormData: function (object, data, headers) {
            return data;
        },
        /**
         * 触发startUpload事件时执行
         * 当开始上传流程时触发，用来设置Uploader配置项
         * @param {Object} uploader
         * @returns uploader
         */
        setUploaderOptions: function (uploader) {
            return uploader;
        },
        /**
         * 触发uploadSuccess事件时执行
         * 当文件上传成功时触发
         * @param {Object} res 上传接口返回的response
         * @returns {Boolean} 上传接口返回的response成功状态条件 (比如: res.code == 200)
         */
        getResponseSuccess: function (res) {
            return res.code == 200;
        },
        /* 指定上传接口返回的response中视频路径的字段，默认为 url */
        videoSrcField: 'url'
    }
};

/**
 * 涂鸦上传service
 * @param {Object} context scrawlObj对象
 * @param {Object} editor  编辑器对象
 * @returns scrawlUploadService 对象
 */
window.UEDITOR_CONFIG['scrawlUploadService'] = function (context, editor) {
    return scrawlUploadService = {
        /**
         * 点击涂鸦模态框确认按钮时触发
         * 上传涂鸦图片
         * @param {Object} file 涂鸦canvas生成的图片
         * @param {Object} base64 涂鸦canvas生成的base64
         * @param {Function} success 上传成功回调函数,回传上传成功的response对象
         * @param {Function} fail 上传失败回调函数,回传上传失败的response对象
         */
        /**
         * 上传成功的response对象必须为以下两个属性赋值
         * 
         * 上传接口返回的response成功状态条件 {Boolean} (比如: res.code == 200)
         * res.responseSuccess = res.code == 200;
         * 
         * 指定上传接口返回的response中涂鸦图片路径的字段，默认为 url 
         * res.videoSrcField = 'url';
         */
        uploadScraw: function (file, base64, success, fail) {
        }
    }
}

/**
 * 附件上传service
 * @param {Object} context UploadFile对象 附件上传上下文
 * @param {Object} editor  编辑器对象
 * @returns fileUploadService 对象
 */
window.UEDITOR_CONFIG['fileUploadService'] = function (context, editor) {
    return {
        /** 
         * 触发fileQueued事件时执行
         * 当文件被加入队列以后触发，用来设置上传相关的数据 (比如: url和自定义参数)
         * @param {Object} file 当前选择的文件对象
         */
        setUploadData: function (file) {
            return file;
        },
        /**
         * 触发uploadBeforeSend事件时执行
         * 在文件上传之前触发，用来添加附带参数
         * @param {Object} object 当前上传对象
         * @param {Object} data 默认的上传参数，可以扩展此对象来控制上传参数
         * @param {Object} headers 可以扩展此对象来控制上传头部
         * @returns 上传参数对象
         */
        setFormData: function (object, data, headers) {
            return data;
        },
        /**
         * 触发startUpload事件时执行
         * 当开始上传流程时触发，用来设置Uploader配置项
         * @param {Object} uploader
         * @returns uploader
         */
        setUploaderOptions: function (uploader) {
            return uploader;
        },
        /**
         * 触发uploadSuccess事件时执行
         * 当文件上传成功时触发
         * @param {Object} res 上传接口返回的response
         * @returns {Boolean} 上传接口返回的response成功状态条件 (比如: res.code == 200)
         */
        getResponseSuccess: function (res) {
            return res.code == 200;
        },
        /* 指定上传接口返回的response中附件路径的字段，默认为 url */
        fileSrcField: 'url'
    }
};