let dotNetInvokeReference = null;

// noinspection JSUnusedGlobalSymbols
export function initCropper(canvas, cropButton, restoreButton, resultContainer, fileInputId, dotNetRef, maxDimOpt) {
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
                    cropper = new Cropper(canvas, {
                        aspectRatio: 369 / 210,
                        maxWidth: maxAllowWidth,
                        maxHeight: maxAllowHeight,
                        center: true,
                        cropBoxResizable: false,
                        dragMode: 'move'
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

        // noinspection JSUnresolvedFunction 
        dotNetInvokeReference.invokeMethodAsync('CroppedHandler', croppedImageDataURL);

        // TODO: implement signalR stream functionality as MS <InputFile> component
        // see: https://github.com/dotnet/aspnetcore/blob/master/src/Components/Web/src/Forms/InputFile/SharedBrowserFileStream.cs
        // cropper.getCroppedCanvas().toBlob((blob => {
        //     let croppedFile = new File([blob], uploadFileName + "_crop", {type: uploadMineType});
        //     dotNetInvokeReference.invokeMethodAsync('CroppedHandler', croppedFile);
        // }));

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
