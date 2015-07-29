angular.module("defaultApp", ['chieffancypants.loadingBar', 'ngAnimate', 'toaster'])
   .config(function (cfpLoadingBarProvider) {
       cfpLoadingBarProvider.includeSpinner = true;
   })
    .controller("DefaultController", ['$scope', 'cfpLoadingBar', 'toaster', '$window','$timeout',
        function ($scope, cfpLoadingBar, toaster, $window, $timeout) {

            $scope.fileName = '';
            $scope.description = '';


            $scope.warn = function () {
                toaster.success({ title: "温馨提示", body: "亲，活动目前为推广期" });
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