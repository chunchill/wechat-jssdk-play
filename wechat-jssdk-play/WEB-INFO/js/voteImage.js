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
                    var serverImageFolder = cfg.baseUrl + '/UploadedFiles/images';
                    $http.get(cfg.serverApiUrl + 'Image/GetAllUploadedImages').
                      success(function (data, status, headers, config) {
                          data.forEach(function (item) {
                              $scope.images.push({
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
    if (!cfg.compareDateWithToday(cfg.voteStartDate))
        window.location.href = "warning.html";
})