var gulp = require('gulp');
var gutil = require('gulp-util');
var less = require('gulp-less');
var cleancss = require('gulp-clean-css');
var csscomb = require('gulp-csscomb');
var concat = require('gulp-concat');
var rename = require('gulp-rename');
var uglify = require('gulp-uglify');
var through = require('through2');
var LessPluginAutoPrefix = require('less-plugin-autoprefix');

var autoprefix = new LessPluginAutoPrefix({ browsers: ["last 4 versions"] });

var paths = {
    webroot: "./wwwroot/",
    assetsDir: "./wwwroot/dist"
};

function PreserveLicense() { }
PreserveLicense.prototype = {
    save: function () {
        var self = this;
        return function (file, enc, cb) {
            if (file.isNull()) {
                return cb(null, file);
            }

            if (file.isStream()) {
                return cb(createError(file, 'Streaming not supported', null));
            }

            self.licenses = [];
            String(file.contents).replace(/\s*(\/\*\!.+?\*\/)\s*/g, function (z, license) {
                self.licenses.push(license);
            });

            cb(null, file);
        }
    },

    restore: function () {
        var self = this;
        return function (file, enc, cb) {
            if (file.isNull()) {
                return cb(null, file);
            }

            if (file.isStream()) {
                return cb(createError(file, 'Streaming not supported', null));
            }

            if (self.licenses.length > 0) {
                self.licenses.push('');
            }

            file.contents = new Buffer(self.licenses.join('\n') + String(file.contents));

            cb(null, file);
        }
    }
};

gulp.task('watch', function () {
    gulp.watch('./ClientApp/**/*.less', ['build', 'minify']);
});

gulp.task('build', function () {
    gulp.src('./wwwroot/common/less/*.less')
        .pipe(less({
            plugins: [autoprefix]
        }))
        .pipe(csscomb())
        .pipe(gulp.dest('./wwwroot/common/css'));
});

gulp.task('minify', function () {
    var cssLicense = new PreserveLicense();
    gulp.src([
        './wwwroot/common/css/*.css',
        './wwwroot/3rdparty/css/sh/*.css',
        './wwwroot//lib/ladda/dist/ladda-themeless.min.css',
        './wwwroot/lib/toastr/toastr.css'
    ])
        .pipe(concat('oneblog.css'))
        .pipe(through.obj(cssLicense.save()))
        .pipe(cleancss())
        .pipe(through.obj(cssLicense.restore()))
        .pipe(rename({
            suffix: '.min'
        }))
        .pipe(gulp.dest(paths.assetsDir));

    var jsLicense = new PreserveLicense();
    var mangleProperties = {
        regex: /^m_/
    };
    var uglify_pipe = uglify({
        mangle: {
            properties: mangleProperties
        },
        // mangleProperties: mangleProperties
    }).on('error', function (err) {
        gutil.log(gutil.colors.red('[Error]'), err.toString());
        this.emit('end');
    });
    gulp.src([
        './wwwroot/common/js/lib/*.js',
        './wwwroot/common/js/init.js',
        './wwwroot/common/js/app/*.js',
        './wwwroot/3rdparty/js/sh/shCore.js',
        './wwwroot/3rdparty/js/sh/shBrushCSharp.js',
        './wwwroot/3rdparty/js/sh/shBrushJScript.js',
        './wwwroot/3rdparty/js/sh/shBrushXml.js',
        './wwwroot/3rdparty/js/sh/shBrushCss.js',
        './wwwroot/3rdparty/js/sh/shBrushSass.js',
        './wwwroot/lib/toastr/toastr.min.js',
        './wwwroot/lib/ladda/dist/*.js',
        './wwwroot/3rdparty/js/syntaxhighlight.js'
    ])
        .pipe(concat('oneblog.js'))
        .pipe(through.obj(jsLicense.save()))
        .pipe(uglify_pipe)
        .pipe(through.obj(jsLicense.restore()))
        .pipe(rename({
            suffix: '.min'
        }))
        .pipe(gulp.dest(paths.assetsDir));
});

gulp.task('default', ['build', 'minify']);
