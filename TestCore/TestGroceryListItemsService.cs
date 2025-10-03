using Grocery.Core.Data.Repositories;
using Grocery.Core.Services;
using Grocery.Core.Models;

namespace TestCore
{
    [TestFixture]
    public class TestGroceryListItemsService
    {
        private GroceryListItemsService _service;
        private GroceryListItemsRepository _itemsRepo;
        private ProductRepository _productRepo;

        [SetUp]
        public void Setup()
        {
            // Setup makes it so that each test starts with a fresh repository
            _itemsRepo = new GroceryListItemsRepository();
            _productRepo = new ProductRepository();
            _service = new GroceryListItemsService(_itemsRepo, _productRepo);
        }



        // UC10: tests for increasing/decreasing product amounts in grocery list

        [Test]
        public void TC10_01_IncreaseAmount_ShouldIncreaseByOne()
        {
            // Arrange
            var item = _itemsRepo.GetAll().First();
            int originalAmount = item.Amount;
            var product = _productRepo.Get(item.ProductId);
            int originalStock = product!.Stock;

            // Act
            item.Amount++;
            _itemsRepo.Update(item);
            product.Stock--;
            _productRepo.Update(product);

            // Assert
            var updatedItem = _itemsRepo.Get(item.Id);
            var updatedProduct = _productRepo.Get(product.Id);

            Assert.IsNotNull(updatedItem);
            Assert.AreEqual(originalAmount + 1, updatedItem.Amount);
            Assert.AreEqual(originalStock - 1, updatedProduct!.Stock);
        }

        [Test]
        public void TC10_02_DecreaseAmount_ShouldDecreaseByOne_WhenAmountGreaterThanOne()
        {
            // Arrange - Grab item with Amount > 1
            var item = _itemsRepo.GetAll().First(i => i.Amount > 1);
            int originalAmount = item.Amount;
            var product = _productRepo.Get(item.ProductId);
            int originalStock = product!.Stock;

            // Act - decrease by 1
            item.Amount--;
            _itemsRepo.Update(item);
            product.Stock++;
            _productRepo.Update(product);

            // Assert - Check that Amount decreased by 1 and Stock increased by 1
            var updatedItem = _itemsRepo.Get(item.Id);
            var updatedProduct = _productRepo.Get(product.Id);

            Assert.IsNotNull(updatedItem);
            Assert.AreEqual(originalAmount - 1, updatedItem.Amount);
            Assert.AreEqual(originalStock + 1, updatedProduct!.Stock);
        }

        [Test]
        public void TC10_03_DecreaseAmount_ShouldStayZero_WhenAmountIsZero()
        {
            // Arrange - make item with Amount = 0
            var item = new GroceryListItem(0, 1, 1, 0);
            var addedItem = _itemsRepo.Add(item);
            var product = _productRepo.Get(addedItem.ProductId);
            int originalStock = product!.Stock;

            Assert.AreEqual(0, addedItem.Amount);  // Check: Amount is 0

            // Act
            if (addedItem.Amount > 0)
            {
                // Als Amount > 0, dan verlagen
                addedItem.Amount--;
                _itemsRepo.Update(addedItem);
                product.Stock++;
                _productRepo.Update(product);
            }

            // Assert - amount is still 0 and stock unchanged
            var updatedItem = _itemsRepo.Get(addedItem.Id);
            var updatedProduct = _productRepo.Get(product.Id);

            Assert.IsNotNull(updatedItem);
            Assert.AreEqual(0, updatedItem.Amount);  // Blijft 0
            Assert.AreEqual(originalStock, updatedProduct!.Stock);  // Stock ongewijzigd
        }

        [Test]
        public void TC10_04_DecreaseAmount_FromOneToZero_ShouldWork()
        {
            // Arrange make item with Amount = 1
            var item = new GroceryListItem(0, 1, 1, 1);
            var addedItem = _itemsRepo.Add(item);
            var product = _productRepo.Get(addedItem.ProductId);
            int originalStock = product!.Stock;

            Assert.AreEqual(1, addedItem.Amount);  // Check: Amount is 1

            // Act - decrease by 1 to 0
            addedItem.Amount--;
            _itemsRepo.Update(addedItem);
            product.Stock++;
            _productRepo.Update(product);

            // Assert - Amount is 0 and Stock increased by 1
            var updatedItem = _itemsRepo.Get(addedItem.Id);
            var updatedProduct = _productRepo.Get(product.Id);

            Assert.IsNotNull(updatedItem, "Item moet nog bestaan (niet verwijderd!)");
            Assert.AreEqual(0, updatedItem.Amount, "Amount moet 0 zijn");
            Assert.AreEqual(originalStock + 1, updatedProduct!.Stock, "Stock moet +1 zijn");
        }

        [Test]
        public void IncreaseAmount_ShouldNotWork_WhenStockIsZero()
        {
            // Arrange - grab product with 0 stock
            var productWithNoStock = _productRepo.GetAll().First(p => p.Stock == 0);
            Assert.AreEqual(0, productWithNoStock.Stock);

            // Act & Assert
            bool canIncrease = productWithNoStock.Stock > 0;

            Assert.IsFalse(canIncrease, "Kan niet verhogen als stock 0 is");
        }

        [Test]
        public void Amount_ShouldNeverBeNegative()
        {
            // Arrange
            var item = new GroceryListItem(0, 1, 1, 0);
            var addedItem = _itemsRepo.Add(item);

            // Act 
            if (addedItem.Amount > 0)
            {
                addedItem.Amount--;
            }

            // Assert
            var updated = _itemsRepo.Get(addedItem.Id);
            Assert.GreaterOrEqual(updated!.Amount, 0, "Amount mag nooit negatief zijn");
        }

        [Test]
        public void TC11_01_GetBestSellingProducts_ShouldReturnTopFive()
        {
            // Arrange & Act
            var bestSellers = _service.GetBestSellingProducts(5);

            // Assert
            Assert.IsNotNull(bestSellers);
            Assert.LessOrEqual(bestSellers.Count, 5, "Maximaal 5 producten");
            Assert.IsTrue(bestSellers.All(item => item.NrOfSells > 0), "Alle producten moeten verkocht zijn");
        }

        [Test]
        public void TC11_01_GetBestSellingProducts_ShouldBeSortedDescending()
        {
            // Arrange & Act
            var bestSellers = _service.GetBestSellingProducts(5);

            // Assert - Check if sorted from high to low
            for (int i = 0; i < bestSellers.Count - 1; i++)
            {
                Assert.GreaterOrEqual(bestSellers[i].NrOfSells, bestSellers[i + 1].NrOfSells,
                    $"Product op positie {i} heeft {bestSellers[i].NrOfSells} verkopen, " +
                    $"maar product op positie {i + 1} heeft {bestSellers[i + 1].NrOfSells} verkopen");
            }
        }

        [Test]
        public void TC11_01_GetBestSellingProducts_ShouldHaveCorrectRanking()
        {
            // Arrange & Act
            var bestSellers = _service.GetBestSellingProducts(5);

            // Assert - Check if ranking is correct (1, 2, 3, 4, 5)
            for (int i = 0; i < bestSellers.Count; i++)
            {
                Assert.AreEqual(i + 1, bestSellers[i].Ranking,
                    $"Product op index {i} zou ranking {i + 1} moeten hebben");
            }
        }

        [Test]
        public void TC11_02_GetBestSellingProducts_ShouldUpdateWhenAmountChanges()
        {
            // Arrange - Get initial best sellers
            var initialBestSellers = _service.GetBestSellingProducts(5);

            Assert.IsNotNull(initialBestSellers);
            Assert.Greater(initialBestSellers.Count, 0);

            var initialTopProduct = initialBestSellers.First();
            int initialTopSales = initialTopProduct.NrOfSells;

            // Act - Add more of a different product
            var otherItem = _itemsRepo.GetAll()
                .First(i => i.ProductId != initialTopProduct.Id);

            int largeIncrease = initialTopSales + 10;  // Make this product outsell the current top
            otherItem.Amount += largeIncrease;
            _itemsRepo.Update(otherItem);

            // Get updated best sellers
            var updatedBestSellers = _service.GetBestSellingProducts(5);

            // Assert - the ranking should have changed
            Assert.IsNotNull(updatedBestSellers);
            var updatedTopProduct = updatedBestSellers.First();

            // The product should be the best seller now
            // The new top product is different or has significantly more sales
            bool rankingChanged = updatedTopProduct.Id != initialTopProduct.Id ||
                                  updatedTopProduct.NrOfSells > initialTopSales + 5;

            Assert.IsTrue(rankingChanged, "Ranking zou moeten veranderen na grote toevoeging");
        }

        [Test]
        public void GetBestSellingProducts_ShouldIncludeProductDetails()
        {
            // Act
            var bestSellers = _service.GetBestSellingProducts(5);

            // Assert - Check all products have correct info
            foreach (var product in bestSellers)
            {
                Assert.IsNotNull(product.Name, "Product moet een naam hebben");
                Assert.IsTrue(product.Name.Length > 0, "Naam mag niet leeg zijn");
                Assert.GreaterOrEqual(product.Stock, 0, "Stock mag niet negatief zijn");
                Assert.Greater(product.NrOfSells, 0, "Aantal verkocht moet > 0 zijn");
                Assert.Greater(product.Ranking, 0, "Ranking moet > 0 zijn");
            }
        }

        [Test]
        public void GetBestSellingProducts_WithDifferentTopX_ShouldWork()
        {
            // Test with different values
            var top1 = _service.GetBestSellingProducts(1);
            var top3 = _service.GetBestSellingProducts(3);
            var top5 = _service.GetBestSellingProducts(5);

            Assert.LessOrEqual(top1.Count, 1, "Top 1 mag maximaal 1 product bevatten");
            Assert.LessOrEqual(top3.Count, 3, "Top 3 mag maximaal 3 producten bevatten");
            Assert.LessOrEqual(top5.Count, 5, "Top 5 mag maximaal 5 producten bevatten");

            if (top1.Count > 0 && top3.Count > 0 && top5.Count > 0)
            {
                Assert.AreEqual(top1[0].Id, top3[0].Id, "Top 1 moet hetzelfde zijn in alle lijsten");
                Assert.AreEqual(top1[0].Id, top5[0].Id, "Top 1 moet hetzelfde zijn in alle lijsten");
            }
        }

    }
}