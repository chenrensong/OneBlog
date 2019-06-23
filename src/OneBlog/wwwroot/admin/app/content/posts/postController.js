angular.module('blogAdmin').controller('PostsController', ["$rootScope", "$scope", "$location", "$http", "$filter", "dataService", function ($rootScope, $scope, $location, $http, $filter, dataService) {
    $scope.items = [];
    $scope.filter = ($location.search()).fltr;
    $scope.sortingOrder = 'DateCreated';
    $scope.reverse = true;


    $scope.paginationConf = {
        currentPage: 1, // 当前页数
        totalItems: 0, // 一共多少条数据，和itemsPerPage决定一共会有几页
        itemsPerPage: 15, // 每页几条数据，和totalItems决定一共会有几页
        pagesLength: 1,
        onChange: function () {
            $scope.load($scope.paginationConf.currentPage);
        }
    };

    $scope.load = function (index) {
        var url = '/api/posts';
        spinOn();
        var perPage = 15;
        var p = { take: perPage, skip: perPage * (index - 1) }
        dataService.getItems(url, p)
            .success(function (data) {
                angular.copy(data.Items, $scope.items);
                $scope.paginationConf.currentPage = data.PageIndex;
                $scope.paginationConf.totalItems = data.TotalCount;
                $scope.paginationConf.itemsPerPage = data.PageSize;
                $scope.paginationConf.pagesLength = data.TotalPages;
                gridInit($scope, $filter);
                if ($scope.filter) {
                    $scope.setFilter($scope.filter);
                }
                spinOff();
            })
            .error(function () {
                toastr.error($rootScope.lbl.errorLoadingPosts);
                spinOff();
            });
    };


    $scope.processChecked = function (action, itemsChecked) {
        if (itemsChecked) {
            processChecked("/api/posts/processchecked/", action, $scope, dataService);
        }
    };

    $scope.setFilter = function (filter) {
        if ($scope.filter === 'pub') {
            $scope.gridFilter('IsPublished', true, 'pub');
        }
        if ($scope.filter === 'dft') {
            $scope.gridFilter('IsPublished', false, 'dft');
        }
    }

    $(document).ready(function () {
        bindCommon();
    });

}]);