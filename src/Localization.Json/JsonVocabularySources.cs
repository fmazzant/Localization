//
//
// The MIT License (MIT)
//
// Copyright (c) 2026 Federico Mazzanti
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
//

namespace Mafesoft.Localization.Json
{
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Primitives;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Where the vocabulary files are read: embedded resources or an <see cref="IFileProvider"/>.
    /// </summary>
    internal abstract class JsonVocabularySource : IDisposable
    {
        /// <summary>
        /// Opens the file, or returns null when it does not exist.
        /// </summary>
        public abstract Stream? Open(string fileName);

        /// <summary>
        /// A token that changes when the file changes, or null when the source cannot watch files.
        /// </summary>
        public abstract IChangeToken? Watch(string fileName);

        public virtual void Dispose() { }
    }

    /// <summary>
    /// Embedded resources of an assembly: <c>Resources/it-IT.json</c> is the resource <c>RootNamespace.Resources.it-IT.json</c>.
    /// </summary>
    internal sealed class EmbeddedJsonVocabularySource : JsonVocabularySource
    {
        private readonly Assembly assembly;
        private readonly string prefix;
        private readonly string[] names;

        public EmbeddedJsonVocabularySource(Assembly assembly, string folder)
        {
            this.assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            if (folder is null) throw new ArgumentNullException(nameof(folder));

            // The build turns the folder separators of the resource names into dots.
            var dotted = folder.Replace('\\', '.').Replace('/', '.').Trim('.');
            prefix = dotted.Length == 0 ? string.Empty : dotted + ".";
            names = assembly.GetManifestResourceNames();
        }

        public override Stream? Open(string fileName)
        {
            var suffix = prefix + fileName;
            var name = names.FirstOrDefault(n => string.Equals(n, suffix, StringComparison.OrdinalIgnoreCase))
                ?? names.FirstOrDefault(n => n.EndsWith("." + suffix, StringComparison.OrdinalIgnoreCase));
            return name is null ? null : assembly.GetManifestResourceStream(name);
        }

        public override IChangeToken? Watch(string fileName) => null;
    }

    /// <summary>
    /// The files of an <see cref="IFileProvider"/> (a directory, or the content root of ASP.NET Core).
    /// </summary>
    internal sealed class FileProviderJsonVocabularySource : JsonVocabularySource
    {
        private readonly IFileProvider fileProvider;
        private readonly string folder;
        private readonly bool ownsFileProvider;

        public FileProviderJsonVocabularySource(IFileProvider fileProvider, string folder, bool ownsFileProvider)
        {
            this.fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
            if (folder is null) throw new ArgumentNullException(nameof(folder));
            this.folder = folder.Replace('\\', '/').Trim('/');
            this.ownsFileProvider = ownsFileProvider;
        }

        private string PathOf(string fileName) => folder.Length == 0 ? fileName : folder + "/" + fileName;

        public override Stream? Open(string fileName)
        {
            var file = fileProvider.GetFileInfo(PathOf(fileName));
            return file.Exists && !file.IsDirectory ? file.CreateReadStream() : null;
        }

        public override IChangeToken? Watch(string fileName)
        {
            var token = fileProvider.Watch(PathOf(fileName));
            return token is NullChangeToken ? null : token;
        }

        public override void Dispose()
        {
            if (ownsFileProvider)
            {
                (fileProvider as IDisposable)?.Dispose();
            }
        }
    }
}
