
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;

namespace Grocery.Core.Services
{
    public class BoughtProductsService : IBoughtProductsService
    {
        private readonly IGroceryListItemsRepository _groceryListItemsRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IProductRepository _productRepository;
        private readonly IGroceryListRepository _groceryListRepository;
        public BoughtProductsService(IGroceryListItemsRepository groceryListItemsRepository, IGroceryListRepository groceryListRepository, IClientRepository clientRepository, IProductRepository productRepository)
        {
            _groceryListItemsRepository=groceryListItemsRepository;
            _groceryListRepository=groceryListRepository;
            _clientRepository=clientRepository;
            _productRepository=productRepository;
        }
        public List<BoughtProducts> Get(int? productId)
        {
            // Make an empty list to store results
            List<BoughtProducts> result = new();

            // If there is no product selected, return empty list
            if (productId == null) return result;

            // Search for all grocery list items that match the product ID
            var itemsWithProduct = _groceryListItemsRepository.GetAll()
                .Where(item => item.ProductId == productId.Value)
                .ToList();

            // For every found item, find the associated grocery list, client, and product
            foreach (var item in itemsWithProduct)
            {
                // search the grocery list
                var groceryList = _groceryListRepository.Get(item.GroceryListId);
                if (groceryList == null) continue; // Skip als niet gevonden

                // search the client of the grocery list
                var client = _clientRepository.Get(groceryList.ClientId);
                if (client == null) continue;

                // search the product
                var product = _productRepository.Get(item.ProductId);
                if (product == null) continue;

                // add to results
                result.Add(new BoughtProducts(client, groceryList, product));
            }

            return result;
        }
    }
}
