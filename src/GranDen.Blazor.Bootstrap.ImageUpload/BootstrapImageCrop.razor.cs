using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Implementation;

namespace GranDen.Blazor.Bootstrap.ImageUpload
{
    /// <summary>
    /// Image Cropper component
    /// </summary>
    public partial class BootstrapImageCrop
    {
        [Inject] private ILogger<BootstrapImageCrop> Logger { get; set; }

        /// <summary>
        /// Outer container CSS class, default is "d-flex flex-column justify-content-center align-items-center align-content-start"
        /// </summary>
        [Parameter]
        public string ContainerCssClass { get; set; } =
            "d-flex flex-column justify-content-center align-items-center align-content-start";

        /// <summary>
        /// Toolbar container CSS class, default is "d-flex flex-row justify-content-center align-items-center align-content-center"
        /// </summary>
        [Parameter]
        public string ToolBarCssClass { get; set; } =
            "d-flex flex-row justify-content-center align-items-center align-content-center";

        /// <summary>
        /// Image Part (Cropper.js canvas &amp; Cropped Result view) container CSS class, default is
        /// "p-1 d-flex flex-column justify-content-center align-items-center align-content-start"
        /// </summary>
        [Parameter]
        public string ImagePartContainerCssClass { get; set; } =
            "p-1 d-flex flex-column justify-content-center align-items-center align-content-start";

        /// <summary>
        /// Cropper.js Canvas div container CSS class, default is "p-1 w-100 d-flex justify-content-center align-items-stretch"
        /// </summary>
        [Parameter]
        public string CanvasContainerCssClass { get; set; } = "p-1 d-flex justify-content-center align-items-stretch";

        /// <summary>
        /// Cropper.js Canvas CSS class
        /// </summary>
        [Parameter]
        public string CropCanvasCssClass { get; set; } = "w-auto h-auto";

        /// <summary>
        /// Crop result image container CSS class, default is "p-1 d-flex justify-content-center"
        /// </summary>
        [Parameter]
        public string CroppedResultViewContainerCssClass { get; set; } = "p-1 d-flex justify-content-center";


        /// <summary>
        /// Initial prompt text when the whole component shown up
        /// </summary>
        [Parameter]
        public string DefaultFilePickPrompt { get; set; } = "Choose file";

        /// <summary>
        /// File Picker Button display string 
        /// </summary>
        [Parameter]
        public string FilePickButtonUiLabel { get; set; } = "Browse";

        /// <summary>
        /// Crop Button display string
        /// </summary>
        [Parameter]
        public string CropButtonUiLabel { get; set; } = "Crop";

        /// <summary>
        /// Reset Crop Button display string
        /// </summary>
        [Parameter]
        public string ResetCropButtonUiLabel { get; set; } = "Browse";

        /// <summary>
        /// file filter when choosing file via open file dialog of operating system, default is "image/*"
        /// </summary>
        [Parameter]
        public string AcceptPattern { get; set; } = "image/*";

        /// <summary>
        /// Set Canvas maximum width and height
        /// </summary>
        [Parameter]
        public (uint width, uint height) MaxDimension { get; set; } = (width: 1024, height: 1024);

        /// <summary>
        /// The Cropper Box aspect ratio
        /// </summary>
        [Parameter]
        public (double width, double height) CropperAspectRatio { get; set; } = (width: double.NaN, height: double.NaN);

        /// <summary>
        /// Cropper.js <code>cropBoxResizable</code> option
        /// </summary>
        [Parameter]
        public bool CropBoxResizable { get; set; } = true;

        /// <summary>
        /// Cropper.js <code>dragMode</code> option
        /// </summary>
        [Parameter]
        public string CropBoxDragOpt { get; set; } = "none";

        /// <summary>
        ///  Upload File selection change event
        /// </summary>
        [Parameter]
        public EventCallback<InputFileChangeEventArgs> InputFileChanged { get; set; }

        /// <summary>
        /// Crop Image created event 
        /// </summary>
        [Parameter]
        public EventCallback<CropImageCreatedEventArgs> CroppedImageCreated { get; set; }

        /// <summary>
        /// Reset Crop Button click event
        /// </summary>
        [Parameter]
        public EventCallback<MouseEventArgs> ResetCropButtonClicked { get; set; }

        /// <summary>
        /// Toggle to show/hide cropped resulting image
        /// </summary>
        [Parameter]
        public bool ShowCroppedResult { get; set; } = false;

        /// <summary>
        /// Provide a parameter to Manually set result crop image upload chunk size
        /// </summary>
        [Parameter]
        public long? UploadChunkSize { get; set; }


        private string _prompt;

        private ElementReference cropButton;
        private ElementReference resetCropButton;
        private ElementReference canvas;
        private ElementReference resultContainer;
        private readonly string _fileInputId = Guid.NewGuid().ToString();

        IJSObjectReference _cropperJsModule;
        DotNetObjectReference<BootstrapImageCrop> _dotNetInvokeRef;

        private readonly Guid _fileUploadId = Guid.NewGuid();

        /// <inheritdoc />
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                var importPath = $"./_content/{typeof(BootstrapImageCrop).Assembly.GetName().Name}";
                _cropperJsModule = await JS.InvokeAsync<IJSObjectReference>("import", $"{importPath}/canvasCropper.js");
                _dotNetInvokeRef = DotNetObjectReference.Create(this);

                if (double.IsNaN(CropperAspectRatio.width) || double.IsNaN(CropperAspectRatio.height))
                {
                    await _cropperJsModule.InvokeVoidAsync("initCropper", canvas, cropButton, resetCropButton,
                        ShowCroppedResult ? resultContainer : null,
                        _fileInputId,
                        _dotNetInvokeRef,
                        new {Width = MaxDimension.width, Height = MaxDimension.height},
                        null,
                        CropBoxResizable, CropBoxDragOpt);
                }
                else
                {
                    await _cropperJsModule.InvokeVoidAsync("initCropper", canvas, cropButton, resetCropButton,
                        ShowCroppedResult ? resultContainer : null,
                        _fileInputId,
                        _dotNetInvokeRef,
                        new {Width = MaxDimension.width, Height = MaxDimension.height},
                        new {Width = CropperAspectRatio.width, Height = CropperAspectRatio.height},
                        CropBoxResizable, CropBoxDragOpt);
                }
            }
        }

        private Task OnInputFileChange(InputFileChangeEventArgs e)
        {
            _prompt = e.File.Name;

            return InputFileChanged.InvokeAsync(e);
        }

        /// <summary>
        /// Collect Cropped Image event handler
        /// </summary>
        [JSInvokable]
        public async Task<IJSObjectReference> CroppedHandler()
        {
            const int defaultChunkSize = (int) (32 * 1024 * 0.95); //default signalR message size is 32KB
            var dataUrlJsGenerator =
                await _cropperJsModule.InvokeAsync<IJSObjectReference>("getCropDataImgUrlGenerator",
                    UploadChunkSize.HasValue ? UploadChunkSize : defaultChunkSize);
            if (dataUrlJsGenerator == null)
            {
                Logger.LogInformation("js data img url generator is null");
                return null;
            }

            var dataUrl = await FetchDataUrl(dataUrlJsGenerator);
            Logger.LogInformation("data img url fetched, total size = {0}", dataUrl.Length);
            await CroppedImageCreated.InvokeAsync(new CropImageCreatedEventArgs {DataImageUrl = dataUrl});

            return dataUrlJsGenerator;
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class GeneratorResponse
        {
            public string Value { get; set; }
            public bool Done { get; set; }
        }

        private static async ValueTask<string> FetchDataUrl(IJSObjectReference dataUrlJsGenerator)
        {
            var stringBuilder = new StringBuilder();
            bool finished;
            do
            {
                var fetched = await dataUrlJsGenerator.InvokeAsync<GeneratorResponse>("next");
                finished = fetched.Done;
                stringBuilder.Append(fetched.Value);
            } while (!finished);

            return stringBuilder.ToString();
        }

        private async void ResetCropButtonClick(MouseEventArgs e)
        {
            await ResetCropButtonClicked.InvokeAsync(e);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            _dotNetInvokeRef?.Dispose();

            if (_cropperJsModule != null)
            {
                await _cropperJsModule.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}