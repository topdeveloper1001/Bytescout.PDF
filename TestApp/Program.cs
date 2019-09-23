using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Bytescout.PDF;
using Font = Bytescout.PDF.Font;
using Image = Bytescout.PDF.Image;
using SolidBrush = Bytescout.PDF.SolidBrush;
using StringFormat = Bytescout.PDF.StringFormat;

namespace TestApp
{
	class Program
	{
		static void Main(string[] args)
        {
            //CheckBoxIssue();
            //TestSigning();
            //TestGetImages();
        }

        private static void TestSigning()
        {
            string input = @"c:\5\fw8ben.pdf";
            string output = input + "_signed.pdf";

            using (Document pdfDocument = new Document())
            {
                pdfDocument.RegistrationName = "demo";
                pdfDocument.RegistrationKey = "demo";

                pdfDocument.Load(input, "password");

                var certData = @"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";
	            var certPath = @"cert.p12";
                string certPassword = "abc1";

                var bytes = Convert.FromBase64String(certData);
                File.WriteAllBytes(certPath, bytes);

                // Invisible signature
                pdfDocument.Sign(certPath, certPassword);

                pdfDocument.Save(output);
            }

            Process.Start(output);
        }

        private static void CheckBoxIssue()
        {
            Document doc = new Document(@"c:\5\inbox-technical30\Page1.pdf");
            
            for (var i = 0; i < doc.Pages.Count; i++)
            {
                Page page = doc.Pages[i];

                Console.WriteLine($"Page {i} -----------------------------------------");

                foreach (Annotation annotation in page.Annotations)
                {
                    if (annotation is Field field)
                        Console.WriteLine(field.Name);
                }
            }

            ((EditBox) doc.Pages[0].Annotations["Text1"]).Text = "qwe";
            ((CheckBox) doc.Pages[0].Annotations["Check Box1"]).Checked = true;
            
            doc.Save(@"1.pdf");
            doc.Dispose();

            Process.Start(@"1.pdf");
        }

        private static void TestGetImages()
        {
            string input = @"c:\5\inbox-technical74\Big Yellow Group 2017 FS.pdf";
            
            using (Document doc = new Document(input))
            {
                var images = doc.Pages[0].Images;
                Image image = images[0];
            }
        }
    }
}