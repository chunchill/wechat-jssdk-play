var app = angular.module('app', ['ngPhotoSwipe', 'appConfig']);

app.controller('imageController', ['$scope', '$http', '$window', 'config', function ($scope, $http, $window, cfg) {
    $scope.images = [];
    $scope.vote = function (img) {
       
        alert(img);
        window.event.stopPropagation();
    };
    $scope.getImages = function () {

        var serverImageFolder = cfg.baseUrl + '/UploadedFiles/images';

        $http.get(cfg.serverApiUrl + 'Image/GetAllUploadedImages').
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