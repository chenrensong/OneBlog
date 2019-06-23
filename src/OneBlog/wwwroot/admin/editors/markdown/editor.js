(function () {

    'use strict';


    angular.module('blogAdmin')
	.controller('markdowndeepController', ['$scope', function ($scope) {

	}])
	.directive('markdowndeepEditor', ['$timeout', '$sce', function ($timeout, $sce) {

	    return {
	        restrict: 'EA',
	        template: '<div class="{{displayMode}}">' +
						'	<div id="{{id}}-header">' +
						'		<span class="md-editor-controls">' +
						'			<a ng-click="invokeCommand(\'heading\')" title="Change Heading Style (Ctrl+H, or Ctrl+0 to Ctrl+6)" tabindex="-1"><strong>H1</strong></a>' +
						'			<a ng-click="invokeCommand(\'bold\')" title="Bold (Ctrl+B)" tabindex="-1"><strong>B</strong></a>' +
						'			<a ng-click="invokeCommand(\'italic\')" tabindex="-1" class="serif"><em>I</em></a>' +
						'			<a ng-click="invokeCommand(\'code\')" title="Preformatted Code (Ctrl+K or Tab/Shift+Tab on multiline selection)" tabindex="-1">&lt;&gt;</a>' +
						'			<a ng-click="invokeCommand(\'ullist\')" title="Bullets (Ctrl+U)" tabindex="-1"><i class="fi-list-bullet"></i></a>' +
						'			<a ng-click="invokeCommand(\'ollist\')" title="Numbered List (Ctrl+O)" tabindex="-1"><i class="fi-list-number"></i></a>' +
						'			<a ng-click="invokeCommand(\'indent\')" title="Quote (Ctrl+Q)" tabindex="-1"><i class="fi-quote"></i></a>' +
						'			<a ng-click="invokeCommand(\'hr\')" title="Insert Horizontal Rule (Ctrl+R)" tabindex="-1"><i class="fi-minus"></i></a>' +
						'			<a ng-click="invokeCommand(\'link\')" title="Insert Hyperlink (Ctrl+L)" tabindex="-1"><i class="fi-link"></i></a>' +
						'			<a ng-click="invokeCommand(\'img\')" title="Insert Image (Ctrl+G)" tabindex="-1"><i class="fi-photo"></i></a>' +
                        '		    <a ng-click="openCommand(\'admin/filemanager\')" title="Insert File (Ctrl+F)" tabindex="-1"><i class="fi-folder"></i></a>' +

						'			<a ng-show="displayMode == \'display-mode-inline\'" ng-click="setDisplayMode(\'full\')" tabindex="-1" class="right">Preview&nbsp;&nbsp;<i class="fi-arrows-out"></i></a>' +
						'			<a ng-show="displayMode == \'display-mode-full\'" ng-click="setDisplayMode(\'inline\')" tabindex="-1" class="right">Exit Preview&nbsp;&nbsp;<i class="fi-arrows-compress"></i></a>' +
						'		</span>' +
						'	</div>' +
						'	<div>' +
						'		<div>' +
						'			<div>' +
						'				<textarea id="{{id}}-input" ng-keydown="keydown()" ng-model="editorValue" placeholder="Type here..."></textarea>' +
						'			</div>' +
						'			<div ng-show="displayMode == \'display-mode-full\'">' +
						'				<div id="{{id}}-output" ng-bind-html="editorOutput"></div>' +
						'			</div>' +
						'		</div>' +
						'	</div>' +
						'</div>',

	        replace: true,
	        scope: {
	            ngModel: '=',
	            editorValue: '=ngModel',
	            defaultEditorHeight: '@defaultEditorHeight',
	            id: '@id'
	        },

	        require: 'ngModel',
	        link: function (scope, element, attrs, ngModel) {

	            scope.editorOutput = null;
	            scope.editor = null;
	            scope.displayMode = null;
	            scope.timeoutPromise = null;

	            var inputElem = null;

	       

	            scope.setDisplayMode = function (displayMode) {
	                scope.displayMode = 'display-mode-' + displayMode;

	                var elemHeader = angular.element(document.getElementById(scope.id + '-header'));
	                var elemInput = angular.element(document.getElementById(scope.id + '-input'));
	                var elemOutput = angular.element(document.getElementById(scope.id + '-output'));
	                var elemBody = angular.element(document).find('body');

	                if (displayMode === 'full') {
	                    // Set body overflow hidden to remove scroll bars

	                    elemBody.css('overflow', 'hidden');

	                    // Set input and output display heights relative to window size for full screen mode

	                    elemInput.height(angular.element(window).height() - elemHeader.outerHeight());
	                    elemOutput.height(angular.element(window).height() - elemHeader.outerHeight());
	                }
	                if (displayMode === 'inline') {

	                    // Reset window size

	                    elemBody.css('overflow', 'auto');

	                    elemInput.height(scope.defaultEditorHeight + 'px'); // 
	                    elemOutput.height('auto');
	                }
	            }

	            scope.invokeCommand = function (command) {
	                scope.editor.InvokeCommand(command);
	            }

	            scope.openCommand = function (uri) {
	                var url = SiteVars.RelativeWebRoot + uri;
	                //showIframe(url, window.innerWidth * 0.7, window.innerHeight * 0.9);
	                //openDialogByIframe(window.innerWidth, window.innerHeight * 0.9, "ssss", url);

	                TINY.box.show({
	                    iframe: url, boxid: 'frameless', width: 1100, height: 600,
	                    fixed: false, maskid: 'bluemask', maskopacity: 40
	                })
	            }


	            // Set initial display mode

	            scope.setDisplayMode('inline');

	            var markdown = new MarkdownKit.Markdown();

	            scope.keydown = function () {
	                scope.refreshPreview();
	            }

	            // Common refresh logic accessible from ng-keydown
	            // so we can capture keyboard shortcuts

	            scope.refreshPreview = function (value) {
	                if (!value) {
	                    value = inputElem.value;
	                }

	                // Use timeout promise and cancellation
	                // to limit the frequency that the input is parsed

	                if (scope.timeoutPromise != null) {

	                    $timeout.cancel(scope.timeoutPromise);
	                }

	                scope.timeoutPromise = $timeout(function () {

	                    // So that model is updated

	                    scope.editorValue = value; // So that model is updated

	                    // Update preview

	                    scope.editorOutput = $sce.trustAsHtml(markdown.Transform(value));

	                }, 200);
	            }

	            $timeout(function () {

	                inputElem = document.getElementById(element.attr('id') + '-input');

	                console.log('inputElem', inputElem);

	                var elemInput = angular.element(document.getElementById(scope.id + '-input'));
	                elemInput.height(scope.defaultEditorHeight + 'px'); // 

	                scope.editor = new MarkdownDeepEditor.Editor(inputElem, null);

	                markdown.ExtraMode = true;

	                markdown.SafeMode = false;

	                markdown.MarkdownInHtml = true;

	                // So that clicking links doesn't cause user to be
	                // navigated away from page and potentially loose content

	                markdown.NewWindowForExternalLinks = true;

	                // Load watch enables us to transform content once 
	                // loaded by parent controller. Once we have a non null / undefined 
	                // value, we cancel the watch

	                var loadWatch = scope.$watch(function () {

	                    return ngModel.$modelValue;

	                }, function (val) {

	                    if (val) {

	                        scope.editorOutput = $sce.trustAsHtml(markdown.Transform(val));

	                        loadWatch();
	                    }
	                });

	                // Watch the raw element input manually. This enables the markdown to be updated
	                // on markdownDeep control use

	                scope.$watch(function () {

	                    // var elemInput = angular.element(document.getElementById(scope.id + '-input'));

	                    return inputElem.value;

	                }, function (val) {

	                    scope.refreshPreview(val);
	                });

	                // Set up the input and scroll elements to sync. This is intentionally one way.
	                // The output syncs to the manually scrolled input, but not vice versa

	                var inputScrollElem = angular.element(document.getElementById(scope.id + '-input'));
	                var outputScrollElem = angular.element(document.getElementById(scope.id + '-output'));

	                var syncInputOutpuScroll = function (e) {

	                    if (inputScrollElem && outputScrollElem) {

	                        var inputScrollElemScrollTop = inputScrollElem.scrollTop();

	                        var inputScrollElemScrollHeight = inputScrollElem[0].scrollHeight;
	                        var outputScrollElemScrollHeight = outputScrollElem[0].scrollHeight;

	                        if (inputScrollElemScrollTop && inputScrollElemScrollHeight) {

	                            var inputScrollElemScrollPct = inputScrollElemScrollTop / inputScrollElemScrollHeight;

	                            outputScrollElem[0].scrollTop = outputScrollElemScrollHeight * inputScrollElemScrollPct;
	                        }
	                    }
	                }

	                inputScrollElem.on('scroll', syncInputOutpuScroll);
	            });
	        }
	    };

	}]);

})();


$(function () {
    /*  在textarea处插入文本--Start */
    (function ($) {
        $.fn.extend({
            insertContent: function (myValue, t) {
                var $t = $(this)[0];
                if (document.selection) { // ie  
                    this.focus();
                    var sel = document.selection.createRange();
                    sel.text = myValue;
                    this.focus();
                    sel.moveStart('character', -l);
                    var wee = sel.text.length;
                    if (arguments.length == 2) {
                        var l = $t.value.length;
                        sel.moveEnd("character", wee + t);
                        t <= 0 ? sel.moveStart("character", wee - 2 * t
                                - myValue.length) : sel.moveStart(
                                "character", wee - t - myValue.length);
                        sel.select();
                    }
                } else if ($t.selectionStart
                        || $t.selectionStart == '0') {
                    var startPos = $t.selectionStart;
                    var endPos = $t.selectionEnd;
                    var scrollTop = $t.scrollTop;
                    $t.value = $t.value.substring(0, startPos)
                            + myValue
                            + $t.value.substring(endPos,
                                    $t.value.length);
                    this.focus();
                    $t.selectionStart = startPos + myValue.length;
                    $t.selectionEnd = startPos + myValue.length;
                    $t.scrollTop = scrollTop;
                    if (arguments.length == 2) {
                        $t.setSelectionRange(startPos - t,
                                $t.selectionEnd + t);
                        this.focus();
                    }
                } else {
                    this.value += myValue;
                    this.focus();
                }
            }
        })
    })(jQuery);
    /* 在textarea处插入文本--Ending */
});

var editorGetHtml = function () {
    return $("#md-editor-input").val();
}

var editorSetHtml = function (html) {
    $("#md-editor-input").val(html);
}