using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using PornWebApi.Domain.Interfaces;
using PornWebApi.Models;

namespace PornWebApi.Domain
{
    public class PornPageService : IPornPageService
    {
        public async Task<List<PornPageItem>> LoadPornPageByCategory(string category, int page, string m, CancellationToken cancellationToken)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Referrer = new Uri("http://www.91porn.com/v.php");
            httpClient.DefaultRequestHeaders.Host = "www.91porn.com";
            httpClient.DefaultRequestHeaders.Add("Cookie", "language=cn_CN;");
            string url = $"http://www.91porn.com/v.php?category={category}&viewtype=basic&page={page}&m={m}";
            var response = await httpClient.GetAsync(url, cancellationToken);
            var html = await response.Content.ReadAsStringAsync();
            var pageDocument = new HtmlDocument();
            pageDocument.LoadHtml(html);

            HtmlNode body = pageDocument.GetElementbyId("fullside");

            var pageItems = new List<PornPageItem>();

            var listChannels = body.SelectNodes("//*[@class='listchannel']");
            Parallel.ForEach(listChannels, (listChannel) =>
            {
                var pornPageItem = new PornPageItem();
                var aNode = listChannel.ChildNodes.FindFirst("a");
                string contentUrl = aNode.Attributes["href"].Value;
                contentUrl = contentUrl.Substring(0, contentUrl.IndexOf("&", StringComparison.Ordinal));
                pornPageItem.ViewKey = contentUrl.Substring(contentUrl.IndexOf("=", StringComparison.Ordinal) + 1);
                var imgNode = aNode.ChildNodes.FindFirst("img");
                pornPageItem.Title = imgNode.Attributes["title"].Value;
                pornPageItem.ImgUrl = imgNode.Attributes["src"].Value.Replace("1_", "").Replace("2_", "").Replace("3_", "");

                string allInfo = listChannel.InnerText;

                int sindex = allInfo.IndexOf("时长", StringComparison.Ordinal);

                String duration = allInfo.Substring(sindex + 3, 5);
                pornPageItem.Duration = duration;

                string info = allInfo.Replace("\n", "")
                                     .Replace(" ", "")
                                     .Replace("&nbsp;", "");

                int start = info.IndexOf("添加时间", StringComparison.Ordinal);
                int authorIndex = info.IndexOf("作者", StringComparison.Ordinal);
                int viewsIndex = info.IndexOf("查看", StringComparison.Ordinal);
                int favoriteIndex = info.IndexOf("收藏", StringComparison.Ordinal);

                pornPageItem.AddedTime = info.Substring(start, authorIndex - start);
                pornPageItem.Author = info.Substring(authorIndex, viewsIndex - authorIndex);
                pornPageItem.ViewsNumber = info.Substring(viewsIndex, favoriteIndex - viewsIndex);
                
                pageItems.Add(pornPageItem);
            });
            return pageItems;
        }
    }
}