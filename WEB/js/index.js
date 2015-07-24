var app = angular.module("uploadApp", []);

app.controller("UploadController", ['$scope', '$http', function ($scope, $http) {

    $scope.fileName = '';
    $scope.description = '';

    $scope.submit = function () {

        var imageData = {
            fileName: $scope.fileName,
            description: $scope.description,
            openId :"abcdefg"
        };

        $http.post('http://localhost:13239/Api/Image/submit', imageData).
          success(function (data, status, headers, config) {
              alert('submit successfully');
          }).
          error(function (data, status, headers, config) {
              alert('something wrong');
          });
        //alert('name:' + $scope.fileName + '; description' + $scope.description);
    };

    $scope.reset = function () {
        $scope.fileName = '';
        $scope.description = '';
    };

}]);