using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace MockLanguageExtension
{
    public class Rust
    {
#pragma warning disable 649
        [Export, Name("rs"), BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
        internal static ContentTypeDefinition RsContentTypeDefinition;

        [Export, FileExtension(".rs"), ContentType("rs")]
        internal static FileExtensionToContentTypeDefinition RsFileExtensionDefinition;
#pragma warning restore 649
    }
}
