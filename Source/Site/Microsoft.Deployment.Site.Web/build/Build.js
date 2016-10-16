require("babel-polyfill");

var gulp = require('gulp');

var bundler = require('aurelia-bundler');
var path = require('path');

var babel = require('gulp-babel');
var sourcemaps = require('gulp-sourcemaps');
var typescript = require('gulp-typescript');

gulp.task('build-TS', function () {
    return gulp.src(['wwwroot/**/*.ts', 'typings/**.*'])
        .pipe(sourcemaps.init({ loadMaps: true }))
        .pipe(typescript({
            sourceMap: true,
            target: "es6",
            module: "commonjs",
            declaration: false,
            noImplicitAny: false,
            removeComments: true,
            emitDecoratorMetadata: true,
            experimentalDecorators: true,
            noExternalResolve: false
        }))
        .pipe(babel({
            plugins: ['transform-runtime'],
            presets: ['es2015']
        }
        ))
        .pipe(sourcemaps.write('.', { includeContent: false, sourceRoot: '/' }))
        .pipe(gulp.dest('wwwroot/'));
});

gulp.task('Build', ['build-TS'], function () { });