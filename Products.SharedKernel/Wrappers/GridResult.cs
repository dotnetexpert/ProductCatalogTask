namespace Products.Api.Contracts.Common
{
    public class GridResult<T>
    {
        public IList<T> Data { get; set; }
        public int Total { get; set; }
        public int Page { get; set; }
    }
}
