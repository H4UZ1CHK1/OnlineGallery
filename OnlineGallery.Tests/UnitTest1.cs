using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using OnlineGallery.Controllers;
using OnlineGallery.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

public class ImagesControllerTests
{
    [Fact]
    public async Task Buy_AddsTransaction_WhenNotPurchased()
    {
        var options = new DbContextOptionsBuilder<OnlineGalleryContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_Buy")
            .Options;

        using var context = new OnlineGalleryContext(options);

        var testUser = new User
        {
            UserId = 8,
            FirstName = "Василий",
            CardHolderName = "Василий Петрович",
            CardNumber = "1234567890123456",
            ExpirationDate = "12/30",
            CVV = "123"
        };

        var testImage = new Image
        {
            ImageId = 1,
            Title = "Тестовое Изображение"
        };

        context.Users.Add(testUser);
        context.Images.Add(testImage);
        context.SaveChanges();

        var controller = new ImagesController(context);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
        new Claim(ClaimTypes.NameIdentifier, testUser.UserId.ToString())
    }, "mock"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user
            }
        };

        controller.TempData = new TempDataDictionary(
            controller.ControllerContext.HttpContext,
            Mock.Of<ITempDataProvider>()
        );

        var result = await controller.Buy(testImage.ImageId);

        var transaction = context.Transactions.FirstOrDefault(t => t.ImageId == testImage.ImageId && t.UserId == testUser.UserId);
        Assert.NotNull(transaction);
    }
    [Fact]
    public async Task Buy_DoesNotDuplicateTransaction()
    {
        var options = new DbContextOptionsBuilder<OnlineGalleryContext>()
            .UseInMemoryDatabase("NoDuplicateTransactionDb")
            .Options;

        using var context = new OnlineGalleryContext(options);

        context.Images.Add(new Image { ImageId = 1, Title = "test" });
        context.Users.Add(new User { UserId = 1, CardHolderName = "A", CardNumber = "111", ExpirationDate = "12/25", CVV = "123" });
        context.Transactions.Add(new Transactions { UserId = 1, ImageId = 1 });
        await context.SaveChangesAsync();

        var controller = new ImagesController(context);
        controller.ControllerContext.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "TestAuth"))
        };

        var result = await controller.Buy(1);

        Assert.Single(context.Transactions); // должно остаться 1
    }
    [Fact]
    public async Task Create_AddsImage_WhenModelIsValid()
    {
        var options = new DbContextOptionsBuilder<OnlineGalleryContext>()
            .UseInMemoryDatabase(databaseName: "CreateImageDb")
            .Options;

        using var context = new OnlineGalleryContext(options);
        var controller = new ImagesController(context);

        var image = new Image
        {
            Title = "Тест",
            Description = "Описание",
            FilePath = "/images/test.jpg",
            UserId = 1,
            CategoryId = 1,
            DateUploaded = DateTime.Now
        };

        var content = "Fake image content";
        var fileName = "test.jpg";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var formFile = new FormFile(stream, 0, stream.Length, "ImageFile", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        var result = await controller.Create(image, formFile);

        Assert.Single(context.Images);
        Assert.IsType<RedirectToActionResult>(result);
    }

}
