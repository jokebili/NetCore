using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace BooksApi.Models
{
    public class Book
    {
        /// <summary>
        /// Id
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        /// <summary>
        /// 书名
        /// </summary>
        [BsonElement("bookName")]
        public string BookName { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        [BsonElement("price")]
        public double Price { get; set; }
        /// <summary>
        /// 分类
        /// </summary>
        [BsonElement("category")]
        public string Category { get; set; }
        /// <summary>
        /// 作者
        /// </summary>
        [BsonIgnoreIfNull]
        [BsonElement("author")]
        public string Author { get; set; }
        /// <summary>
        /// 上架时间 
        /// </summary>
        [BsonElement("releaseDate")]
        public DateTime ReleaseDate { get; set; }
    }
}