using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IronSoftware.Drawing.AnyBitmap;
using SixLabors.ImageSharp;

namespace RPGCreator.thirdparty.ImageSharp
{
    internal static class ImageExtensions
    {
        #region Public Methods

        /// <summary>
        /// Extension method that converts a Image to an byte array
        /// </summary>
        /// <param name="imageIn">The Image to convert</param>
        /// <returns>An byte array containing the JPG format Image</returns>
        public static byte[] ToArray(this SixLabors.ImageSharp.Image imageIn)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                imageIn.Save(ms, JpegFormat.Instance);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Extension method that converts a Image to an byte array
        /// </summary>
        /// <param name="imageIn">The Image to convert</param>
        /// <param name="fmt"></param>
        /// <returns>An byte array containing the JPG format Image</returns>
        public static byte[] ToArray(this SixLabors.ImageSharp.Image imageIn, IImageFormat fmt)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                imageIn.Save(ms, fmt);
                return ms.ToArray();
            }
        }

        ///// <summary>
        ///// Extension method that converts a Image to an byte array
        ///// </summary>
        ///// <param name="imageIn">The Image to convert</param>
        ///// <returns>An byte array containing the JPG format Image</returns>
        //public static byte[] ToArray(this global::System.Drawing.Image imageIn)
        //{
        //    return ToArray(imageIn, ImageFormat.Png);
        //}

        ///// <summary>
        ///// Converts the image data into a byte array.
        ///// </summary>
        ///// <param name="imageIn">The image to convert to an array</param>
        ///// <param name="fmt">The format to save the image in</param>
        ///// <returns>An array of bytes</returns>
        //public static byte[] ToArray(this global::System.Drawing.Image imageIn, ImageFormat fmt)
        //{
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        imageIn.Save(ms, fmt);
        //        return ms.ToArray();
        //    }
        //}

        /// <summary>
        /// Extension method that converts a byte array with JPG data to an Image
        /// </summary>
        /// <param name="byteArrayIn">The byte array with JPG data</param>
        /// <returns>The reconstructed Image</returns>
        public static Image ToImage(this byte[] byteArrayIn)
        {
            using (MemoryStream ms = new MemoryStream(byteArrayIn))
            {
                Image returnImage = Image.Load(ms);
                return returnImage;
            }
        }

        //public static Image ToNetImage(this byte[] byteArrayIn)
        //{
        //    using (MemoryStream ms = new MemoryStream(byteArrayIn))
        //    {
        //        global::System.Drawing.Image returnImage = global::System.Drawing.Image.FromStream(ms);
        //        return returnImage;
        //    }
        //}

        #endregion Public Methods
    }
}
