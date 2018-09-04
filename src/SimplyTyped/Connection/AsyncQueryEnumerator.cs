using System;
using System.Threading.Tasks;
using Amazon.SimpleDB.Model;
using SimplyTyped.Core;
using SimplyTyped.Core.Query;

namespace SimplyTyped.Query
{
    internal class AsyncQueryEnumerator<T> : IAsyncQueryEnumerator<T> where T : new()
    {
        private Domain<T> _domain;
        private SelectRequest _req;
        private T[] _currentPage;
        private int _currentIndex;

        private bool _hasMorePages = true;

        public AsyncQueryEnumerator(Domain<T> domain, SelectRequest req)
        {
            _domain = domain;
            _req = req;
        }

        public Task<T> Current
        {
            get
            {
                if (_currentPage == null || _currentPage.Length == 0 || _currentIndex > _currentPage.Length - 1)
                    throw new InvalidOperationException("The current state of the enumerator is not valid");
                return Task.FromResult(_currentPage[_currentIndex]);
            }
        }


        public async Task<bool> MoveNextAsync()
        {
            // already have a page loaded, and we have not reached the end yet
            if (_currentPage != null && _currentIndex < _currentPage.Length - 1)
            {
                _currentIndex++;
                return true;
            }

            // we have already fetched the last page, and completed reading it, so we're done 
            if (!_hasMorePages)
                return false;

            //we fetch a page, and return true if it is not empty
            await FetchNextPage();
            return _currentPage.Length > 0;
        }

        private async Task FetchNextPage()
        {
            var page = await _domain.SelectBatchAsync(_req);
            _hasMorePages = !string.IsNullOrEmpty(page.NextToken);
            _req.NextToken = page.NextToken;
            _currentPage = page.Data;
            _currentIndex = 0;
        }

        internal class SelectPage<T1>
        {
            public T1[] Data { get; set; }
            public string NextToken { get; set; }
        }
    }
}