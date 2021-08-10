namespace GroceryStoreAPI.Models
{
    public class Customer : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}