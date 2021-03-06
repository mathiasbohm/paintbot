﻿// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK Github:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;

using Moq;
using Autofac;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    public abstract class PromptTests_Base : DialogTestBase
    {
        public interface IPromptCaller<T> : IDialog<object>
        {
            Task FirstMessage(IDialogContext context, IAwaitable<Connector.Message> message);
            Task PromptResult(IDialogContext context, IAwaitable<T> result);
        }

        public static Mock<IPromptCaller<T>> MockDialog<T>(Action<IDialogContext, ResumeAfter<T>> prompt)
        {
            var dialog = new Moq.Mock<IPromptCaller<T>>(MockBehavior.Strict);
            dialog
                .Setup(d => d.StartAsync(It.IsAny<IDialogContext>()))
                .Returns<IDialogContext>(async c => { c.Wait(dialog.Object.FirstMessage); });
            dialog
                .Setup(d => d.FirstMessage(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<Connector.Message>>()))
                .Returns<IDialogContext, IAwaitable<object>>(async (c, a) => { prompt(c, dialog.Object.PromptResult); });
            dialog
                .Setup(d => d.PromptResult(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<T>>()))
                .Returns<IDialogContext, IAwaitable<T>>(async (c, a) => { c.Done(default(T)); });


            return dialog;
        }
    }


    [TestClass]
    public sealed class PromptTests_Success : PromptTests_Base
    {
        private const string PromptText = "hello there";

        public async Task PromptSuccessAsync<T>(Action<IDialogContext, ResumeAfter<T>> prompt, string text, T expected)
        {
            var dialogRoot = MockDialog<T>(prompt);

            Func<IDialog<object>> MakeRoot = () => dialogRoot.Object;
            var toBot = MakeTestMessage();

            using (new FiberTestBase.ResolveMoqAssembly(dialogRoot.Object))
            using (var container = Build(Options.ScopedQueue, dialogRoot.Object))
            {
                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    DialogModule_MakeRoot.Register(scope, MakeRoot);

                    var task = scope.Resolve<IPostToBot>();

                    await task.PostAsync(toBot, CancellationToken.None);
                    AssertMentions(PromptText, scope);
                }

                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    DialogModule_MakeRoot.Register(scope, MakeRoot);

                    var task = scope.Resolve<IPostToBot>();

                    toBot.Text = text;

                    await task.PostAsync(toBot, CancellationToken.None);
                    AssertNoMessages(scope);
                    dialogRoot.Verify(d => d.PromptResult(It.IsAny<IDialogContext>(), It.Is<IAwaitable<T>>(actual => actual.GetAwaiter().GetResult().Equals(expected))), Times.Once);
                }
            }
        }

        [TestMethod]
        public async Task PromptSuccess_Text()
        {
            await PromptSuccessAsync((context, resume) => PromptDialog.Text(context, resume, PromptText), "lol wut", "lol wut");
        }

        [TestMethod]
        public async Task PromptSuccess_Confirm_Yes()
        {
            await PromptSuccessAsync((context, resume) => PromptDialog.Confirm(context, resume, PromptText), "yes", true);
        }

        [TestMethod]
        public async Task PromptSuccess_Confirm_Yes_CaseInsensitive()
        {
            await PromptSuccessAsync((context, resume) => PromptDialog.Confirm(context, resume, PromptText), "Yes", true);
        }

        [TestMethod]
        public async Task PromptSuccess_Confirm_No()
        {
            await PromptSuccessAsync((context, resume) => PromptDialog.Confirm(context, resume, PromptText), "no", false);
        }

        [TestMethod]
        public async Task PromptSuccess_Confirm_No_CaseInsensitive()
        {
            await PromptSuccessAsync((context, resume) => PromptDialog.Confirm(context, resume, PromptText), "No", false);
        }

        [TestMethod]
        public async Task PromptSuccess_Number_Long()
        {
            await PromptSuccessAsync((context, resume) => PromptDialog.Number(context, resume, PromptText), "42", 42L);
        }

        [TestMethod]
        public async Task PromptSuccess_Number_Double()
        {
            await PromptSuccessAsync((context, resume) => PromptDialog.Number(context, resume, PromptText), "42", 42d);
        }

        [TestMethod]
        public async Task PromptSuccess_Choice()
        {
            var choices = new[] { "one", "two", "three" };
            await PromptSuccessAsync((context, resume) => PromptDialog.Choice(context, resume, choices, PromptText), "two", "two");
        }

        [TestMethod]
        public async Task PromptSuccess_Choice_Overlapping()
        {
            var choices = new[] { "9", "19", "else" };
            await PromptSuccessAsync((context, resume) => PromptDialog.Choice(context, resume, choices, PromptText), "9", "9");
        }

        [TestMethod]
        public async Task PromptSuccess_Choice_Overlapping_Reverse()
        {
            var choices = new[] { "19", "9", "else" };
            await PromptSuccessAsync((context, resume) => PromptDialog.Choice(context, resume, choices, PromptText), "9", "9");
        }
    }

    [TestClass]
    public sealed class PromptTests_Failure : PromptTests_Base
    {
        private const string PromptText = "hello there";
        private const string RetryText = "hello there again";
        private const int MaximumAttempts = 2;

        public async Task PromptFailureAsync<T>(Action<IDialogContext, ResumeAfter<T>> prompt)
        {
            var dialogRoot = MockDialog<T>(prompt);

            Func<IDialog<object>> MakeRoot = () => dialogRoot.Object;
            var toBot = MakeTestMessage();

            using (new FiberTestBase.ResolveMoqAssembly(dialogRoot.Object))
            using (var container = Build(Options.ScopedQueue, dialogRoot.Object))
            {
                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    DialogModule_MakeRoot.Register(scope, MakeRoot);

                    var task = scope.Resolve<IPostToBot>();

                    await task.PostAsync(toBot, CancellationToken.None);
                    AssertMentions(PromptText, scope);
                }

                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    DialogModule_MakeRoot.Register(scope, MakeRoot);

                    var task = scope.Resolve<IPostToBot>();

                    await task.PostAsync(toBot, CancellationToken.None);
                    AssertMentions(RetryText, scope);
                }

                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    DialogModule_MakeRoot.Register(scope, MakeRoot);

                    var task = scope.Resolve<IPostToBot>();

                    await task.PostAsync(toBot, CancellationToken.None);
                    AssertMentions("too many attempts", scope);
                    dialogRoot.Verify(d => d.PromptResult(It.IsAny<IDialogContext>(), It.Is<IAwaitable<T>>(actual => actual.ToTask().IsFaulted)), Times.Once);
                }
            }
        }

        [TestMethod]
        public async Task PromptFailure_Number()
        {
            await PromptFailureAsync<long>((context, resume) => PromptDialog.Number(context, resume, PromptText, RetryText, MaximumAttempts));
        }

        [TestMethod]
        public async Task PromptFailure_Choice()
        {
            var choices = new[] { "one", "two", "three" };
            await PromptFailureAsync<string>((context, resume) => PromptDialog.Choice(context, resume, choices, PromptText, RetryText, MaximumAttempts));
        }

        [TestMethod]
        public async Task PromptFailure_Confirm()
        {
            await PromptFailureAsync<bool>((context, resume) => PromptDialog.Confirm(context, resume, PromptText, RetryText, MaximumAttempts));
        }
    }
}
