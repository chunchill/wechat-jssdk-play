var app = angular.module('app', ['ngPhotoSwipe']);

app.controller('imageController', ['$scope', '$http', '$window', function ($scope, $http, $window) {
    $scope.images = [];

    $scope.getImages = function () {

        var serverImageFolder = 'http://localhost:13239/UploadedFiles/images';
        var openId = 'abcdefg';
        $http.get('http://localhost:13239/Api/Image/GetUploadedImages?openId=' + openId).
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