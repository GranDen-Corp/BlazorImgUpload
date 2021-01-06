// noinspection JSUnusedGlobalSymbols
export function initCropper(canvas, cropButton, restoreButton, resultContainer, fileInputId) {

    let context = canvas.getContext("2d");
    let cropper;

    let fileInput = document.getElementById(fileInputId);
    fileInput.addEventListener('change', function (event) {
        let uploadFile = event.target.files[0];
        if (uploadFile.type.match(/^image\//)) {
            let img = new Image();
            img.onload = function () {
                // if (img.src !== null) {
                //     URL.revokeObjectURL(img.src);
                // }
                context.canvas.height = img.height;
                context.canvas.width = img.width;
                context.drawImage(img, 0, 0);
                cropper = new Cropper(canvas, {
                        aspectRatio: 1,
                        center: true
                    });
            };
            img.src = URL.createObjectURL(uploadFile);
        }
    });

    cropButton.addEventListener('click', function (e) {
        const croppedImageDataURL = cropper.getCroppedCanvas().toDataURL('image/png');
        const resultImg = new Image();
        resultImg.src = croppedImageDataURL;
        resultContainer.append(resultImg);
    });

    restoreButton.addEventListener('click', function (e) {
        cropper.reset();
        resultContainer.firstChild.remove();
    });

}
