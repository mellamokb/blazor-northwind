using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BlazorNorthwind.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Net.Http;
using System.IO;

namespace BlazorNorthwind.Server.Controllers
{
    [Route("api/Category")]
    public class CategoryController : Controller
    {
        [HttpGet("All")]
        public IEnumerable<CategoryTransfer> All()
        {
            var cs = DbUtil.GetConnectionString();
            using (IDbConnection db = new SqlConnection(cs))
            {
                var categories = db.Query<Category>("Select [CategoryID],[CategoryName],[Description],[Picture] From [Categories] C");
                foreach (var category in categories)
                {
                    var dto = new CategoryTransfer();
                    dto.CategoryID = category.CategoryID;
                    dto.CategoryName = category.CategoryName;
                    dto.Description = category.Description;
                    yield return dto;
                }
            }
        }

        [HttpPut("Save/{id}")]
        public int Save(int id, [FromBody]Category category)
        {
            var cs = DbUtil.GetConnectionString();
            using (IDbConnection db = new SqlConnection(cs))
            {
                var rowsAffected = db.Execute("Update C Set CategoryName=@CategoryName, Description=@Description From [Categories] C Where [CategoryID]=@CategoryID"
                    , new { CategoryID = id, CategoryName = category.CategoryName, Description = category.Description });
                return rowsAffected;
            }
        }

        [HttpGet("Filter/{name}/{description}")]
        public IEnumerable<CategoryTransfer> Filter(string name, string description)
        {
            var result = All();
            if (!string.IsNullOrWhiteSpace(name)) result = result.Where(x => x.CategoryName.IndexOf(name.Trim(), StringComparison.InvariantCultureIgnoreCase) >= 0);
            if (!string.IsNullOrWhiteSpace(description)) result = result.Where(x => x.Description.IndexOf(description.Trim(), StringComparison.InvariantCultureIgnoreCase) >= 0);
            return result;
        }

        [HttpGet("Picture/{id}")]
        public FileResult Picture(int id)
        {
            var cs = DbUtil.GetConnectionString();
            using (IDbConnection db = new SqlConnection(cs))
            {
                var category = db.QuerySingle<Category>("Select [CategoryID],[CategoryName],[Description],[Picture] From [Categories] C Where [CategoryID]=@CategoryID", new { CategoryID = id });

                // strip OLE header
                if (category.Picture.Length > 78 && category.Picture[0] == 21 && category.Picture[1] == 28 && category.Picture[2] == 47)
                {
                    var tmp = new byte[category.Picture.Length - 78];
                    Array.Copy(category.Picture, 78, tmp, 0, category.Picture.Length - 78);
                    category.Picture = tmp;
                }

                var mimeType = GetImageType(category.Picture);

                return File(category.Picture, mimeType);
            }
        }

        public  static string GetHeaderInfo(byte[] imageData)
        {
            var sb = new StringBuilder();
            foreach (byte b in imageData.Take(8))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        public static string GetImageType(byte[] imageData)
        {
            var headerCode = GetHeaderInfo(imageData).ToUpper();

            if (headerCode.StartsWith("FFD8FFE0"))
            {
                return "image/jpeg";
            }
            else if (headerCode.StartsWith("49492A"))
            {
                return "image/tiff";
            }
            else if (headerCode.StartsWith("424D"))
            {
                return "image/bmp";
            }
            else if (headerCode.StartsWith("474946"))
            {
                return "image/gif";
            }
            else if (headerCode.StartsWith("89504E470D0A1A0A"))
            {
                return "image/png";
            }
            else
            {
                return "application/octet-stream"; //UnKnown
            }
        }
    }
}