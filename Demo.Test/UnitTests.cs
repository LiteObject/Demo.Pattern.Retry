using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Demo.Test
{
    /* Why unit test?
     * - Less time performing functional tests
     * - Protection against regression
     * - Executable documentation
     * - Less coupled code
     * 
     * Characteristics of a good unit test
     * - Fast, Isolated, Repeatable, Self-Checking, Timely
     * 
     * Naming your tests
     * - The name of the method being tested.
     * - The scenario under which it's being tested.
     * - The expected behavior when the scenario is invoked.
     * 
     * Source:
     * https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
     */

    internal class UnitTests
    {
        private readonly ITestOutputHelper _output;

        public UnitTests(ITestOutputHelper output)
        {
            _output = output;
        }

        //[Fact]
        public async Task RetryPolicy_Should_Be_Applied_OnGetCallFailure()
        {
            // Arrange 
            IServiceCollection services = new ServiceCollection();

            services.AddHttpClient("TestClient", c => {
                c.BaseAddress = new Uri("localhost");
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddHttpMessageHandler(() => new StubDelegatingHandler());

            HttpClient client = services.BuildServiceProvider()
                                .GetRequiredService<IHttpClientFactory>()
                                .CreateClient("TestClient");

            // ACT
            var result = await client.GetAsync("/");

            // ASSERT
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        private IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                // .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetryAsync: OnRetryAsync);
        }

        private Task OnRetryAsync(DelegateResult<HttpResponseMessage> arg1, TimeSpan arg2)
        {
            this._output.WriteLine($">>> {nameof(OnRetryAsync)}");
            return Task.CompletedTask;
        }
    }

    public class StubDelegatingHandler : DelegatingHandler
    {
        private int _count = 0;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_count == 0)
            {
                _count++;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
