angular.module("uploadApp", ['chieffancypants.loadingBar', 'ngAnimate', 'toaster', 'appConfig'])
   .config(function (cfpLoadingBarProvider) {
       cfpLoadingBarProvider.includeSpinner = true;
   })
    .controller("UploadController", ['$scope', '$http', '$timeout', 'cfpLoadingBar', 'config', 'toaster','$window',
        function ($scope, $http, $timeout, cfpLoadingBar, cfg, toaster, $window) {
            var cfg = $window.config;
            if (!cfg.compareDateWithToday(cfg.uploadStartDate))
                $window.location.href = "warning.html";
            $scope.fileName = '';
            $scope.description = '';
            $scope.showErrMsg = function () {
                toaster.error("温馨提示","图片大小不超过1M");
            };
            $scope.submit = function () {
                function checkImgType(fileName) {
                    if (fileName == "") {
                        return false;
                    } else {
                        if (!/\.(gif|jpg|jpeg|png|GIF|JPG|PNG)$/.test(fileName)) {
                            fileName = "";
                            return false;
                        }
                    }
                    return true;
                }
                $scope.startProgressBar();

                if (cfg.openId === undefined) {
                    toaster.error("温馨提示", "请到微信>设置>通用>清理微信存储空间后再进入页面");
                    return;
                }

                if ($scope.fileName === '') {
                    toaster.error("温馨提示", "图片上传出错");
                    $scope.completeProgressBar();
                    return;
                }
                var tempfile = $scope.fileName;
                var extension = tempfile.substring(tempfile.lastIndexOf('.'), tempfile.length).toLowerCase();
                if (extension !== "") {
                    if (!checkImgType($scope.fileName)) {
                        alert($scope.fileName);
                        toaster.error("温馨提示", "删除当前文件，选择图片上传");
                        $scope.completeProgressBar();
                        return;
                    }
                }
                if ($scope.description === '') {
                    toaster.error("温馨提示", "添加描述后再提交");
                    $scope.completeProgressBar();
                    return;
                }
                var imageData = {
                    fileName: $scope.fileName,
                    description: $scope.description,
                    openId: cfg.openId,
                    nickName:cfg.nickName
                };
                var url = cfg.serverApiUrl + 'Image/submit';
                $http.post(url, imageData).
                  success(function (data, status, headers, config) {
                      $scope.completeProgressBar();
                      $scope.reset();
                      $window.location.href = "success.html";
                  }).
                  error(function (data, status, headers, config) {
                      $scope.completeProgressBar();
                      toaster.error("温馨提示", "服务器故障");
                  });
            };

            $scope.reset = function () {
                //$scope.fileName = '';
                $scope.description = '';
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

            $timeout(function () {
                $scope.completeProgressBar();
            }, 750);

        }]);

$(function () {
    var cfg = window.config;
    if (!cfg.compareDateWithToday(cfg.uploadStartDate))
        window.location.href = "warning.html";
})