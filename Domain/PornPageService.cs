using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
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
    
        public async Task<PornVideoItem> AnalyzeVideoUrl(string viewKey, CancellationToken cancellationToken) 
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Referrer = new Uri("http://www.91porn.com/view_video.php");
            httpClient.DefaultRequestHeaders.Host = "www.91porn.com";
            httpClient.DefaultRequestHeaders.Add("Cookie", "language=cn_CN;");
            httpClient.DefaultRequestHeaders.Add("X-Forwarded-For", GetRandomIPAddress());

            string url = $"http://www.91porn.com/view_video.php?viewkey={viewKey}";

            var response = await httpClient.GetAsync(url, cancellationToken);
            var html = await response.Content.ReadAsStringAsync();
            var pageDocument = new HtmlDocument();
            pageDocument.LoadHtml(html);

            var videoResult = new PornVideoItem();
            /*if (html.contains("你每天只可观看10个视频")) {
                Logger.d("已经超出观看上限了");
                //设置标志位,用于上传日志
                videoResult.setId(VideoResult.OUT_OF_WATCH_TIMES);
                return videoResult;
            }
            if (html.contains("视频不存在,可能已经被删除或者被举报为不良内容!")) {
                videoResult.setId(VideoResult.VIDEO_NOT_EXIST_OR_DELETE);
                return videoResult;
            }*/
            const string pattern = "document.write\\(strencode\\(\"(.+)\",\"(.+)\",.+\\)\\);";
            var m = Regex.Match(html, pattern);
            string param1 = "", param2 = "";
            param1 = m.Groups[1].Value;
            param2 = m.Groups[2].Value;
            param1 = Encoding.UTF8.GetString(Convert.FromBase64String(param1));
            string source_str = "";
            for (int i = 0, k = 0; i < param1.Count(); i++) 
            {
                k = i % param2.Count();
                source_str += "" + (char) (param1[i] ^ param2[k]);
            }
            source_str = Encoding.UTF8.GetString(Convert.FromBase64String(source_str));

            string videoUrl = null;
            if (string.IsNullOrEmpty(source_str)) {
                videoUrl = pageDocument.DocumentNode.ChildNodes.FindFirst("video").ChildNodes.FindFirst("source").Attributes["src"].Value;
                videoResult.VideoUrl = videoUrl;
            } else {
                var source = new HtmlDocument();
                source.LoadHtml(source_str);
                videoUrl = source.DocumentNode.ChildNodes.FindFirst("source").Attributes["src"].Value;
                videoResult.VideoUrl = videoUrl;
            }

            /*
            int startIndex = videoUrl.lastIndexOf("/");
            int endIndex = videoUrl.indexOf(".mp4");
            String videoId = videoUrl.substring(startIndex + 1, endIndex);
            videoResult.setVideoId(videoId);
            Logger.t(TAG).d("视频Id：" + videoId);

            //这里解析的作者id已经变了，非纯数字了
            Document doc = Jsoup.parse(html);
            String ownerUrl = doc.select("a[href*=UID]").first().attr("href");
            String ownerId = ownerUrl.substring(ownerUrl.indexOf("=") + 1, ownerUrl.length());
            videoResult.setOwnerId(ownerId);
            Logger.t(TAG).d("作者Id：" + ownerId);

            String addToFavLink = doc.getElementById("addToFavLink").selectFirst("a").attr("onClick");
            String args[] = addToFavLink.split(",");
            String userId = args[1].trim();
            Logger.t(TAG).d("userId:::" + userId);
            user.setUserId(Integer.parseInt(userId));

            //原始纯数字作者id，用于收藏接口
            String authorId = args[3].replace(");", "").trim();
            Logger.t(TAG).d("authorId:::" + authorId);
            videoResult.setAuthorId(authorId);

            String ownerName = doc.select("a[href*=UID]").first().text();
            videoResult.setOwnerName(ownerName);
            Logger.t(TAG).d("作者：" + ownerName);

            String allInfo = doc.getElementById("videodetails-content").text();
            String addDate = allInfo.substring(allInfo.indexOf("添加时间"), allInfo.indexOf("作者"));
            videoResult.setAddDate(addDate);
            Logger.t(TAG).d("添加时间：" + addDate);

            String otherInfo = allInfo.substring(allInfo.indexOf("注册"), allInfo.indexOf("简介"));
            videoResult.setUserOtherInfo(otherInfo);
            Logger.t(TAG).d(otherInfo);

            try {
                String thumImg = doc.getElementById("player_one").attr("poster");
                videoResult.setThumbImgUrl(thumImg);
                Logger.t(TAG).d("缩略图：" + thumImg);
            } catch (Exception e) {
                e.printStackTrace();
            }
            */

            //String videoName = doc.getElementById("viewvideo-title").text();
            //videoResult.setVideoName(videoName);

            return videoResult;
        }

        private string GetRandomIPAddress() 
        {
            var radom = new Random();
            return $"{radom.Next(255)}.{radom.Next(255)}.{radom.Next(255)}.{radom.Next(255)}";
        }
    }
}