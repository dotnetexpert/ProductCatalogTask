namespace Products.Api.Contracts.Common
{
    public class GridQuery
    {
        public Dictionary<string, string>? Filter { get; set; }
        public string? Sort { get; set; }
        public bool Ascending { get; set; }

        private int _page = 1;

        public int Page
        {
            get => _page <= 0 ? 1 : _page;
            set => _page = value;
        }

        private int _pageSize = 10;

        public int PageSize
        {
            get
            {
                // If page size is set to MaxValue from frontend (dropdown mode), allow high limit
                if (_pageSize == int.MaxValue)
                    return int.MaxValue;

                return _pageSize <= 0 ? 10 : _pageSize;
            }
            set => _pageSize = value;
        }

        //public int PageSize
        //{
        //    get => _pageSize <= 0 ? 10 : _pageSize;
        //    set => _pageSize = value;
        //}
    }
}
