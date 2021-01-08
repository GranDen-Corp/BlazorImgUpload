// noinspection JSUnusedGlobalSymbols
export function hookFileUploadEvent(previewImg, fileUploadRefId) {
    let fileUpload = document.getElementById(fileUploadRefId);
    if (fileUpload !== null && previewImg !== null) {
        fileUpload.addEventListener('change', function (event) {
            previewImg.src = URL.createObjectURL(event.target.files[0]);
            previewImg.onload = function () {
                URL.revokeObjectURL(previewImg.src);
            }
        });
    }

    import('./bs-custom-file-input.min.js').then((module) => {
        bsCustomFileInput.init();
    });
}
