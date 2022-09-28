using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;

namespace Rust.Language.Server.Client
{
    /// <summary>
    /// Exists as an abstract implementor of <see cref="ILanguageClient" /> purely to manage the non-standard async
    /// event handlers.
    /// </summary>
    /// 
    [Export(typeof(ILanguageClient)), ContentType("rs")]
    public class LanguageClient : ILanguageClient
    {
        /// <summary>
        /// The language client name (displayed to the user).
        /// </summary>
        /// 
        public string Name => "rust-analyzer";

        /// <summary>
        /// Unused, implementing <see cref="ILanguageClient"/>
        /// No additional settings are provided for this server, so we do not need any configuration section names.
        /// </summary>
        /// 
        public IEnumerable<string> ConfigurationSections => null;

        /// <summary>
        /// Unused, implementing <see cref="ILanguageClient"/>
        /// We do not provide any additional initialization options.
        /// </summary>
        /// 
        public object InitializationOptions => null;

        /// <summary>
        /// Unused, implementing <see cref="ILanguageClient"/>
        /// Files that we care about are already provided and watched by the workspace.
        /// </summary>
        /// 
        public IEnumerable<string> FilesToWatch => null;

        /// <summary>
        /// Failures are catastrophic as user will not have language features without this server.
        /// </summary>
        /// 
        public bool ShowNotificationOnInitializeFailed => true;
        
        /// <summary>
        /// Represents an asynchronous event handler.
        /// </summary>
        /// 
        public event AsyncEventHandler<EventArgs> StartAsync;

#pragma warning disable 67 // The event 'LanguageClient.StopAsync' is never used
        /// <summary>
        /// Unused, implementing <see cref="ILanguageClient"/>
        /// </summary>
        /// 
        public event AsyncEventHandler<EventArgs> StopAsync;
#pragma warning restore 67

        /// <summary>
        /// Start semantic features like completion or goto definition by talking to an external language server process.
        /// </summary>
        /// 
        public async Task<Connection> ActivateAsync(CancellationToken token)
        {
            await Task.Yield();

            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.rustup\\toolchains\\nightly-x86_64-pc-windows-msvc\\bin\\rust-analyzer.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process
            {
                StartInfo = info
            };

            if (process.Start())
                return new Connection(process.StandardOutput.BaseStream, process.StandardInput.BaseStream);

            return null;
        }

        /// <summary>
        /// Signals that the extension has been loaded. The server can be started immediately, or wait for user action to start.  
        /// To start the server, invoke the <see cref="StartAsync"/> event;
        /// </summary>
        /// 
        public async Task OnLoadedAsync() => await StartAsync.InvokeAsync(this, EventArgs.Empty).ConfigureAwait(false);

        /// <summary>
        /// Signals the extension that the language server has been successfully initialized.
        /// </summary>
        /// <returns>A <see cref="Task"/> which completes when actions that need to be performed when the server is ready are done.</returns>
        /// 
        public Task OnServerInitializedAsync() => Task.CompletedTask;

        /// <summary>
        /// Signals the extension that the language server failed to initialize.
        /// </summary>
        /// <returns>A <see cref="Task"/> which completes when additional actions that need to be performed when the server fails to initialize are done.</returns>
        /// 
        public Task<InitializationFailureContext> OnServerInitializeFailedAsync(ILanguageClientInitializationInfo initializationState) => Task.FromResult(new InitializationFailureContext() { FailureMessage = initializationState.InitializationException?.ToString() ?? "null" });
    }
}
