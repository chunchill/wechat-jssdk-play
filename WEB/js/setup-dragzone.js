//File Upload response from the server
Dropzone.options.dropzoneForm = {
    maxFiles: 2,
    url: "/todo",
    dictResponseError: "上传出错,错误代码:{{statusCode}}",
    init: function () {
        this.on("maxfilesexceeded", function (data) {
            var res = eval('(' + data.xhr.responseText + ')');

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
