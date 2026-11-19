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

namespace Localization.Samples.VocabularyServiceProvider
{
    using System.IO;
    using Mafesoft.Localization.Json;

    /// <summary>
    /// The JSON vocabularies of the samples (Resources/it-IT.json and Resources/en-US.json), read with
    /// Mafesoft.Localization.Json.
    /// </summary>
    public static class SampleVocabularies
    {
        /// <summary>
        /// The files embedded in the sample (MAUI, WPF, Blazor WebAssembly): see the EmbeddedResource in the .projitems file.
        /// </summary>
        public static JsonVocabularyServiceProvider Embedded() =>
            JsonVocabularyServiceProvider.FromEmbeddedResources(typeof(SampleVocabularies).Assembly, "Resources");

        /// <summary>
        /// The files in the source folder of the samples (ASP.NET Core in development): a saved change to a file updates the
        /// running app, and the keys missing in a vocabulary are written to obj/missing-terms.
        /// </summary>
        /// <param name="contentRoot">The content root of the sample, for example builder.Environment.ContentRootPath.</param>
        public static JsonVocabularyServiceProvider FromSourceFolder(string contentRoot) =>
            JsonVocabularyServiceProvider.FromDirectory(
                Path.Combine(contentRoot, "..", "Localization.Samples.VocabularyServiceProvider", "Resources"),
                new JsonVocabularyOptions
                {
                    ReloadOnChange = true,
                    MissingTermsDirectory = Path.Combine(contentRoot, "obj", "missing-terms"),
                });
    }
}
