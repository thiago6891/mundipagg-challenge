using System;
using System.IO;
using api.Interfaces;
using Xunit;
using NSubstitute;
using api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace tests
{
    public class TemplatesControllerTest
    {
        [Fact]
        public void TestInputShouldBeValidatedAndCleanedBeforeSaving()
        {
            var storeService = Substitute.For<IStoreService>();
            var templateService = Substitute.For<ITemplateService>();

            templateService.IsValid(Arg.Any<string>()).ReturnsForAnyArgs(true);
            templateService.Clean(Arg.Any<string>()).ReturnsForAnyArgs("");
            storeService.SaveTemplate(Arg.Any<string>()).Returns(true);

            var controller = new TemplatesController(storeService, templateService);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            
            controller.Post();

            Received.InOrder(() => {
                templateService.IsValid(Arg.Any<string>());
                templateService.Clean(Arg.Any<string>());
                storeService.SaveTemplate(Arg.Any<string>());
            });
        }

        [Fact]
        public void TestInputShouldBeValidatedAndCleanedBeforeDeleting()
        {
            var storeService = Substitute.For<IStoreService>();
            var templateService = Substitute.For<ITemplateService>();

            templateService.IsValid(Arg.Any<string>()).ReturnsForAnyArgs(true);
            templateService.Clean(Arg.Any<string>()).ReturnsForAnyArgs("");
            storeService.DeleteTemplate(Arg.Any<string>()).Returns(true);

            var controller = new TemplatesController(storeService, templateService);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            
            controller.Delete();

            Received.InOrder(() => {
                templateService.IsValid(Arg.Any<string>());
                templateService.Clean(Arg.Any<string>());
                storeService.DeleteTemplate(Arg.Any<string>());
            });
        }
    }
}