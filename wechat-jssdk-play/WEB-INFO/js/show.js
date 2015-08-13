var app = angular.module('app', ['ngPhotoSwipe', 'chieffancypants.loadingBar', 'toaster', 'appConfig']);

app.controller('imageController', ['$scope', '$http', '$window', 'config', 'cfpLoadingBar', 'toaster',
    function ($scope, $http, $window, cfg, cfpLoadingBar, toaster) {
        $scope.images = [];
        var cfg = $window.config;
        if (!cfg.compareDateWithToday(cfg.showStartDate))
            $window.location.href = "warning.html";

        $scope.getImages = function () {
            $scope.startProgressBar();
            var serverImageFolder = cfg.baseUrl + 'UploadedFiles/show/images';
            var postUrl = cfg.serverApiUrl + 'Image/GetAllGreatImages';
            $http.get(postUrl).success(function (data, status, headers, config) {
                data.forEach(function (item) {
                    $scope.images.push({
                        src: serverImageFolder + '/' + item.FileName,
                        safeSrc: serverImageFolder + '/' + item.FileName,
                        thumb: serverImageFolder + '/' + item.FileName,
                        caption: item.Description,
                        needVote: false,
                        size: screenSize(item.Width, item.Height),
                        type: 'image'
                    });
                });
                $scope.completeProgressBar();
            }).error(function (data, status, headers, config) {
                $scope.completeProgressBar();
                toaster.error("温馨提示", "服务器故障");
            });

        };

        var screenSize = function (width, height) {
            var x = width ? width : $window.innerWidth;
            var y = height ? height : $window.innerHeight;

            return x + 'x' + y;
        };

        $scope.startProgressBar = function () {
            cfpLoadingBar.start();
            $scope.inProgress = true;
        };

        $scope.completeProgressBar = function () {
            cfpLoadingBar.complete();
            $scope.inProgress = false;
        }

        $scope.inProgress = true;

        $scope.startProgressBar();

        $scope.getImages();
    }]);


$(function () {
    var cfg = window.config;
    if (!cfg.compareDateWithToday(cfg.showStartDate))
        window.location.href = "warning.html";
})