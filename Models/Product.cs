using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace MongoDbApp.Models
{
    public class Product
    {
        [Display(Name = "Производитель")]
        public string Company { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string ImageId { get; set; }

        [Display(Name = "Модель")]
        public string Name { get; set; }

        [Display(Name = "Цена")]
        public int Price { get; set; }

        public bool HasImage() => String.IsNullOrWhiteSpace(ImageId) is false;
    }
}