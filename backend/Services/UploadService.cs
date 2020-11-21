using backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace backend.Services
{
  public class UploadService
  {
    private readonly IMongoCollection<Photo> _photos;
    private readonly string _baseUrl;

    public UploadService(IConfiguration config)
    {
      //string connectionString = ConfigurationExtensions.GetConnectionString(config, "MONGODB_CONNECTION");

      var client = new MongoClient(config.GetConnectionString("WrautomatenDb"));
      //var client = new MongoClient(connectionString);

      var database = client.GetDatabase("wrautomaten");
      _photos = database.GetCollection<Photo>("Photos");


      _baseUrl = config.GetSection("AppSettings")["BaseUrl"];
      // _baseUrl = config.GetValue<string>("BASE_URL");
    }

    public List<Photo> Get(int? limit)
    {
      return _photos.Find<Photo>(Order => true).ToList();
    }

    public Photo Get(string id)
    {
      return _photos.Find<Photo>(x => x.Id == id).FirstOrDefault();
    }

    public Photo Create(string filename)
    {
      Photo upload = new Photo();

      upload.Url = $"{ _baseUrl }/images/{ filename }";

      upload.Alt = filename.Substring(0, filename.IndexOf("."));
      upload.Title = upload.Alt;

      _photos.InsertOne(upload);

      return upload;
    }

    public bool IsValidImage(Stream imageStream)
    {
      try
      {
        var img = System.Drawing.Image.FromStream(imageStream);
        return true;
      }
      catch
      {
        // bad image
        return false;
      }
    }

    public Photo Upload(IFormFile file, string path)
    {
      try
      {
        string folderName = "images";

        string newPath = Path.Combine(path, folderName);
        if (!Directory.Exists(newPath))
        {
          Directory.CreateDirectory(newPath);
        }
        if (file.Length > 0)
        {
          string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
          string fullPath = Path.Combine(newPath, fileName);
          if (File.Exists(fullPath))
          {
            for (int i = 1; i >= 1; i++)
            {
              string newFileName = fileName.Substring(0, fileName.IndexOf(".")) + i.ToString() + fileName.Substring(fileName.IndexOf("."));
              fullPath = Path.Combine(newPath, newFileName);
              if (!File.Exists(fullPath))
                i = -2;
            }
          }

          bool fakeImage = false;
          using (var stream = new FileStream(fullPath, FileMode.Create))
          {
            file.CopyTo(stream);
            if (!IsValidImage(stream))
              fakeImage = true;
          }
          if (fakeImage)
          {
            System.IO.File.Delete(fullPath);
            return null;
          }


          return Create(fileName);


        }
        else
        {
          return null;
        }
      }
      catch
      {
        return null;
      }
    }

    public Photo Update(string id, Photo photo)
    {
      _photos.ReplaceOne(x => x.Id == id, photo);

      return photo;
    }

    public void Remove(string id)
    {
      _photos.DeleteOne(x => x.Id == id);
    }
  }
}
