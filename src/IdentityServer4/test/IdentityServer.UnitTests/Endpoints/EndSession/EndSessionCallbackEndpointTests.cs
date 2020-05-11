﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer.UnitTests.Common;
using IdentityServer4.Endpoints;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xunit;

namespace IdentityServer.UnitTests.Endpoints.EndSession
{
    public class EndSessionCallbackEndpointTests
    {
        private const string Category = "End Session Callback Endpoint";

        MockLogoutNotificationService _mockLogoutNotificationService = new MockLogoutNotificationService();
        StubEndSessionRequestValidator _stubEndSessionRequestValidator = new StubEndSessionRequestValidator();
        EndSessionCallbackEndpoint _subject;

        public EndSessionCallbackEndpointTests()
        {
            _subject = new EndSessionCallbackEndpoint(
                _stubEndSessionRequestValidator,
                _mockLogoutNotificationService,
                new LoggerFactory().CreateLogger<EndSessionCallbackEndpoint>());
        }

        [Fact]
        public async Task callback_validation_success_should_invoke_back_channel_clients()
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Method = "GET";

            _stubEndSessionRequestValidator.EndSessionCallbackValidationResult.IsError = false;
            _stubEndSessionRequestValidator.EndSessionCallbackValidationResult.LogoutContext = new LogoutNotificationContext()
            {
                SubjectId = "sub", SessionId = "sid", ClientIds = new[] { "client" }
            };

            var result = await _subject.ProcessAsync(ctx);

            // this is a deliberable hack since we have a fire-and-forget to calling back-channel clients
            await Task.Delay(100);

            _mockLogoutNotificationService.SendBackChannelLogoutNotificationsCalled.Should().BeTrue();
        }

        [Fact]
        public async Task callback_validation_error_should_not_invoke_back_channel_clients()
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Method = "GET";

            _stubEndSessionRequestValidator.EndSessionCallbackValidationResult.IsError = true;

            var result = await _subject.ProcessAsync(ctx);

            // this is a deliberable hack since we have a fire-and-forget to calling back-channel clients
            await Task.Delay(100);

            _mockLogoutNotificationService.SendBackChannelLogoutNotificationsCalled.Should().BeFalse();
        }
    }
}
