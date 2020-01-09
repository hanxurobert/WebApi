using System;

namespace PornWebApi.Models
{
    public class PornPageItem
    {
        public PornPageItem()
        {
            Id = Guid.NewGuid().ToString();
        }
        
        public string Id { get; }

        public string ViewKey { get; set; }
        
        public string Title { get; set; }

        public string ImgUrl { get; set; }

        public string Duration { get; set; }

        public string AddedTime { get; set; }

        public string Author { get; set; }

        public string ViewsNumber { get; set; }

    }
}