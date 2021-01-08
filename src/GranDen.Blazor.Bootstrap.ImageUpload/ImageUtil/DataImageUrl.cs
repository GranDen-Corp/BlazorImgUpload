using System;
using System.Text.RegularExpressions;

namespace GranDen.Blazor.Bootstrap.ImageUpload.ImageUtil
{
    /// <summary>
    /// Utility class for handling Data Image Url
    /// </summary>
    public static class DataImageUtil
    {
        /// <summary>
        /// Extract Data Image Url to its binary content &amp; file extension
        /// </summary>
        /// <param name="dataUrl"></param>
        /// <returns></returns>
        public static (byte[] binary, string ext) ExtractImageDataUrl(string dataUrl)
        {
            var matchGroups= Regex.Match(dataUrl, @"data:image/(?<file_type>\w+?)(;(?<encode>\w+?))?,(?<data>.+)").Groups;
            var ext = matchGroups["file_type"].Value;
            var base64Data = matchGroups["data"].Value;
            var binary = Convert.FromBase64String(base64Data);
            return (binary, ext);
        } 
    }
}