using System;
using System.IO;
//using System.Drawing;
using System.Drawing.Imaging;

namespace Bytescout.PDF
{
    internal class DCTDecoder
    {
        public static void Encode(System.Drawing.Image image, Stream output, int jpegQuality)
        {
            ImageCodecInfo encoder = GetEncoderInfo(ImageFormat.Jpeg);
            EncoderParameters encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, jpegQuality);
            output.SetLength(0);
            image.Save(output, encoder, encoderParameters);
        }

        private static ImageCodecInfo GetEncoderInfo(ImageFormat imageFormat)
        {
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

            foreach (ImageCodecInfo encoder in encoders)
                if (encoder.FormatID == imageFormat.Guid)
                    return encoder;

            return null;
        }

        public static MemoryStream Decode(Stream stream, PDFDictionary decodeParms)
        {
            MemoryStream decoded = new MemoryStream();
#if !EXCLUDE_UNSAFE
			DCTStream dct = new DCTStream(stream, new DCTParameters(decodeParms));
            byte[] buf = new byte[4096];
            int numRead;
            while ((numRead = dct.Read(buf, 0, buf.Length)) > 0)
                decoded.Write(buf, 0, numRead);
#endif
			return decoded;
		}
    }
}
