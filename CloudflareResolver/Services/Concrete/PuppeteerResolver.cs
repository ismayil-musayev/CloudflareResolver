using CloudflareResolver.Helpers;
using CloudflareResolver.Models;
using Microsoft.Extensions.Logging;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.AnonymizeUa;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudflareResolver.Services.Concrete
{
    public class PuppeteerResolver : IResolver, IAsyncDisposable, IDisposable
    {
        private readonly object lockObject = new();
        private readonly ILogger _logger;
        private Browser _browser;

        public PuppeteerResolver(ILogger<PuppeteerResolver> logger)
        {
            _logger = logger;
        }

        public async Task<ResolverResponse> Resolve(ResolverRequest request)
        {
            var browser = await GetBrowser().ConfigureAwait(false);
            await using var page = await browser.NewPageAsync().ConfigureAwait(false);
            _logger.LogInformation($"New page created for url {page.Url}");

            page.DefaultTimeout = page.DefaultNavigationTimeout = 60000;

            if (!string.IsNullOrWhiteSpace(request.UserAgent))
            {
                await page.SetUserAgentAsync(request.UserAgent).ConfigureAwait(false);
            }

            _logger.LogInformation($"Going to url {request.Url}");
            await page.GoToAsync(request.Url).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(request.SelectorForWait))
            {
                _logger.LogInformation($"Waiting for selector {request.SelectorForWait}");
                await page.WaitForSelectorAsync(request.SelectorForWait).ConfigureAwait(false);
            }
            else
            {
                _logger.LogInformation($"Waiting for navigation");
                await page.WaitForNavigationAsync().ConfigureAwait(false);
            }

            var cookieParams = await page.GetCookiesAsync().ConfigureAwait(false);
            _logger.LogInformation($"Cookies count for url {request.Url} is {cookieParams.Length}");

            await page.DeleteCookieAsync(cookieParams).ConfigureAwait(false);
            _logger.LogInformation($"Cookies deleted for url {request.Url}");

            await page.CloseAsync().ConfigureAwait(false);

            var response = new ResolverResponse
            {
                Cookies = cookieParams.Select(x => new Cookie
                {
                    Name = x.Name,
                    Value = x.Value
                }).ToList()
            };

            return response;
        }

        private async ValueTask<Browser> GetBrowser()
        {
            if (_browser == null || _browser.IsClosed)
            {
                await InitBrowserSync();
            }
            else
            {
                _logger.LogInformation("Browser exists");
            }

            return _browser;
        }

        private async Task InitBrowserSync()
        {
            bool lockTaken = false;
            try
            {
                Monitor.Enter(lockObject, ref lockTaken);

                if (_browser == null || _browser.IsClosed)
                {
                    var extra = new PuppeteerExtra();
                    extra.Use(new AnonymizeUaPlugin()).Use(new StealthPlugin());

                    await "Xvfb :99 -screen 0 1280x1024x24 -nolisten tcp &".Bash().ConfigureAwait(false);

                    _browser = await extra.LaunchAsync(new LaunchOptions
                    {
                        Headless = false,
                        Args = new[] { "--no-sandbox", "--disable-gpu" },
                        ExecutablePath = "/usr/bin/chromium"
                    }).ConfigureAwait(false);

                    _logger.LogInformation("New browser launched");
                }
                else
                {
                    _logger.LogInformation("Browser exists");
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(lockObject);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();

            Dispose(false);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _logger.LogInformation("Disposed");

            if (disposing)
            {
                _browser?.Dispose();
            }

            _browser = null;
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            _logger.LogInformation("Disposed Async");

            if (_browser is not null)
            {
                await _browser.DisposeAsync().ConfigureAwait(false);
            }

            _browser = null;
        }
    }
}
