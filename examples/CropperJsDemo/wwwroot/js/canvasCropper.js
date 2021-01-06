// noinspection JSUnusedGlobalSymbols
export function initCropper(canvas, cropButton, restoreButton, resultContainer, fileInputId) {
    let context = canvas.getContext("2d");
    let cropper = null;
    let previewImg = new Image();
    let uploadMineType = 'image/png';
    let fileInput = document.getElementById(fileInputId);

    if (fileInput) {
        fileInput.addEventListener('change', function (event) {
            let uploadFile = event.target.files[0];
            if (uploadFile.type.match(/^image\//)) {
                uploadMineType = uploadFile.type;
                previewImg.onload = function () {
                    if (cropper !== null) {
                        cropper.destroy();
                        cropper = null;
                    }
                    context.clearRect(0, 0, canvas.width, canvas.height);
                    context.canvas.height = previewImg.height;
                    context.canvas.width = previewImg.width;
                    context.drawImage(previewImg, 0, 0);
                    cropper = new Cropper(canvas, {
                        aspectRatio: 1,
                        center: true
                    });
                };
                if (previewImg.src) {
                    URL.revokeObjectURL(previewImg.src);
                }
                previewImg.src = URL.createObjectURL(uploadFile);
            }
        });
    }

    cropButton.addEventListener('click', function (e) {
        if (!cropper) {
            return;
        }
        const croppedImageDataURL = cropper.getCroppedCanvas().toDataURL(uploadMineType);
        const resultImg = new Image();
        resultImg.src = croppedImageDataURL;
        resultContainer.append(resultImg);
    });

    restoreButton.addEventListener('click', function (e) {
        if (cropper) {
            cropper.reset();
        }
        let previewImg = resultContainer.firstChild;
        if (previewImg) {
            previewImg.remove();
        }
    });

}
