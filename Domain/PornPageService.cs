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
        public async Task<PornPage> LoadPornPageByCategory(string category, int page, string m, CancellationToken cancellationToken)
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
            var pornPage = new PornPage();
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
                pornPageItem.ImgUrl = imgNode.Attributes["src"].Value;

                string allInfo = listChannel.InnerText;

                int sindex = allInfo.IndexOf("时长", StringComparison.Ordinal);

                String duration = allInfo.Substring(sindex + 3, 5);
                pornPageItem.Duration = duration;

                int start = allInfo.IndexOf("添加时间", StringComparison.Ordinal);
                String info = allInfo.Substring(start);
                pornPageItem.Info = info.Replace("还未被评分", "");
                pageItems.Add(pornPageItem);
            });
            pornPage.Items = pageItems;
            return pornPage;
        }
    }
}