using BlazorNorthwind.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BlazorNorthwind.Server.Controllers
{
    [Route("api/Category")]
    public class CategoryController : Controller
    {
        [HttpGet("All")]
        public IEnumerable<CategoryTransfer> All()
        {
            using (var ctx = new NorthwindContext())
            {
                foreach (var category in ctx.Categories)
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
        public async Task<int> Save(int id, [FromBody]Category category)
        {
            using (var ctx = new NorthwindContext())
            {
                var cat = ctx.Categories.Find(category.CategoryID);
                cat.CategoryName = category.CategoryName;
                cat.Description = category.Description;
                var rowsAffected = await ctx.SaveChangesAsync();
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
            using (var ctx = new NorthwindContext())
            {
                var category = ctx.Categories.SingleOrDefault(c => c.CategoryID == id);
                if (category == null) return null;

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