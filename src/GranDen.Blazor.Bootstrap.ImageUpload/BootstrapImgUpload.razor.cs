using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace GranDen.Blazor.Bootstrap.ImageUpload
{
    /// <summary>
    /// ImageUpload component
    /// </summary>
    public partial class BootstrapImgUpload
    {
        [Inject] private ILogger<BootstrapImgUpload> Logger { get; set; }

        #region Component Parameters

        /// <summary>
        /// Initial prompt text when the whole component shown up
        /// </summary>
        [Parameter]
        public string DefaultPrompt { get; set; } = "Choose file";

        /// <summary>
        /// Outer Container CSS class, default is "d-flex flex-column justify-content-center"
        /// </summary>
        [Parameter]
        public string ContainerCssClass { get; set; } = "d-flex flex-column justify-content-center";

        /// <summary>
        /// Tooggle the preview img panel 
        /// </summary>
        [Parameter]
        public bool ShowPreview { get; set; }

        /// <summary>
        /// The src attribute value of the &lt;img&gt; used in preview
        /// </summary>
        [Parameter]
        public string PreviewImgSrc { get; set; }

        /// <summary>
        /// The outside &lt;div&gt; container Css class, default is <code>p-1</code>
        /// </summary>
        [Parameter]
        public string PreviewContainerCssClass { get; set; } = "p-1";

        /// <summary>
        /// Preview Img css class string
        /// </summary>
        [Parameter]
        public string PreviewImgCssClass { get; set; } = "img-thumbnail";

        /// <summary>
        /// Upload Button display string
        /// </summary>
        [Parameter]
        public string ButtonUiLabel { get; set; } = "Browse";

        /// <summary>
        /// file filter when choosing file via open file dialog of operating system, default is "image/*"
        /// </summary>
        [Parameter]
        public string AcceptPattern { get; set; } = "image/*";

        /// <summary>
        /// The &lt;div&gt; container of Bootstrap File Browser UI, default is <code>p-1</code>
        /// </summary>
        [Parameter]
        public string FileBarContainerCssClass { get; set; } = "p-1";

        /// <summary>
        /// Upload File selection change event
        /// </summary>
        [Parameter]
        public EventCallback<InputFileChangeEventArgs> InputFileChanged { get; set; }

        #endregion

        private string _prompt;
        private string _previewImgAltText;

        private readonly string _fileUploadId = Guid.NewGuid().ToString();
        private ElementReference previewImg;

        private IJSObjectReference _blobUtilModule;

        private readonly string DisposeTimeoutLogTemplate =
            $"Disposing JSInterop object {nameof(_blobUtilModule)} in {nameof(BootstrapImgUpload)} component timeout";

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (string.IsNullOrEmpty(_prompt) && !string.IsNullOrEmpty(DefaultPrompt))
            {
                _prompt = DefaultPrompt;
            }
        }

        /// <inheritdoc />
        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                var importPath = $"./_content/{typeof(BootstrapImgUpload).Assembly.GetName().Name}";
                _blobUtilModule = await JS.InvokeAsync<IJSObjectReference>("import", $"{importPath}/imgPreviewUtil.js");

                if (ShowPreview)
                {
                    await _blobUtilModule.InvokeVoidAsync("hookFileUploadEvent", previewImg, _fileUploadId);
                }
                else
                {
                    await _blobUtilModule.InvokeVoidAsync("hookFileUploadEvent", null, _fileUploadId);
                }
            }
        }

        private Task OnInputFileChange(InputFileChangeEventArgs e)
        {
            var fileName = e.File.Name;
            _previewImgAltText = _prompt = fileName;

            return InputFileChanged.InvokeAsync(e);
        }

        #region Dispose Pattern implementation

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Actual dispose object implementation
        /// </summary>
        /// <returns></returns>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                switch (_blobUtilModule)
                {
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                    case IAsyncDisposable asyncDisposable:
                    {
                        try
                        {
                            asyncDisposable.DisposeAsync().AsTask().RunSynchronously();
                        }
                        catch (OperationCanceledException ex)
                        {
                            Logger.LogDebug(ex, DisposeTimeoutLogTemplate);
                        }

                        break;
                    }
                }
            }

            _blobUtilModule = null;
        }

        /// <summary>
        /// Actual async dispose object implementation,
        /// see: https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#implement-the-async-dispose-pattern
        /// </summary>
        /// <returns></returns>
        protected async virtual ValueTask DisposeAsyncCore()
        {
            if (_blobUtilModule != null)
            {
                try
                {
                    await _blobUtilModule.DisposeAsync().ConfigureAwait(false);
                }
                catch (OperationCanceledException ex)
                {
                    Logger.LogDebug(ex, DisposeTimeoutLogTemplate);
                }
            }

            _blobUtilModule = null;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();

            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}