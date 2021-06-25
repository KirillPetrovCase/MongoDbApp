using MongoDbApp.Models;
using System.Collections.Generic;

namespace MongoDbApp.ViewModels
{
    public class IndexViewModel
    {
        public FilterViewModel Filter { get; set; }
        public IEnumerable<Product> Products { get; set; }

        public IndexViewModel(FilterViewModel filter, IEnumerable<Product> products)
        {
            Filter = filter;
            Products = products;
        }
    }
}
