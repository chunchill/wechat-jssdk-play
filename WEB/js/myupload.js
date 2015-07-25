var app = angular.module('app', ['ngPhotoSwipe', 'appConfig']);

app.controller('imageController', ['$scope', '$http', '$window', 'config', function ($scope, $http, $window, cfg) {
    $scope.images = [];

    $scope.getImages = function () {

        var serverImageFolder = cfg.baseUrl + 'UploadedFiles/images';
        var openId = cfg.openId;
        $http.get(cfg.serverApiUrl + 'Image/GetUploadedImages?openId=' + openId).
          success(function (data, status, headers, config) {
              data.forEach(function (item) {
                  $scope.images.push({
                      src: serverImageFolder + '/' + item.FileName,
                      safeSrc: serverImageFolder + '/' + item.FileName,
                      thumb: serverImageFolder + '/' + item.FileName,
                      caption: item.Description,
                      size: screenSize(item.Width, item.Height),
                      type: 'image'
                  });
              });
          }).
          error(function (data, status, headers, config) {
              alert('something wrong!');
          });

    };

    var screenSize = function (width, height) {
        var x = width ? width : $window.innerWidth;
        var y = height ? height : $window.innerHeight;

        return x + 'x' + y;
    };

    $scope.getImages();
}]);