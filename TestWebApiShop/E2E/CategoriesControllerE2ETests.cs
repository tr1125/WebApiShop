using Xunit;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using WebApiShop;

namespace TestWebApiShop.E2E
{
    public class CategoriesControllerE2ETests : IAsyncLifetime
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;

        public async Task InitializeAsync()
        {
            _factory = new WebApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }

        public async Task DisposeAsync()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        #region Get All Categories Tests

        [Fact]
        public async Task Get_ReturnsOkStatus()
        {
            // Act
            var response = await _client.GetAsync("/api/categories");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_ReturnsListOfCategories()
        {
            // Act
            var response = await _client.GetAsync("/api/categories");
            var categories = await response.Content.ReadAsAsync<List<CategoryDTO>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            categories.Should().NotBeNull();
            categories.Should().BeOfType<List<CategoryDTO>>();
        }

        [Fact]
        public async Task Get_ContentTypeIsJson()
        {
            // Act
            var response = await _client.GetAsync("/api/categories");

            // Assert
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        }

        [Fact]
        public async Task Get_ReturnsValidListStructure()
        {
            // Act
            var response = await _client.GetAsync("/api/categories");
            var categories = await response.Content.ReadAsAsync<List<CategoryDTO>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            categories.Should().AllSatisfy(cat =>
            {
                cat.Should().NotBeNull();
                cat.CategoryName.Should().NotBeNullOrEmpty();
            });
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task Get_InvalidRoute_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/categories/invalid");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_ConcurrentRequests_AllSucceed()
        {
            // Act
            var tasks = Enumerable.Range(0, 5)
                .Select(i => _client.GetAsync("/api/categories"))
                .ToList();

            var responses = await Task.WhenAll(tasks);

            // Assert
            responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
        }

        #endregion

        #region Response Validation Tests

        [Fact]
        public async Task Get_ResponseIsSerializable()
        {
            // Act
            var response = await _client.GetAsync("/api/categories");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().NotBeNullOrEmpty();
            content.Should().StartWith("[");
            content.Should().EndWith("]");
        }

        [Fact]
        public async Task Get_MultipleCallsReturnConsistentData()
        {
            // Act
            var response1 = await _client.GetAsync("/api/categories");
            var response2 = await _client.GetAsync("/api/categories");
            var categories1 = await response1.Content.ReadAsAsync<List<CategoryDTO>>();
            var categories2 = await response2.Content.ReadAsAsync<List<CategoryDTO>>();

            // Assert
            response1.StatusCode.Should().Be(HttpStatusCode.OK);
            response2.StatusCode.Should().Be(HttpStatusCode.OK);
            categories1.Should().HaveCount(categories2.Count);
        }

        #endregion
    }
}
