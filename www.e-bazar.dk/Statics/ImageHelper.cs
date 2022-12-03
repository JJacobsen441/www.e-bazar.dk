using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using www.e_bazar.dk.SharedClasses;

namespace www.e_bazar.dk.Statics
{
    public class ImageHelper
    {
        public static PATH GetPath(TYPE type)
        {
            switch (type)
            {
                case TYPE.PROFILE:
                    return PATH.PROFILE_DIRECTORY_TMP;
                case TYPE.BOOTH:
                    return PATH.BOOTH_DIRECTORY_TMP;
                case TYPE.PRODUCT:
                    return PATH.PRODUCT_DIRECTORY_TMP;
                case TYPE.COLLECTION:
                    return PATH.COLLECTION_DIRECTORY_TMP;
                default:
                    return PATH.DEFAULT;
            }
        }

        public static Image CropImage(Image tmpImg, TYPE type)
        {
            int _divW = type == TYPE.PROFILE ? 4 : 5;
            int _divH = type == TYPE.PROFILE ? 5 : 4;
            if (type == TYPE.BOOTH)
            {
                _divW = 6;
                _divH = 4;
            }
            int _div = Math.Max(_divH, _divW) - Math.Min(_divH, _divW);

            double per = (double)((double)Math.Min(_divW, _divH) / (double)Math.Max(_divW, _divH));
            double _ratio = (double)tmpImg.Height < (double)tmpImg.Width
                ? (double)tmpImg.Height / (double)tmpImg.Width
                : (double)tmpImg.Width / (double)tmpImg.Height;

            int _w = 0, _h = 0;
            if (type != TYPE.PROFILE)//landscape
            {
                double _s_w = (double)tmpImg.Width / (double)tmpImg.Height;
                double _s_h = (double)tmpImg.Height / (double)tmpImg.Width;
                double scale_w = (double)_divW / (double)_divH;
                double scale_h = (double)_divH / (double)_divW;
                if (_s_w > 1)//vandret
                {
                    if (tmpImg.Width < tmpImg.Height * scale_w)
                    {
                        double s = _s_w * scale_h;
                        _w = (int)((double)tmpImg.Width/* * _s_h*/);
                        _h = (int)((double)tmpImg.Height * s);
                    }
                    else
                    {
                        double s = _s_h * scale_w;
                        _w = (int)((double)tmpImg.Width * s);
                        _h = (int)((double)tmpImg.Height);
                    }
                }
                else//lodret
                {
                    double s = _s_w * scale_h;
                    _w = (int)((double)tmpImg.Width);
                    _h = (int)((double)tmpImg.Height * s);
                }
            }

            if (type == TYPE.PROFILE)//portrait
            {
                double _s_w = (double)tmpImg.Width / (double)tmpImg.Height;
                double _s_h = (double)tmpImg.Height / (double)tmpImg.Width;
                double scale_w = (double)_divW / (double)_divH;
                double scale_h = (double)_divH / (double)_divW;
                if (_s_w > 1)//vandret
                {
                    double s = _s_h * scale_w;
                    _w = (int)((double)tmpImg.Width * s);
                    _h = (int)((double)tmpImg.Height);
                }
                else//lodret
                {
                    if (tmpImg.Height > tmpImg.Width * scale_h)
                    {
                        double s = _s_w * scale_h;
                        _w = (int)((double)tmpImg.Width);
                        _h = (int)((double)tmpImg.Height * s);
                    }
                    else
                    {
                        double s = _s_h * scale_w;
                        _w = (int)((double)tmpImg.Width * s);
                        _h = (int)((double)tmpImg.Height);
                    }
                }
            }

            /*if (type == TYPE.PROFILE)//portrait
            {
                len_1 = (double)((double)tmpImg.Width / (double)_divW);
                //bool landscape = false;
                if ((double)tmpImg.Width + (len_1 * _div) < (double)tmpImg.Height && (double)tmpImg.Width < (double)tmpImg.Height)
                {
                    _r1 = ((double)tmpImg.Width + (len_1 * _div)) / (double)tmpImg.Height;
                    _w = (int)((double)tmpImg.Width);
                    _h = (int)((double)tmpImg.Height * _r1);
                }
                else if ((double)tmpImg.Width < (double)tmpImg.Height)
                {
                    len_2 = (double)tmpImg.Width / _divH;
                    _w = (int)(double)((len_2 * _divW));
                    _h = (int)((double)tmpImg.Width);
                }
                else if ((double)tmpImg.Width + (len_1 * _div) > (double)tmpImg.Height && (double)tmpImg.Width > (double)tmpImg.Height)
                {
                    _r1 = (double)tmpImg.Height / ((double)tmpImg.Width + (len_1 * _div));
                    _w = (int)((double)tmpImg.Width * _r1);
                    _h = (int)(double)tmpImg.Height;
                }
                else if ((double)tmpImg.Width >= (double)tmpImg.Height)
                {
                    len_2 = (double)tmpImg.Height / _divH;
                    _w = (int)(double)((len_2 * _divW));
                    _h = (int)((double)tmpImg.Height);
                }
            }*/
            Rectangle cropRect = new Rectangle(0, 0, _w, _h);
            Bitmap bmpImage = new Bitmap(tmpImg);
            return bmpImage.Clone(cropRect, bmpImage.PixelFormat);
        }

        public static Image FixOrientation(MemoryStream ms)
        {
            Image originalImage = Image.FromStream(ms);

            if (originalImage.PropertyIdList.Contains(274))//0x0112
            {
                int rotationValue = originalImage.GetPropertyItem(274).Value[0];
                switch (rotationValue)
                {
                    case 1: // landscape, do nothing
                        break;
                    case 2:
                        originalImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                    case 3: // bottoms up
                        originalImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case 4:
                        originalImage.RotateFlip(RotateFlipType.Rotate180FlipX);
                        break;
                    case 5:
                        originalImage.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case 6: // rotated 90 left
                        originalImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case 7:
                        originalImage.RotateFlip(RotateFlipType.Rotate270FlipX);
                        break;
                    case 8: // rotated 90 right
                        originalImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                }
                originalImage.RemovePropertyItem(274);
            }
            return originalImage;
        }

        public static Image ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
        
        public static Image ResizeImageKeepRatio(Image image, int maxWidth, int maxHeight)
        {
            // Get the image's original width and height
            int originalWidth = image.Width;
            int originalHeight = image.Height;

            // To preserve the aspect ratio
            float ratioX = (float)maxWidth / (float)originalWidth;
            float ratioY = (float)maxHeight / (float)originalHeight;
            float ratio = Math.Min(ratioX, ratioY);

            float sourceRatio = (float)originalWidth / originalHeight;

            // New width and height based on aspect ratio
            int newWidth = (int)(originalWidth * ratio);
            int newHeight = (int)(originalHeight * ratio);

            // Convert other formats (including CMYK) to RGB.
            Bitmap newImage = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb);

            // Draws the image in the specified size with quality mode set to HighQuality
            using (Graphics graphics = Graphics.FromImage(newImage))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return newImage;
        }
    }
}