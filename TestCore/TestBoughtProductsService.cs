using Grocery.Core.Data.Repositories;
using Grocery.Core.Services;
using NUnit.Framework;

namespace TestCore
{
    [TestFixture]
    public class TestBoughtProductsService
    {
        private BoughtProductsService _service;

        [SetUp]
        public void Setup()
        {
            var itemsRepo = new GroceryListItemsRepository();
            var listRepo = new GroceryListRepository();
            var clientRepo = new ClientRepository();
            var productRepo = new ProductRepository();
            _service = new BoughtProductsService(itemsRepo, listRepo, clientRepo, productRepo);
        }

        [Test]
        public void Get_WithValidProduct_ShouldReturnResults()
        {
            // Arrange
            int productId = 1;

            // Act
            var result = _service.Get(productId);

            // Assert
            Assert.IsNotNull(result);
            Assert.Greater(result.Count, 0);
        }

        [Test]
        public void Get_WithNull_ShouldReturnEmpty()
        {
            // Act
            var result = _service.Get(null);

            // Assert
            Assert.AreEqual(0, result.Count);
        }
    }
}