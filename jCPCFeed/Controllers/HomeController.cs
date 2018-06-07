using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using TagLib;

namespace jCPCFeed.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return Content(xmlStr(), "text/xml");
        }

        private string xmlStr() {

            // PARENT: [lastBuildDate], [items]
            // ITEMS: [title], [description], [summary], [mp3url], [duration], [createdate]

            var folderPath = ConfigurationManager.AppSettings["FilesPath"];
            var siteUrl = ConfigurationManager.AppSettings["SiteUrl"];
            var xmlParentPath = ConfigurationManager.AppSettings["xmlParentPath"];
            var xmlItemPath = ConfigurationManager.AppSettings["xmlItemPath"];

            var xmlParent = System.IO.File.ReadAllText(xmlParentPath);
            //xmlParent = Regex.Replace(xmlParent, @"\r\n?|\n|\t", "");
            var xmlItem = System.IO.File.ReadAllText(xmlItemPath);
            var itemsXml = new List<string>();

            var allFiles =
                Directory.GetFiles(folderPath)
                    .Select(x => new FileInfo(x))
                    .Where(x => x.Extension == ".mp3")
                    .OrderByDescending(x => x.CreationTime);

            if (!allFiles.Any()) {
                return "";
            }

            foreach (var item in allFiles)
            {
                if (item.Length == 0) {
                    System.IO.File.Delete(item.FullName);
                    continue;
                };

                TagLib.File thisMp3 = TagLib.File.Create(item.FullName);

                var secs = Convert.ToDouble(Math.Round(Convert.ToDecimal(thisMp3.Properties.Duration.TotalSeconds), 0));
                var tim = TimeSpan.FromSeconds(secs).ToString(@"hh\:mm\:ss");
                var title = item.Name.Substring(0, item.Name.Length - item.Extension.Length);

                var itemXmlDone = xmlItem.Replace("[title]", title);
                itemXmlDone = itemXmlDone.Replace("[description]", $"<![CDATA[{thisMp3.Tag.Comment}]]>");
                itemXmlDone = itemXmlDone.Replace("[mp3url]", $"{siteUrl}/audio/{item.Name}");
                itemXmlDone = itemXmlDone.Replace("[duration]", tim);
                itemXmlDone = itemXmlDone.Replace("[createdate]", item.CreationTime.ToString("ddd, dd MMM yyy HH:mm:ss GMT"));

                var itemImg = "";
                if (title.ToLower().Contains("knb"))
                {
                    itemImg = $@"{siteUrl}/content/kb.png";
                }
                else if (title.ToLower().Contains("howard"))
                {
                    itemImg = $@"{siteUrl}/content/hs.png";
                }
                else
                {
                    itemImg = $@"{siteUrl}/content/eip.png";
                }

                itemXmlDone = itemXmlDone.Replace("[itemimage]", itemImg);
                itemsXml.Add(itemXmlDone);

            }

            var lastBuildDate = allFiles.Select(x => x.CreationTime).First();

            xmlParent = xmlParent.Replace("[lastBuildDate]", lastBuildDate.ToString("ddd, dd MMM yyy HH:mm:ss GMT"));
            xmlParent = xmlParent.Replace("[items]", string.Join("", itemsXml.ToArray()));

            return xmlParent;
        }
    }
}

