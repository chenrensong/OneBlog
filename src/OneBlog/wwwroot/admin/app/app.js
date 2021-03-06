﻿(function () {
    var app = angular.module("blogAdmin", ['ngRoute', 'ngSanitize', 'tm.pagination']);
    var config = ["$routeProvider", function ($routeProvider) {
        app.registerCtrl = $routeProvider.register;
        $routeProvider
        .when("/", { templateUrl: "/admin/app/dashboard/dashboardView.html" })
        .when("/content/posts", { templateUrl: "/admin/app/content/posts/postView.html" })
        .when("/content/blogs", { templateUrl: "/admin/app/content/blogs/blogView.html" })
        .when("/content/comments", { templateUrl: "/admin/app/content/comments/commentView.html" })
        .when("/content/comments/filters", { templateUrl: "/admin/app/content/comments/commentFilters.html" })
        .when("/content/pages", { templateUrl: "/admin/app/content/pages/pageView.html" })
        .when("/content/categories", { templateUrl: "/admin/app/content/categories/categoryView.html" })
        .when("/content/tags", { templateUrl: "/admin/app/content/tags/tagView.html" })
        .when("/custom/plugins", { templateUrl: "/admin/app/custom/plugins/pluginView.html" })
        .when("/custom/plugins/gallery", { templateUrl: "/admin/app/custom/plugins/pluginGallery.html" })
        .when("/custom/themes", { templateUrl: "/admin/app/custom/themes/themeView.html" })
        .when("/custom/themes/gallery", { templateUrl: "/admin/app/custom/themes/themeGallery.html" })
        .when("/custom/widgets", { templateUrl: "/admin/app/custom/widgets/widgetView.html" })
        .when("/custom/widgets/gallery", { templateUrl: "/admin/app/custom/widgets/widgetGallery.html" })

        .when("/security/profile", { templateUrl: "/admin/app/security/profile/profileView.html" })
        .when("/security/roles", { templateUrl: "/admin/app/security/roles/roleView.html" })
        .when("/security/users", { templateUrl: "/admin/app/security/users/userView.html" })

        .when("/settings/basic", { templateUrl: "/admin/app/settings/basicView.html" })
        .when("/settings/feed", { templateUrl: "/admin/app/settings/feedView.html" })
        .when("/settings/email", { templateUrl: "/admin/app/settings/emailView.html" })
        .when("/settings/comments", { templateUrl: "/admin/app/settings/commentView.html" })
        .when("/settings/controls", { templateUrl: "/admin/app/settings/controlView.html" })
        .when("/settings/advanced", { templateUrl: "/admin/app/settings/advancedView.html" })
        .when("/settings/controls/blogroll", { templateUrl: "/admin/app/settings/controls/blogrollView.html" })
        .when("/settings/controls/pings", { templateUrl: "/admin/app/settings/controls/pingView.html" })
        .when("/settings/tools", { templateUrl: "/admin/app/settings/tools/checkView.html" })

        .otherwise({ redirectTo: "/" });

      
    }];
    app.config(config);

   
    app.directive('focusMe', ['$timeout', function ($timeout) {
        return function (scope, element, attrs) {
            scope.$watch(attrs.focusMe, function (value) {
                if (value) {
                    $timeout(function () {
                        element.focus();
                    }, 700);
                }
            });
        };
    }]);


    app.directive('angularTooltip', function () {
        return {
            restrict: 'A',
            replace: false,
            scope: {
                tooltipPlacement: '=?',
                tooltip: '='
            },
            compile: function compile(tElement, tAttributes) {
                return function postLink(scope, element, attributes, controller) {
                    if (scope.tooltip !== '') {
                        element.attr('data-toggle', 'tooltip');
                        element.attr('data-placement', scope.tooltipPlacement || 'bottom');
                        element.attr('title', scope.tooltip);
                        element.tooltip();
                    }

                    scope.$watch('tooltip', function (newVal) {
                        if (!element.attr('data-toggle')) {
                            element.attr('data-toggle', 'tooltip');
                            element.attr('data-placement', scope.tooltipPlacement || 'bottom');
                            element.attr('title', scope.tooltip);
                            element.tooltip();
                        }
                        element.attr('title', '');
                        element.attr('data-original-title', newVal);
                    });
                };
            }
        };
    });

    // dragable
    app.factory('DragDropHandler', [function () {
        return {
            dragObject: undefined,
            addObject: function (object, objects, to) {
                objects.splice(to, 0, object);
            },
            moveObject: function (objects, from, to) {
                objects.splice(to, 0, objects.splice(from, 1)[0]);
            }
        };
    }]);

    //app.factory('ListFactory', [function ($http, $rootScope) {
    //    return {
    //        getList: function (condition) {
    //            return $http.get(baseAddress + "trust/GetResult?" + condition);
    //        },
    //        getListCount: function (condition) {
    //            return $http.get(baseAddress + "trust/GetResultCount?" + condition);
    //        }
    //    };
    //}]);


    app.directive('draggable', ['DragDropHandler', function (DragDropHandler) {
        return {
            scope: {
                draggable: '='
            },
            link: function (scope, element, attrs) {
                element.draggable({
                    connectToSortable: attrs.draggableTarget,
                    helper: "clone",
                    start: function () {
                        DragDropHandler.dragObject = scope.draggable;
                    },
                    stop: function () {
                        DragDropHandler.dragObject = undefined;
                    }
                });

                element.disableSelection();
            }
        };
    }]);

    app.directive('droppable', ['DragDropHandler', function (DragDropHandler) {
        return {
            scope: {
                droppable: '=',
                ngMove: '&',
                ngCreate: '&'
            },
            link: function (scope, element, attrs) {
                element.sortable({
                    connectWith: ['.draggable', '.sortable'],
                });
                element.disableSelection();
                var list = element.attr('Id');
                element.on("sortupdate", function (event, ui) {

                    var from = angular.element(ui.item).scope().$index;
                    var to = element.children().index(ui.item);

                    if (to >= 0) {
                        //item is moved to this list
                        scope.$apply(function () {
                            if (from >= 0) {
                                //item is coming from a sortable

                                if (!ui.sender) {
                                    //item is coming from this sortable
                                    DragDropHandler.moveObject(scope.droppable, from, to);

                                } else {
                                    //item is coming from another sortable
                                    scope.ngMove({
                                        from: from,
                                        to: to,
                                        fromList: ui.sender.attr('Id'),
                                        toList: list
                                    });
                                    ui.item.remove();
                                }
                            } else {
                                //item is coming from a draggable
                                scope.ngCreate({
                                    object: DragDropHandler.dragObject,
                                    to: to,
                                    list: list
                                });

                                ui.item.remove();
                            }
                        });
                    }
                });

            }
        };
    }]);
    // end dragable

    var run = ["$rootScope", "$log", function ($rootScope, $log) {

        $rootScope.lbl = BlogAdmin.i18n;
        $rootScope.SiteVars = SiteVars;
        $rootScope.security = new security();
        toastr.options.positionClass = 'toast-bottom-right';
        toastr.options.backgroundpositionClass = 'toast-bottom-right';
    }];

    app.run(run);

    var security = function () {
        // dashboard
        this.showTabDashboard = showTabDashboard();
        this.viewErrorMessages = viewErrorMessages();
        function showTabDashboard() { return UserVars.Rights.indexOf("ViewDashboard") > -1 ? true : false; }
        function viewErrorMessages() { return UserVars.Rights.indexOf("ViewDetailedErrorMessages") > -1 ? true : false; }

        // blogs
        this.showTabBlogs = showTabBlogs();
        function showTabBlogs() { return (SiteVars.IsPrimary == "True" && UserVars.IsAdmin == "True") ? true : false; }

        // content
        this.showTabContent = showTabContent();
        function showTabContent() { return UserVars.Rights.indexOf("EditOwnPosts") > -1 ? true : false; }

        // customization/packaging
        this.showTabCustom = showTabCustom();
        this.canManageExtensions = canManageExtensions();
        this.canManageThemes = canManageThemes();
        this.canManageWidgets = canManageWidgets();
        this.canManagePackages = canManagePackages();

        function showTabCustom() {
            return (UserVars.Rights.indexOf("ManageExtensions") > -1 ||
                UserVars.Rights.indexOf("ManageWidgets") > -1 ||
                UserVars.Rights.indexOf("ManageThemes") > -1 ||
                UserVars.Rights.indexOf("ManagePackages") > -1) ? true : false;
        }
        function canManageExtensions() { return UserVars.Rights.indexOf("ManageExtensions") > -1; }
        function canManageThemes() { return UserVars.Rights.indexOf("ManageThemes") > -1; }
        function canManageWidgets() { return UserVars.Rights.indexOf("ManageWidgets") > -1; }
        function canManagePackages() { return UserVars.Rights.indexOf("ManagePackages") > -1; }

        // users
        this.showTabUsers = showTabUsers();
        this.canManageUsers = canManageUsers();
        this.canManageRoles = canManageRoles();
        this.canManageProfile = canManageProfile();

        function showTabUsers() { return (UserVars.Rights.indexOf("EditOtherUsers") > -1) ? true : false; }
        function canManageUsers() { return UserVars.Rights.indexOf("EditOtherUsers") > -1 ? true : false; }
        function canManageRoles() { return UserVars.Rights.indexOf("EditRoles") > -1 ? true : false; }
        function canManageProfile() { return UserVars.Rights.indexOf("EditOwnUser") > -1 ? true : false; }

        // settings
        this.showTabSettings = showTabSettings();
        function showTabSettings() { return UserVars.Rights.indexOf("AccessAdminSettingsPages") > -1 ? true : false; }
    }
})();