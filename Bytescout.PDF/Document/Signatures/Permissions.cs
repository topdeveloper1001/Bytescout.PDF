using System;
using System.Collections.Generic;
using System.Text;

namespace Bytescout.PDF
{
    internal class Permissions : Sig
    {
        internal Permissions(Certificate certificate)
            : base(certificate)
        {
            PDFDictionary dict = base.GetDictionary();
            //dict.AddItem("Filter", new PDFName("Adobe.PPKMS"));  // Adobe.PPKLite
            //dict.AddItem("SubFilter", new PDFName("adbe.pkcs7.sha1"));  // adbe.pkcs7.detached

            PDFDictionary transformParams = new PDFDictionary();
            transformParams.AddItem("Type", new PDFName("TransformParams"));
            transformParams.AddItem("V", new PDFName("2.2"));

            PDFArray document = new PDFArray();
            document.AddItem(new PDFName("FullSave"));
            transformParams.AddItem("Document", document);
            PDFArray form = new PDFArray();
            form.AddItem(new PDFName("Add"));
            form.AddItem(new PDFName("FillIn"));
            form.AddItem(new PDFName("Delete"));
            form.AddItem(new PDFName("SubmitStandalone"));
            transformParams.AddItem("Form", form);
            PDFArray signature = new PDFArray();
            signature.AddItem(new PDFName("Modify"));
            transformParams.AddItem("Signature", signature);


            PDFDictionary reference = new PDFDictionary();
            reference.AddItem("Type", new PDFName("SigRef"));
            reference.AddItem("TransformMethod", new PDFName("UR3"));
            reference.AddItem("TransformParams", transformParams);
            PDFArray references = new PDFArray();
            references.AddItem(reference);
            dict.AddItem("Reference", references);


            //PDFDictionary app = new PDFDictionary();
            //app.AddItem("Name", new PDFName("test"));
            //PDFArray os = new PDFArray();
            //os.AddItem(new PDFName("Win"));
            //app.AddItem("OS", os);
            //app.AddItem("R", new PDFNumber(988160));
            ////app.AddItem("REx", new PDFString("2015.020.20039"));
 
            //PDFDictionary filter = new PDFDictionary();
            //filter.AddItem("Date", new PDFString(System.Text.Encoding.Default.GetBytes("Sep 30 2016 23:36:28"), false));
            //filter.AddItem("Name", new PDFName("Adobe.PPKMS"));
            //filter.AddItem("R", new PDFNumber(131104));
            //filter.AddItem("V", new PDFNumber(2));

            //PDFDictionary pubSec = new PDFDictionary();
            //pubSec.AddItem("Date", new PDFString(System.Text.Encoding.Default.GetBytes("Sep 30 2016 23:36:28"), false));
            //pubSec.AddItem("NonEFontNoWarn", new PDFBoolean(true));
            //pubSec.AddItem("R", new PDFNumber(131105));

            //PDFDictionary prop_build = new PDFDictionary();
            //prop_build.AddItem("App", app);
            //prop_build.AddItem("Filter", filter);
            //prop_build.AddItem("PubSec", pubSec);
            //dict.AddItem("Prop_Build", prop_build);


        }

    }
}
