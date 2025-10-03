using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;

namespace Grocery.Core.Services
{
    public class GroceryListItemsService : IGroceryListItemsService
    {
        private readonly IGroceryListItemsRepository _groceriesRepository;
        private readonly IProductRepository _productRepository;

        public GroceryListItemsService(IGroceryListItemsRepository groceriesRepository, IProductRepository productRepository)
        {
            _groceriesRepository = groceriesRepository;
            _productRepository = productRepository;
        }

        public List<GroceryListItem> GetAll()
        {
            List<GroceryListItem> groceryListItems = _groceriesRepository.GetAll();
            FillService(groceryListItems);
            return groceryListItems;
        }

        public List<GroceryListItem> GetAllOnGroceryListId(int groceryListId)
        {
            List<GroceryListItem> groceryListItems = _groceriesRepository.GetAll().Where(g => g.GroceryListId == groceryListId).ToList();
            FillService(groceryListItems);
            return groceryListItems;
        }

        public GroceryListItem Add(GroceryListItem item)
        {
            return _groceriesRepository.Add(item);
        }

        public GroceryListItem? Delete(GroceryListItem item)
        {
            throw new NotImplementedException();
        }

        public GroceryListItem? Get(int id)
        {
            return _groceriesRepository.Get(id);
        }

        public GroceryListItem? Update(GroceryListItem item)
        {
            return _groceriesRepository.Update(item);
        }

        public List<BestSellingProducts> GetBestSellingProducts(int topX = 5)
        {
            List<GroceryListItem> allItems = _groceriesRepository.GetAll();

            // group by ProductId and sum the Amount for each product
            var groupedByProduct = allItems
                .GroupBy(item => item.ProductId)
                .Select(group => new
                {
                    ProductId = group.Key,
                    TotalAmount = group.Sum(item => item.Amount)
                })
                .OrderByDescending(x => x.TotalAmount)  // sort by total amount descending
                .Take(topX)
                .ToList();

            // make a list of BestSellingProducts based on the grouped data
            List<BestSellingProducts> result = new List<BestSellingProducts>();
            int ranking = 1; // start ranking from 1

            foreach (var item in groupedByProduct)
            {
                Product? product = _productRepository.Get(item.ProductId);

                if (product != null)
                {
                    BestSellingProducts bestSeller = new BestSellingProducts(
                        productId: product.Id,
                        name: product.Name,
                        stock: product.Stock,
                        nrOfSells: item.TotalAmount,
                        ranking: ranking
                    );

                    result.Add(bestSeller);
                    ranking++;
                }
            }

            return result;
        }

        private void FillService(List<GroceryListItem> groceryListItems)
        {
            foreach (GroceryListItem g in groceryListItems)
            {
                g.Product = _productRepository.Get(g.ProductId) ?? new(0, "", 0);
            }
        }
    }
}
