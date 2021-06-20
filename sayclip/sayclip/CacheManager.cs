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

        public async Task saveTranslationEntry(Translation entry)
        {
            
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
