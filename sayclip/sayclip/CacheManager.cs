using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace sayclip
{
    public sealed class CacheManager : IDisposable
    {
        private LiteDatabase db;
        private ILiteCollection<Translation> translationsCollection;
        private bool disposedValue;

        public CacheManager()
        {
            this.db = new LiteDatabase(ConfigurationManager.getInstance.cacheFileLocation);
            this.translationsCollection = db.GetCollection<Translation>("translations");
        }

        public async   Task saveTranslationEntry(Translation entry)
        {
            this.translationsCollection.Insert(entry);
            this.translationsCollection.EnsureIndex(x => x.sourceText);
            this.translationsCollection.EnsureIndex(x => x.fromLanguage);
            this.translationsCollection.EnsureIndex(x => x.toLanguage);
            this.translationsCollection.EnsureIndex(x => x.plugin);
        }

        public async Task<Translation> getTranslationEntry(Translation filters)
        {
            Translation result;
            ILiteQueryable<Translation> query = this.translationsCollection.Query().Where(x => x.sourceText == filters.sourceText)
                .Where(x => x.fromLanguage == filters.fromLanguage && x.toLanguage == filters.toLanguage);
            if(!ConfigurationManager.getInstance.useGlobalCache)
            {
                query = query.Where(x => x.plugin == filters.plugin);
            }
            result = query.FirstOrDefault();
            return (result);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    db?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                translationsCollection = null;
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~CacheManager()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
