using Syncfusion.EJ2.FileManager.AzureFileProvider;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Syncfusion.EJ2.FileManager.Base;

namespace EJ2FileManagerServices.Controllers
{

    [Route("api/[controller]")]
    [EnableCors("AllowAllOrigins")]
    public class FileManagerController : Controller
    {
        //Set Azure Info Here!
        private static readonly string baseFolder = "Files";
        private readonly string startPath = "https://ej2syncfusionfilemanager.blob.core.windows.net/files/";
        private string originalPath = "https://ej2syncfusionfilemanager.blob.core.windows.net/files/" + baseFolder;
        private static readonly string accountName = "ej2syncfusionfilemanager";
        private static readonly string key = "cgKqBPKOGYjPPKn/0eHa9XrYvhPThD43yDAk6QXiEW34kN5cTYY+rD0m/+aHTB1c7TbFSiq3MPDEn8mKMX7jjA==";
        private static readonly string baseBlobPath = "files";

        //DO NOT MODIFY BELOW HERE
        public string basePath;
        public AzureFileProvider operation;

        public FileManagerController()//IHostingEnvironment hostingEnvironment
        {
            this.operation = new AzureFileProvider();
            this.operation.RegisterAzure(accountName, key, baseBlobPath);
            this.operation.SetBlobContainer(startPath, originalPath);
            this.operation.SetDownloadPath(@"C:\");
        }
        [HttpPost]
        [Route("AzureFileOperations")]
        [Route("AzureFileOperations/{newroot}")]
        public object AzureFileOperations([FromBody] FileManagerDirectoryContent args, [FromRoute] string newroot)
        {
            if (args.Path != "")
            {
                originalPath = originalPath.Replace(startPath, "");
                args.Path = (originalPath + args.Path).Replace("//", "/");
                args.TargetPath = (originalPath + args.TargetPath).Replace("//", "/");

                if (newroot != null)
                {
                    string destFolder = args.Path.Replace(baseFolder, "").Replace("/","");
                    args.Path = (baseFolder + "/" + newroot + "/" + destFolder + "/").Replace("//", "/");
                    args.TargetPath = (baseFolder + "/" + newroot + "/" + destFolder).Replace("//", "/");
                }
            }
            switch (args.Action)
            {
                case "read":
                    return Json(this.ToCamelCase(this.operation.Read(args.Path, args.Data)));
                case "delete":
                    return this.ToCamelCase(this.operation.Delete(args.Path, args.Names, args.Data));
                case "details":
                    return this.ToCamelCase(this.operation.Details(args.Path, args.Names, args.Data));
                case "create":
                    return this.ToCamelCase(this.operation.Create(args.Path, args.Name));
                case "search":
                    return this.ToCamelCase(this.operation.Search(args.Path, args.SearchString, args.ShowHiddenItems, args.CaseSensitive, args.Data));
                case "rename":
                    return this.ToCamelCase(this.operation.Rename(args.Path, args.Name, args.NewName, false, args.Data));
                case "copy":
                    return this.ToCamelCase(this.operation.Copy(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData, args.Data));
                case "move":
                    return this.ToCamelCase(this.operation.Move(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData, args.Data));
            }
            return null;
        }
        public string ToCamelCase(object userData)
        {
            return JsonConvert.SerializeObject(userData, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            });
        }

        [Route("AzureUpload")]
        [Route("AzureUpload/{newroot}")]
        public ActionResult AzureUpload(FileManagerDirectoryContent args, [FromRoute] string newroot)
        {
            if (args.Path != "")
            {
                originalPath = originalPath.Replace(startPath, "");
                args.Path = (originalPath + args.Path).Replace("//", "/");

                if (newroot != null)
                {
                    string destFolder = args.Path.Replace(baseFolder, "").Replace("/", "");
                    args.Path = (baseFolder + "/" + newroot + "/" + destFolder + "/").Replace("//", "/");
                    args.TargetPath = (baseFolder + "/" + newroot + "/" + destFolder).Replace("//", "/");
                }
            }
            operation.Upload(args.Path, args.UploadFiles, args.Action, args.Data);
            return Json("");
        }

        [Route("AzureDownload")]
        [Route("AzureDownload/{newroot}")]
        public object AzureDownload(string downloadInput, [FromRoute] string newroot)
        {
            FileManagerDirectoryContent args = JsonConvert.DeserializeObject<FileManagerDirectoryContent>(downloadInput);
            if (newroot != null)
            {
                string destFolder = args.Path.Replace(baseFolder, "").Replace("/", "");
                args.Path = (baseFolder + "/" + newroot + "/" + destFolder + "/").Replace("//", "/");
                args.TargetPath = (baseFolder + "/" + newroot + "/" + destFolder).Replace("//", "/");
            }
            return operation.Download(args.Path, args.Names, args.Data);
        }


        [Route("AzureGetImage")]
        [Route("AzureGetImage/{newroot}")]
        public IActionResult AzureGetImage(FileManagerDirectoryContent args, [FromRoute] string newroot)
        {
            if (newroot != null)
            {
                args.Path = "/" + newroot + args.Path.Replace(newroot,"");
                args.Path = args.Path.Replace("//", "/");
            }
            return this.operation.GetImage(args.Path, args.Id, true, null, args.Data);
        }
    }

}
