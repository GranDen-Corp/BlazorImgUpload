using System;

namespace GranDen.Blazor.Bootstrap.ImageUpload
{
    /// <summary>
    /// Raised event when crop image created
    /// </summary>
    public class CropImageCreatedEventArgs : EventArgs
    {
        /// <summary>
        /// Data Image Url
        /// </summary>
        public string DataImageUrl { get; set; }
    }
}