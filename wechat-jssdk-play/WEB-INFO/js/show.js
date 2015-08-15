var app = angular.module('app', ['ngPhotoSwipe', 'chieffancypants.loadingBar', 'toaster', 'appConfig']);

app.controller('imageController', ['$scope', '$http', '$window', 'config', 'cfpLoadingBar', 'toaster',
    function ($scope, $http, $window, cfg, cfpLoadingBar, toaster) {
        $scope.images = [];
        var cfg = $window.config;
        if (!cfg.compareDateWithToday(cfg.showStartDate))
            $window.location.href = "warning.html";

        //-----------------------------
        var screenSize = function (width, height) {
            var x = width ? width : $window.innerWidth;
            var y = height ? height : $window.innerHeight;

            return x + 'x' + y;
        };
        $scope.datasource = [];
        $scope.pageSize = 2; //count of images for each screen page
        $scope.currentPageIndex = 1; //start from page 1
        $scope.totalItemsCount = 0; //total count of images 
        $scope.startIndex = 0; //the start index of the image source

        //get the next page
        $scope.next = function () {

            if ($scope.currentPageIndex * $scope.pageSize < $scope.totalItemsCount) {
                $scope.startIndex = $scope.currentPageIndex * $scope.pageSize;
                $scope.currentPageIndex += 1;
            } else {
                $scope.currentPageIndex = 1;
                $scope.startIndex = 0;
            }
            setImageItems(false);
        }

        //get the previous page
        $scope.pervious = function () {
            if ($scope.currentPageIndex == 1) {
                $scope.currentPageIndex = Math.round($scope.totalItemsCount / $scope.pageSize);
                $scope.startIndex = ($scope.currentPageIndex - 1) * $scope.pageSize;
            } else {

                $scope.startIndex = ($scope.currentPageIndex - 2) * $scope.pageSize;
                $scope.currentPageIndex -= 1;
            }
            setImageItems(false);
        }

        function setImageItems(isInit) {
            $scope.images = [];
            var image;

            if (isInit) {
                $scope.startIndex = 0;
            }

            for (var i = $scope.startIndex; i < $scope.startIndex + $scope.pageSize; i++) {
                if (i < $scope.totalItemsCount) {
                    image = {
                        src: $scope.datasource[i].src,
                        safeSrc: $scope.datasource[i].safeSrc,
                        thumb: $scope.datasource[i].thumb,
                        caption: $scope.datasource[i].caption,
                        size: $scope.datasource[i].size,
                        imageId: $scope.datasource[i].imageId,
                        needVote: $scope.datasource[i].needVote,
                        voteCount: $scope.datasource[i].voteCount,
                        type: 'image'
                    }
                    $scope.images.push(image);
                }
            }
        }

        $scope.init = function () {
            $scope.currentPageIndex = 1;
            setImageItems(true);
        }

        //----------------------------


        $scope.getImages = function () {
            $scope.startProgressBar();
            var serverImageFolder = cfg.baseUrl + 'UploadedFiles/show/images';
            var postUrl = cfg.serverApiUrl + 'Image/GetAllGreatImages';
            $http.get(postUrl).success(function (data, status, headers, config) {
                $scope.completeProgressBar();
                data.forEach(function (item) {
                    $scope.datasource.push({
                        src: serverImageFolder + '/' + item.FileName,
                        safeSrc: serverImageFolder + '/' + item.FileName,
                        thumb: serverImageFolder + '/' + item.FileName,
                        caption: item.Description,
                        imageId: item.ImageId,
                        needVote: false,
                        voteCount: 0,
                        size: screenSize(item.Width, item.Height),
                        type: 'image'
                    });
                });
                $scope.totalItemsCount = $scope.datasource.length;
                $scope.init();
                
            }).error(function (data, status, headers, config) {
                $scope.completeProgressBar();
                toaster.error("温馨提示", "服务器故障");
            });

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