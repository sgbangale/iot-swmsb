using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using SWMSB.COMMON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace SWMSB.PROVIDERS
{
    public class DocumentResult<TDocument> where TDocument : class
    {
        public IList<TDocument> Data { get; set; }

        public string NextToken { get; set; }

        public string PreviousToken { get; set; }
    }

    public class DocumentProvider<TDocument> where TDocument : class
    {
        protected internal string CollectionName { get; set; }

        protected string DatabaseName { get; }

        private static string DocumentDbEndPoint { get; set; }

        private static string DocumentDbAuthKey { get; set; }

        protected static ConnectionPolicy ConnectionPolicy;


        private readonly JsonSerializerSettings serializerSettings =
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
        public DocumentProvider(DocumentDbSecret documentDbSecret)
        {
            CollectionName = documentDbSecret.DocumentCollectionName;
            DatabaseName = documentDbSecret.DocumentDbName;
            ConnectionPolicy = new ConnectionPolicy
            {
                ConnectionMode = ConnectionMode.Gateway,

                ConnectionProtocol = Protocol.Https,
                RequestTimeout = new TimeSpan(0, 1, 0),
                MaxConnectionLimit = 1000,
                RetryOptions = new RetryOptions
                {
                    MaxRetryAttemptsOnThrottledRequests = 3,
                    MaxRetryWaitTimeInSeconds = 15
                }
            };
            DocumentDbEndPoint = documentDbSecret.DocumentDbEndpointUrl;
            DocumentDbAuthKey = documentDbSecret.DocumentDbAuthorizationKey;
        }
        internal static DocumentClient Client => new DocumentClient(new Uri(DocumentDbEndPoint),
                                                      DocumentDbAuthKey,
                                                      ConnectionPolicy, ConsistencyLevel.Eventual);
        private Uri GetCollectionInternal()
        {
            return UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName);
        }

        private Uri GetDocumentUriInternal(string id)
        {
            return UriFactory.CreateDocumentUri(DatabaseName, CollectionName, id);
        }
        public async Task<TDocument> AddOrUpdateAsync(TDocument entity)
        {
            var task = Client.UpsertDocumentAsync(GetCollectionInternal(), entity, null, true);
            if (task == null) return null;
            var response = await task;
            return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted ||
                   response.StatusCode == HttpStatusCode.Created
                ? JsonConvert.DeserializeObject<TDocument>(response.Resource.ToString(), serializerSettings)
                : null;
        }
        private async Task<Document> GetDocumentByIdInternalAsync(string id)
        {
            try
            {
                var requestOptions = new RequestOptions
                {
                    PartitionKey = new PartitionKey(nameof(TDocument))
                };
                var response = await Client.ReadDocumentAsync<Microsoft.Azure.Documents.Document>(GetDocumentUriInternal(id), requestOptions);
                return response.StatusCode == HttpStatusCode.OK ? response.Document : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> RemoveAsync(string id)
        {
            var requestOptions = new RequestOptions
            {
                PartitionKey = new PartitionKey(nameof(TDocument))
            };
            var document = await GetDocumentByIdInternalAsync(id);
            if (document == null) return false;
            var task = Client.DeleteDocumentAsync(document.SelfLink, requestOptions);
            if (task == null) return false;
            var result = await task;
            return result != null && result.StatusCode == HttpStatusCode.NoContent;
        }
        public async Task<TDocument> GetDocumentByIdAsync(string id)
        {
            var response = await GetDocumentByIdInternalAsync(id);
            return response != null ? (TDocument)(dynamic)response : null;
        }

        protected IQueryable<TDocument> GetDocumentsWithWhere(Expression<Func<TDocument, bool>> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            var options = new FeedOptions
            {
                EnableCrossPartitionQuery = true,
                MaxDegreeOfParallelism = -1,
                MaxItemCount = 200
            };
            var document = Client?.CreateDocumentQuery<TDocument>(GetCollectionInternal(), options);
            return document?.Where(predicate);
        }

        protected internal async Task<List<TDocument>> GetDocumentsAsync()
        {
            var requestOptions = new RequestOptions
            {
                PartitionKey = new PartitionKey(nameof(TDocument))
            };
            var options = new FeedOptions
            {
                MaxItemCount = 100,
                PartitionKey = requestOptions.PartitionKey
            };
            var result = Client.CreateDocumentQuery<TDocument>(GetCollectionInternal(), options).ToList();
            return await Task.FromResult(result);
        }

        public async Task<DocumentResult<TDocument>> GetDocumentsWithContinuationAsync(int pageSize,
            string continuationToken,
            Expression<Func<TDocument, bool>> filterOptions,
            Expression<Func<TDocument, string>> sortOptions)
        {
            var requestOptions =  new RequestOptions
            {
                PartitionKey = new PartitionKey(nameof(TDocument))
            };
            var options = new FeedOptions
            {
                MaxItemCount = pageSize,
                RequestContinuation = continuationToken,
                PartitionKey = requestOptions?.PartitionKey,
            };
            options.RequestContinuation = continuationToken;
            string queryContinuationToken = continuationToken ?? string.Empty;

            IQueryable<TDocument> query;
            if (filterOptions != null)
                query = Client.CreateDocumentQuery<TDocument>(GetCollectionInternal(), options).Where(filterOptions);
            else
                query = Client.CreateDocumentQuery<TDocument>(GetCollectionInternal(), options);

            if (sortOptions != null)
                query = query.OrderBy(sortOptions);
            var requestQuery = query.AsDocumentQuery();
            var page = await requestQuery.ExecuteNextAsync<TDocument>();
            var result = page.ToList();
            if (requestQuery.HasMoreResults)
                queryContinuationToken = page.ResponseContinuation;
            return new DocumentResult<TDocument> { Data = result, NextToken = queryContinuationToken, PreviousToken = continuationToken };
        }
    }
}
