//File Upload response from the server
Dropzone.options.dropzoneForm = {
    maxFiles: 1,
    acceptedFiles: 'image/*',
    //maxFilesize: 2, //max file size 2M
    url: 'http://localhost:13239/Api/Image/Upload',
    dictFileTooBig: "当前文件大小{{filesize}},最大限制：{{maxFilesize}}",
    dictResponseError: "上传出错,错误代码:{{statusCode}}",
    init: function () {
        this.on("maxfilesexceeded", function (data) {
            var res = eval('(' + data.xhr.responseText + ')');

        });

        this.on("success", function (file, res) {
            var fileName = res.Message;
            var element = angular.element($("#fileName"));
            var controller = element.controller();
            var scope = element.scope();
            //as this happends outside of angular you probably have to notify angular of the change by wrapping your function call in $apply
            scope.$apply(function () {
                scope.fileName= fileName;
            });
        });

        this.on("addedfile", function (file) {

            // Create the remove button
            var removeButton = Dropzone.createElement("<button class='small'>取消上传</button>");


            // Capture the Dropzone instance as closure.
            var _this = this;

            // Listen to the click event
            removeButton.addEventListener("click", function (e) {
                // Make sure the button click doesn't submit the form:
                e.preventDefault();
                e.stopPropagation();
                // Remove the file preview.
                _this.removeFile(file);
                // If you want to the delete the file on the server as well,
                // you can do the AJAX request here.
            });

            // Add the button to the file preview element.
            file.previewElement.appendChild(removeButton);
        });
    }
};
