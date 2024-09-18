using System;
using System.Collections.Generic;
using System.Linq;
using ClothesShop.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Humanizer.Localisation;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;
using sib_api_v3_sdk.Model;
using System.Buffers.Text;
using System.Diagnostics;

namespace ClothesShop.Repository.DbSeed
{
    public static class SeedData
    {
        public static void Seed(IApplicationBuilder appBuilder)
        {
            DatabaseContext _context = appBuilder
                .ApplicationServices
                .CreateScope()
                .ServiceProvider
                .GetRequiredService<DatabaseContext>();

            if (_context.Database.GetPendingMigrations().Any())
                _context.Database.Migrate();


            var anyArticles = _context.Articles.Any();

            if (!anyArticles)
            {

                using var transaction = _context.Database.BeginTransaction();
                var ArticleTypes = GetMainArticleTypes();
                _context.ArticleTypes.AddRange(ArticleTypes);

                _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT ArticleTypes ON;");
                _context.SaveChanges();
                _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT ArticleTypes OFF;");
                transaction.Commit();

                var articles = GetArticles();

                _context.Articles.AddRange(articles);
                _context.SaveChanges();
            }
        }

        static List<string> GetTableNames(this DatabaseContext context)
        {
            return new List<string>()
            {
                "Articles", "ArticleTypes", "ArticleArticleTypes"
            };
        }

        static List<Article> GetArticles()
        {
            return new List<Article>()
            {
                new Article()
                {
                    Name = "The Barfly",
                    Description = "Original 90s denim inspired. Clean lines and simple branding.",
                    Price = GetRandomPrice(),
                    ArticleTypeId = 1,
                    Image = "/assets/products/mens jeans.jfif",
                    Tags = ""
                },
                new Article()
                {
                    Name = "The Galaxy",
                    Description = "Robust men's sports sweatpants ideal for spring, summer and autumn and for running.",
                    Price = GetRandomPrice(),
                    ArticleTypeId = 1,
                    Image = "/assets/products/mens_sweatpants.jfif",
                    Tags = ""
                },
                new Article()
                {
                    Name = "Navy Blue",
                    Description = "Very practical men's shirt suitable for day-to-day purposes.",
                    Price = GetRandomPrice(),
                    ArticleTypeId = 1,
                    Image = "/assets/products/mens_shirt.jpg",
                    Tags = ""
                },
                new Article()
                {
                    Name = "Cozy",
                    Description = "Men's Cozy Fleece Button Jacket, Lightweight Cotton Comfort Coat with Lapel and Chest Pocket.",
                    Price = GetRandomPrice(),
                    ArticleTypeId = 1,
                    Image = "/assets/products/mens_jacket.jpg",
                    Tags = ""
                },
                new Article()
                {
                    Name = "Arabella",
                    Description = "A vintage women's dress suitable for every occasion you need.",
                    Price = GetRandomPrice(),
                    ArticleTypeId = 2,
                    Image = "/assets/products/dress.jpg",
                    Tags = ""
                },
                new Article()
                {
                    Name = "Sunrise",
                    Description = "Very warm beige women's jacket suitable for autumn and spring mainly.",
                    Price = GetRandomPrice(),
                    ArticleTypeId = 2,
                    Image = "/assets/products/lady_jacket.jpg",
                    Tags = ""
                },
                new Article()
                {
                    Name = "The Rose",
                    Description = "Shirt made for to meet all your needs, if you need it for business or simple coffee with friends.",
                    Price = GetRandomPrice(),
                    ArticleTypeId = 2,
                    Image = "/assets/products/lady_shirt.jpg",
                    Tags = ""
                },
                new Article()
                {
                    Name = "Alina",
                    Description = "Simple and amazing black stilletos that will go perfectly with every item of your clothes.",
                    Price = GetRandomPrice(),
                    ArticleTypeId = 2,
                    Image = "/assets/products/stilletos.jpg",
                    Tags = ""
                },
                new Article()
                {
                    Name = "Pink Dream",
                    Description = "Very warm, wind proof and water proof jacket for kids.",
                    Price = GetRandomPrice(),
                    ArticleTypeId = 3,
                    Image = "/assets/products/kids_winter_jacket.jpg",
                    Tags = ""
                },
                new Article()
                {
                    Name = "Playground",
                    Description = "The sweatpants are made from recycled cotton and polyester recycled plastic bottles. The inside is soft, brushed knitted fabric. The leg openings are elastic.",
                    Price = GetRandomPrice(),
                    ArticleTypeId = 3,
                    Image = "/assets/products/kids_pants.jfif",
                    Tags = ""
                },
                new Article()
                {
                    Name = "Palm Springs",
                    Description = "Sneakers are made of flexible kinds of material featuring soles made of rubber and an upper part made with leather or canvas.",
                    Price = GetRandomPrice(),
                    ArticleTypeId = 3,
                    Image = "/assets/products/kids_sneakers.jpg",
                    Tags = ""
                },
                new Article()
                {
                    Name = "The Witches",
                    Description = "This kids' t-shirts are made from 100% cotton pre-shrunk jersey knit which is a soft cotton that feels smooth on your child's tender skin.",
                    Price = GetRandomPrice(),
                    ArticleTypeId = 3,
                    Image = "/assets/products/kids_shirt.jpg",
                    Tags = ""
                },
            };
        }

        private static string GetRandomArticleType()
        {
            var random = new Random();
            var ArticleTypes = GetMainArticleTypes();
            var ArticleTypesCount = ArticleTypes.Count;
            int numRandomArticleType = random.Next(1, ArticleTypesCount);

            return ArticleTypes[numRandomArticleType].Name;
        }

        private static decimal GetRandomPrice()
        {
            var random = new Random();
            var basePrice = (decimal)(random.Next(6, 75) * 1.0);
            var decimalPrice = (decimal)(random.NextDouble() * 99);
            return basePrice + decimalPrice;
        }

        static List<ArticleType> GetMainArticleTypes()
        {
            return new List<ArticleType>()
            {
                new ArticleType() { ArticleTypeId = 1, Name = "Men", Description = "Men clothes designed for causual wear" },
                new ArticleType() { ArticleTypeId = 2, Name = "Women", Description = "Variaty of fashionable women's wear" },
                new ArticleType() { ArticleTypeId = 3, Name = "Kids", Description = "Childeren's wear designed for every occasion" },
                  };
        }
    }
}
