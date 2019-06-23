angular.module('blogAdmin').controller('ProfileController', ["$rootScope", "$scope", "$filter", "dataService", function ($rootScope, $scope, $filter, dataService) {
    $scope.user = {};
    $scope.noAvatar = "";
    $scope.photo = $scope.noAvatar;
    $scope.UserVars = UserVars;

    $scope.focusInput = false;
    $scope.customFields = [];
    $scope.editItem = {};

    $scope.load = function () {
        spinOn();
        dataService.getItems('/api/users/Profile/' + UserVars.Name)
            .success(function (data) {
                angular.copy(data, $scope.user);
                $scope.loadCustom();
                $scope.setPhoto();
                spinOff();
            })
            .error(function () {
                toastr.error($rootScope.lbl.errorLoadingUser);
                spinOff();
            });
    }

    $scope.save = function () {
        spinOn();
        dataService.updateItem("/api/users/saveprofile", $scope.user)
            .success(function (data) {
                toastr.success($rootScope.lbl.userUpdatedShort);
                if ($scope.customFields && $scope.customFields.length > 0) {
                    $scope.updateCustom();
                }
                $scope.load();
                spinOff();
            })
            .error(function () {
                toastr.error($rootScope.lbl.updateFailed);
                spinOff();
            });
    }

    $scope.removePicture = function () {
        spinOn();
        dataService.updateItem('/api/users/removeavatar/' + UserVars.Name)
            .success(function (data) {
                $scope.user.Profile.PhotoUrl = data + "?t=" + new Date().getTime();
                $scope.load();
                $scope.setPhoto();
            })
            .error(function () { toastr.error($rootScope.lbl.failed); });
    }

    $scope.changePicture = function (files) {
        var fd = new FormData();
        fd.append("file", files[0]);

        dataService.uploadFile("/account/upload/" + UserVars.Name, fd)
            .success(function (data) {
                $scope.user.Profile.PhotoUrl = data + "?t=" + new Date().getTime();
                $scope.setPhoto();
            })
            .error(function () { toastr.error($rootScope.lbl.failed); });
    }

    $scope.setPhoto = function () {
        $scope.photo = $scope.user.Profile.PhotoUrl;
    }

    $scope.load();

    /* Custom fields */

    $scope.showCustom = function () {
        $("#modal-custom").modal();
        $scope.focusInput = true;
    }

    $scope.loadCustom = function () {
        $scope.customFields = [];

        //dataService.getItems('/api/customfields', { filter: 'CustomType == "PROFILE"' })
        //.success(function (data) {
        //    angular.copy(data, $scope.customFields);
        //})
        //.error(function () {
        //    toastr.error($rootScope.lbl.errorLoadingCustomFields);
        //});
    }

    $scope.saveCustom = function () {
        var customField = {
            "CustomType": "PROFILE",
            "Key": $("#txtKey").val(),
            "Value": $("#txtValue").val()
        };
        if (customField.Key === '') {
            toastr.error("Custom key is required");
            return false;
        }
        dataService.addItem("/api/customfields", customField)
            .success(function (data) {
                toastr.success('New item added');
                $scope.load();
                $("#modal-custom").modal('hide');
            })
            .error(function () {
                toastr.error($rootScope.lbl.updateFailed);
                $("#modal-custom").modal('hide');
            });
    }

    $scope.updateCustom = function () {
        dataService.updateItem("/api/customfields", $scope.customFields)
            .success(function (data) {
                spinOff();
            })
            .error(function () {
                toastr.error($rootScope.lbl.updateFailed);
                spinOff();
            });
    }

    $scope.deleteCustom = function (key, objId) {
        var customField = {
            "CustomType": "PROFILE",
            "Key": key,
            "ObjectId": objId
        };
        spinOn();
        dataService.deleteItem("/api/customfields", customField)
            .success(function (data) {
                toastr.success("Item deleted");
                spinOff();
                $scope.load();
            })
            .error(function () {
                toastr.error($rootScope.lbl.couldNotDeleteItem);
                spinOff();
            });
    }
    $(document).ready(function () {
        bindCommon();
    });

}]);