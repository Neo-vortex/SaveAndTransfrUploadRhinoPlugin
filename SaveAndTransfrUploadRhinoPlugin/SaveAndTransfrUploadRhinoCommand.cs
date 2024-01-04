using Rhino;
using Rhino.Commands;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SaveAndTransfrUploadRhinoPlugin
{
    public class SaveAndTransfrUploadRhinoCommand : Command
    {
        public SaveAndTransfrUploadRhinoCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static SaveAndTransfrUploadRhinoCommand Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "SaveAndUploadToTransfr";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {


            RhinoApp.WriteLine("Saving to file...");
            RhinoDoc doc1 = RhinoDoc.ActiveDoc;
            string desktopFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (doc1 != null)
            {
                // Create a new File3dm object
                File3dm file = new File3dm();

                // Iterate through Rhino objects in the document
                foreach (var obj in doc.Objects)
                {
                    // Add each object to the File3dm object
                    file.Objects.Add(obj.Geometry, obj.Attributes);
                }
                var radnomName = Path.GetRandomFileName();
                // Write the File3dm to the specified file path
                file.Write(Path.Combine(desktopFolderPath, radnomName + ".3dm"), 8); // 8 indicates the file format version (Rhino 6)
                RhinoApp.WriteLine("Saved to " + Path.Combine(desktopFolderPath, radnomName + ".3dm"));
                RhinoApp.WriteLine("Uploading... Please wait...");
                var result = Upload(Path.Combine(desktopFolderPath, radnomName + ".3dm"), "https://transfr.one/" + radnomName + ".3dm").Result;
                result = result + "?raw";
                RhinoApp.WriteLine("Uploaded at : " + result);
                Process.Start(new ProcessStartInfo { FileName = result, UseShellExecute = true });
            }
            return Result.Success;
        }
        static async Task<string> Upload(string fileName, string apiUrl)
        {
            using (FileStream fileStream = File.OpenRead(fileName))
            using (HttpClient httpClient = new HttpClient())
            using (StreamContent content = new StreamContent(fileStream))
            {
                HttpResponseMessage response = await httpClient.PutAsync(apiUrl, content);

                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
