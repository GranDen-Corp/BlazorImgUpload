let dotNetInvokeReference = null;

let dataImgUrl = null;

function* generateStreamCharGenerator(srcStrng, chunkSize) {
    for (let i = 0; i < srcStrng.length; i += chunkSize) {
        let ret = srcStrng.slice(i, i + chunkSize);
        if (i + chunkSize >= srcStrng.length - 1) {
            return ret;
        }
        yield ret;
    }
}

// noinspection JSUnusedGlobalSymbols
export function getCropDataImgUrlGenerator(chunkSize) {
    if (dataImgUrl) {
        return generateStreamCharGenerator(dataImgUrl, chunkSize);
    }
    return null;
}

import('./cropper/cropper.min.js');

// noinspection JSUnusedGlobalSymbols
export function initCropper(canvas, cropButton, resetCropButton, resultContainer, fileInputId, dotNetRef, maxDimOpt, cropBoxAspectRatio, isCropBoxResizable, cropBoxDragModeOpt) {
    import('./bs-custom-file-input/bs-custom-file-input.min.js').then((module) => {
        // noinspection JSUnresolvedVariable,JSUnresolvedFunction
        bsCustomFileInput.init();
    });

    let context = canvas.getContext("2d");
    let cropper = null;
    let previewImg = new Image();
    let uploadFileName = '';
    let uploadMineType = 'image/png';
    let fileInput = document.getElementById(fileInputId);
    dotNetInvokeReference = dotNetRef;

    let maxAllowWidth = 1024;
    let maxAllowHeight = 1024;
    if (maxDimOpt) {
        maxAllowWidth = maxDimOpt.width;
        maxAllowHeight = maxDimOpt.height;
    }

    if (fileInput) {
        fileInput.addEventListener('change', function (event) {
            let uploadFile = event.target.files[0];
            if (uploadFile.type.match(/^image\//)) {
                uploadFileName = uploadFile.name;
                uploadMineType = uploadFile.type;
                previewImg.onload = function () {
                    if (cropper !== null) {
                        cropper.destroy();
                        cropper = null;
                    }
                    context.clearRect(0, 0, canvas.width, canvas.height);

                    let imagWidth = this.naturalWidth;
                    let imageHeight = this.naturalHeight;
                    let zoom = 1;

                    let maxDimension = Math.max(imagWidth, imageHeight);
                    if (maxDimension > maxAllowWidth) {
                        let widthRatio = maxAllowWidth / imagWidth;
                        let heightRatio = maxAllowHeight / imageHeight;
                        zoom = Math.min(widthRatio, heightRatio);
                        context.canvas.width = imagWidth * zoom;
                        context.canvas.height = imageHeight * zoom;
                    } else {
                        context.canvas.height = previewImg.height;
                        context.canvas.width = previewImg.width;
                    }

                    context.drawImage(previewImg,
                        0, 0, previewImg.width, previewImg.height,
                        0, 0, context.canvas.width, context.canvas.height);

                    let cropBoxWidthRatio = NaN;
                    let cropBoxHeightRatio = NaN;

                    if (cropBoxAspectRatio) {
                        cropBoxWidthRatio = cropBoxAspectRatio.width;
                        cropBoxHeightRatio = cropBoxAspectRatio.height;
                    }

                    let cropBoxDragMode = 'move'
                    if (cropBoxDragModeOpt) {
                        cropBoxDragMode = cropBoxDragModeOpt;
                    }

                    cropper = new Cropper(canvas, {
                        aspectRatio: cropBoxWidthRatio / cropBoxHeightRatio,
                        maxWidth: maxAllowWidth,
                        maxHeight: maxAllowHeight,
                        center: true,
                        cropBoxResizable: isCropBoxResizable,
                        dragMode: cropBoxDragMode
                    });
                }
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
        if (dataImgUrl) {
            dataImgUrl = null
        }

        dataImgUrl = cropper.getCroppedCanvas().toDataURL(uploadMineType);

        if(dotNetInvokeReference){
            // noinspection JSUnresolvedFunction
            dotNetInvokeReference.invokeMethodAsync('CroppedHandler');
        }

        // TODO: implement signalR stream functionality as MS <InputFile> component
        // see: https://github.com/dotnet/aspnetcore/blob/master/src/Components/Web/src/Forms/InputFile/SharedBrowserFileStream.cs
        // cropper.getCroppedCanvas().toBlob((blob => {
        //     let croppedFile = new File([blob], uploadFileName + "_crop", {type: uploadMineType});
        //     dotNetInvokeReference.invokeMethodAsync('CroppedHandler', croppedFile);
        // }));

        if (resultContainer) {
            const resultImg = new Image();
            resultImg.src = dataImgUrl;
            resultContainer.append(resultImg);
        }
    });

    resetCropButton.addEventListener('click', function (e) {
        if (cropper) {
            cropper.reset();
        }
        if (dataImgUrl) {
            dataImgUrl = null;
        }
        if (!resultContainer) {
            return;
        }
        const preview = resultContainer.firstChild;
        if (preview) {
            preview.remove();
        }

        if (fileInput && fileInput.value) {
            fileInput.value = null;
        }
    });

}