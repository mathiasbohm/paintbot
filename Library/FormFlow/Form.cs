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

using Microsoft.Bot.Builder.FormFlow.Advanced;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Resources;

namespace Microsoft.Bot.Builder.FormFlow
{
    internal sealed class Form<T> : IForm<T>
        where T : class
    {
        internal readonly bool _ignoreAnnotations;
        internal readonly FormConfiguration _configuration;
        internal readonly Fields<T> _fields;
        internal readonly List<IStep<T>> _steps;
        internal OnCompletionAsyncDelegate<T> _completion;

        public Form(bool ignoreAnnotations, FormConfiguration configuration = null, Fields<T> fields = null, List<IStep<T>> steps = null, OnCompletionAsyncDelegate<T> completion = null)
        {
            _ignoreAnnotations = ignoreAnnotations;
            _configuration = configuration ?? new FormConfiguration();
            _fields = fields ?? new Fields<T>();
            _steps = steps ?? new List<IStep<T>>();
            _completion = completion;
            _resources = new Localizer() { Culture = CultureInfo.CurrentUICulture};
        }

        internal override ILocalizer Resources
        {
            get
            {
                return _resources;
            }
        }

        public override void SaveResources(IResourceWriter writer)
        {
            _resources = new Localizer() { Culture = CultureInfo.CurrentUICulture };
            foreach (var step in _steps)
            {
                step.SaveResources();
            }
            _resources.Save(writer);
        }

        public override void Localize(IDictionaryEnumerator reader, out IEnumerable<string> missing, out IEnumerable<string> extra)
        {
            _resources = _resources.Load(reader, out missing, out extra);
            foreach (var step in _steps)
            {
                step.Localize();
            }
        }

        internal override bool IgnoreAnnotations
        {
            get
            {
                return _ignoreAnnotations;
            }
        }

        internal override FormConfiguration Configuration
        {
            get
            {
                return _configuration;
            }
        }

        internal override IReadOnlyList<IStep<T>> Steps
        {
            get
            {
                return _steps;
            }
        }

        internal override OnCompletionAsyncDelegate<T> Completion
        {
            get
            {
                return _completion;
            }
        }

        internal override IFields<T> Fields
        {
            get
            {
                return _fields;
            }
        }

        private ILocalizer _resources;
    }
}
