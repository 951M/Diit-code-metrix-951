using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Diit.CodeMetrics.Data.Source;
using Microsoft.AspNetCore.Http;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Diit.CodeMetrics.Services.Source
{
    public class FileSourceLoader : ISourceLoader<FileSourceViewModel>
    {
        public async Task<List<Module>> LoadData(FileSourceViewModel fileSourceViewModel)
        {
            List<Module> data = new List<Module>();
            if (fileSourceViewModel == null || fileSourceViewModel.Files == null || fileSourceViewModel.Files.Count == 0)
            {
                throw new ArgumentException("Selected file loading but received 0 files");
            }
            //Project in archive
            if (fileSourceViewModel.Files.Count == 1 && fileSourceViewModel.Files[0].ContentType.Contains("zip"))
            {
                using (var fs = fileSourceViewModel.Files[0].OpenReadStream())
                {
                    using (var zipFile = new ZipFile(fs))
                    {

                        foreach (ZipEntry zipEntry in zipFile)
                        {
                            if (!zipEntry.IsFile ||
                                Path.GetExtension(zipEntry.Name).TrimStart('.') != fileSourceViewModel.FileType)
                                continue;
                            using (var ms = new MemoryStream())
                            {
                                using (var zipEntryFs = zipFile.GetInputStream(zipEntry))
                                {
                                    await zipEntryFs.CopyToAsync(ms);
                                }

                                data.Add(new Module()
                                {
                                    Path = zipEntry.Name,
                                    Source = ms.ToArray()
                                });
                            }
                        }
                    }
                }
            }
            else //Select source with type fileType
            {
                data.AddRange(fileSourceViewModel.Files
                    .Where(file => Path.GetExtension(file.FileName).TrimStart('.') == fileSourceViewModel.FileType)
                    .Select(file =>
                    {
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);

                            return new Module()
                            {
                                Path = file.FileName,
                                Source = ms.ToArray()
                            };
                        }
                    }));
            }

            return data;
        }
    }
}