var app = angular.module('app', ['ngPhotoSwipe', 'chieffancypants.loadingBar', 'toaster', 'appConfig']);

app.controller('imageController', ['$scope', '$http', '$window', 'config', 'toaster', 'cfpLoadingBar',
    function ($scope, $http, $window, cfg, toaster, cfpLoadingBar) {
        $scope.images = [];
        var cfg = $window.config;
        if (!cfg.compareDateWithToday(cfg.voteStartDate))
            $window.location.href = "warning.html";
        $scope.vote = function (img) {
            if (cfg.openId === undefined) {
                toaster.error("温馨提示", "请到微信>设置>通用>清理微信存储空间后再进入页面");
                return;
            }

            var voteUrl = cfg.serverApiUrl + 'Image/Vote';
            var voteData = {
                openId: cfg.openId,
                nickName:cfg.nickName,
                imageId: img
            };
            $scope.startProgressBar();
            $http.post(voteUrl, voteData).
              success(function (data, status, headers, config) {

                  $scope.images.forEach(function (item) {
                      if (item.imageId === img)
                          item.voteCount += 1;
                  });
                  toaster.success({ title: "投票成功", body: "谢谢你的参与" });
                  $scope.completeProgressBar();

              }).
              error(function (data, status, headers, config) {
                  $scope.completeProgressBar();
                  if (data.ExceptionMessage === 'Error-02')
                      toaster.error("温馨提示", "亲，每人每天投一票哦");
                  else
                      toaster.error("温馨提示", "服务器故障");
              });
            window.event.stopPropagation();
        };

        //-----------------------------
        var screenSize = function (width, height) {
            var x = width ? width : $window.innerWidth;
            var y = height ? height : $window.innerHeight;

            return x + 'x' + y;
        };
        $scope.datasource = [];
        $scope.pageSize = 5; //count of images for each screen page
        $scope.currentPageIndex = 1; //start from page 1
        $scope.totalItemsCount = 0; //total count of images 
        $scope.startIndex = 0; //the start index of the image source
        $scope.totalPage = 0;

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
            $scope.totalPage = Math.ceil($scope.totalItemsCount / $scope.pageSize);
        }

        $scope.init = function () {
            $scope.currentPageIndex = 1;
            setImageItems(true);
        }

        //----------------------------

        var getImageCount = function (imgId, voteCounts) {
            if (voteCounts.length == 0) return 0;
            var result = 0;
            voteCounts.forEach(function (item) {
                if (item.UploadedImageId === imgId)
                    result = item.Count;
            });
            return result;
        }

        $scope.getImages = function () {
            $scope.startProgressBar();
            var voteCountUrl = cfg.serverApiUrl + 'Image/GetAllVoteData';
            $http.get(voteCountUrl)
                .success(function (voteCounts, status, headers, config) {
                    $scope.completeProgressBar();
                    var serverImageFolder = cfg.baseUrl + '/UploadedFiles/imagesForVote';
                    $http.get(cfg.serverApiUrl + 'Image/GetSeletectedVoteImages').
                      success(function (data, status, headers, config) {
                          data.forEach(function (item) {
                              $scope.datasource.push({
                                  src: serverImageFolder + '/' + item.FileName,
                                  safeSrc: serverImageFolder + '/' + item.FileName,
                                  thumb: serverImageFolder + '/' + item.FileName,
                                  caption: item.Description,
                                  imageId: item.ImageId,
                                  needVote: true,
                                  voteCount: getImageCount(item.ImageId, voteCounts),
                                  size: screenSize(item.Width, item.Height),
                                  type: 'image'
                              });
                          });
                          $scope.totalItemsCount = $scope.datasource.length;
                          $scope.init();
                      }).
                      error(function (data, status, headers, config) {
                          $scope.completeProgressBar();
                          toaster.error("温馨提示", "服务器故障");
                      });

                }).
                error(function (data, status, headers, config) {
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
    if (!cfg.compareDateWithToday(cfg.voteStartDate))
        window.location.href = "warning.html";
})