angular.module("uploadApp", ['chieffancypants.loadingBar', 'ngAnimate', 'appConfig'])
   .config(function (cfpLoadingBarProvider) {
       cfpLoadingBarProvider.includeSpinner = true;
   })
    .controller("UploadController", ['$scope', '$http', '$timeout', 'cfpLoadingBar', 'config', function ($scope, $http, $timeout, cfpLoadingBar, cfg) {

        $scope.fileName = '';
        $scope.description = '';

        $scope.submit = function () {
            $scope.startProgressBar();
            var imageData = {
                fileName: $scope.fileName,
                description: $scope.description,
                openId: cfg.openId
            };
            var url = cfg.serverApiUrl + 'Image/submit';
            alert(url);
            $http.post(url, imageData).
              success(function (data, status, headers, config) {
                  $scope.completeProgressBar();
                  $scope.reset();
              }).
              error(function (data, status, headers, config) {
                  $scope.completeProgressBar();
                  alert('something wrong');
              });
            //alert('name:' + $scope.fileName + '; description' + $scope.description);
        };

        $scope.reset = function () {
            $scope.fileName = '';
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