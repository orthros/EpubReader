using System;
using System.Collections.Generic;
using ImageSharp;
using System.Linq;
using System.Threading.Tasks;
using VersFx.Formats.Text.Epub.Schema.Opf;
using ImageSharp.Formats;

namespace VersFx.Formats.Text.Epub.Readers
{
    internal static class BookCoverReader
    {
        public static async Task<Image> ReadBookCoverAsync(EpubBookRef bookRef)
        {
            List<EpubMetadataMeta> metaItems = bookRef.Schema.Package.Metadata.MetaItems;
            if (metaItems == null || !metaItems.Any())
                return null;
            EpubMetadataMeta coverMetaItem = metaItems.FirstOrDefault(metaItem => String.Compare(metaItem.Name, "cover", StringComparison.OrdinalIgnoreCase) == 0);
            if (coverMetaItem == null)
                return null;
            if (String.IsNullOrEmpty(coverMetaItem.Content))
                throw new Exception("Incorrect EPUB metadata: cover item content is missing.");
            EpubManifestItem coverManifestItem = bookRef.Schema.Package.Manifest.FirstOrDefault(manifestItem => String.Compare(manifestItem.Id, coverMetaItem.Content, StringComparison.OrdinalIgnoreCase) == 0);
            if (coverManifestItem == null)
                throw new Exception(String.Format("Incorrect EPUB manifest: item with ID = \"{0}\" is missing.", coverMetaItem.Content));
            EpubByteContentFileRef coverImageContentFileRef;
            if (!bookRef.Content.Images.TryGetValue(coverManifestItem.Href, out coverImageContentFileRef))
                throw new Exception(String.Format("Incorrect EPUB manifest: item with href = \"{0}\" is missing.", coverManifestItem.Href));
            byte[] coverImageContent = await coverImageContentFileRef.ReadContentAsBytesAsync().ConfigureAwait(false);

            Configuration imgConfiguration = new Configuration();
            imgConfiguration.AddImageFormat(new GifFormat());
            imgConfiguration.AddImageFormat(new PngFormat());
            imgConfiguration.AddImageFormat(new JpegFormat());
            imgConfiguration.AddImageFormat(new BmpFormat());

            return await Task.Run(() => new Image(coverImageContent, imgConfiguration)).ConfigureAwait(false);
        }
    }
}
