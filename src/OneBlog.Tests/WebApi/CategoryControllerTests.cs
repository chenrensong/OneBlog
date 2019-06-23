using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneBlog.Areas.Admin.Controllers;
using OneBlog.Tests.Fakes;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OneBlog.Tests.WebApi
{
    [TestClass]
    public class CategoryControllerTests
    {
        private CategoriesController _ctrl;
        private HttpClient _httpClient;

        [TestInitialize]
        public void Init()
        {
            _ctrl = new CategoriesController(new FakeCategoryRepository());

            var server = new TestServer(WebHost.CreateDefaultBuilder()
                .UseStartup<Startup>());
            _httpClient = server.CreateClient();
        }

        [TestMethod]
        public async Task CategoryControllerGet()
        {
            var results = await _ctrl.Get(0, 0);
            Assert.IsTrue(results.Count >= 0);
        }

        [TestMethod]
        public async Task CategoryControllerGetById()
        {
            var blog = await _ctrl.Get(Guid.NewGuid().ToString());
            Assert.IsTrue(blog is OkObjectResult);
        }

    }
}
