using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BooksApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using nc.Utils;

namespace nc.Controllers
{
    [ApiVersion("1.0", Deprecated = false)]
    [Produces("application/json")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        IMongoDatabase db;
        IMongoCollection<BsonDocument> books;
        public BooksController()
        {
            db = new DbManager.BaseManager().GetDataBase("Bookdb");
            books = db.GetCollection<BsonDocument>("Books");
        }

        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="customerId">客户编号</param>
        /// <param name="orderId">订单ID</param>
        /// <returns></returns>
        [Route("customers/{customerId}/orders/{orderId}")]
        public string GetOrderByCustomer(int customerId, int orderId)
        {
            var bd = new BsonDocument();
            bd.AddRange(new BsonDocument("a", customerId));
            bd.AddRange(new BsonDocument("b", orderId));
            return bd.ToJson();
        }

        [ValidateAntiForgeryToken]
        [Route("report/fileupload/{rpname}")]
        public async Task<IActionResult> UploadFile(List<IFormFile> files)
        {
            // full path to file in temp location
            var filePath = Path.GetTempFileName();

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

            return Ok();
        }

        [Route("report/file/{rpname}")]
        public IActionResult GetFile(string rpname)
        {
            // full path to file in temp location
            var filePath = Path.GetTempFileName();
            using (Stream s = new FileStream(filePath + rpname, FileMode.Open, FileAccess.Read))
            {
                return File(s, "application/octet-stream");
            }
        }

        //[DeflateCompression]//using zlib compression
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]int skip, [FromQuery]int limit)
        {
            var ops = new FindOptions
            {
                MaxTime = TimeSpan.FromMilliseconds(20),
                BatchSize = 20000
            };

            //禁止使用decimal，转用double型
            //var cond = BsonDocument.Parse("{$and:[{price:{$gte:20}},{price:{$lte:100}}]}");
            //var proj = BsonDocument.Parse("{category:0}");
            //var sort = BsonDocument.Parse("{releaseDate:-1}");
            //var res = await books.Find(cond, ops).Sort(sort).Project(proj).Skip(skip).Limit(limit).ToListAsync();

            //日期时间操作提交时必须是utc时间
            //日期时间查询时必须先转为本地时间
            //配合insert时使用了+8时区
            var start = DateTime.Parse("2019-03-27T00:00:00").AddHours(8);
            var end = DateTime.Parse("2019-03-30T03:01:37").AddHours(8);
            var cond = new BsonDocument
            {{ "$and", new BsonArray
                       {
                            new BsonDocument("releaseDate", new BsonDocument("$gte", start)),
                            new BsonDocument("releaseDate", new BsonDocument("$lte", end))
                       }
             }};
            var res = await books.Find(cond, ops).ToListAsync();

            return new OkObjectResult(new Result() { Ok = true, Change = res.Count, Data = res });
        }

        [HttpGet("{id:length(24)}", Name = "GetBook")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBook(string id)
        {
            //身份验证
            //var pwd = Request.Headers.SingleOrDefault(h => h.Key == "pwd");
            //if (pwd.Value.FirstOrDefault() != "123")
            //{
            //    return StatusCode(HttpStatusCode.NotFound);
            //}
            if (!ObjectId.TryParse(id, out ObjectId oid))
            {
                return NotFound();
            }
            var res = await books.Find(new BsonDocument("_id", oid)).FirstOrDefaultAsync();
            if (res == null)
            {
                return NotFound();
            }

            return new OkObjectResult(new Result() { Ok = true, Change = 1, Data = res });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody]string rawString)
        {
            try
            {
                var obj = BsonDocument.Parse(rawString);
                //时区+8
                obj["releaseDate"] = DateTime.Now.AddHours(8);
                await books.InsertOneAsync(obj);

                return new OkObjectResult(new Result() { Ok = true, Change = 1, Data = obj });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, [FromBody]string rawString)
        {
            var obj = BsonDocument.Parse(rawString);
            if (!ObjectId.TryParse(id, out ObjectId oid))
            {
                return NotFound();
            }

            //BsonDocument ups = new BsonDocument {
            //         { "bookName", obj.BookName },
            //         { "price", obj.Price },
            //         { "author", obj.Author },
            //          { "category", obj.Category },
            //     };
            if (obj.Contains("releaseDate") && obj["releaseDate"] != DateTime.MinValue)
            {
                obj["releaseDate"] = obj["releaseDate"];
            }
            var res = await books.UpdateManyAsync(new BsonDocument("_id", oid), new BsonDocument("$set", obj));
            return new OkObjectResult(new Result() { Ok = true, Change=res.ModifiedCount });
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId oid))
            {
                return NotFound();
            }
            var res = await books.DeleteOneAsync(new BsonDocument("_id", oid));
            return new OkObjectResult(new Result() { Ok = true, Change = res.DeletedCount });
        }
    }
}
