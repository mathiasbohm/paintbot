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

using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using System.IO;

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    public abstract class BotDataBase<T> : IBotData
    {
        protected readonly Message message;

        public BotDataBase(Message message)
        {
            SetField.NotNull(out this.message, nameof(BotDataBase<T>.message), message);
        }

        protected abstract T MakeData();
        protected abstract IBotDataBag WrapData(T data);

        IBotDataBag IBotData.ConversationData
        {
            get
            {
                var data = (T)this.message.BotConversationData;
                if (data == null)
                {
                    data = this.MakeData();
                    this.message.BotConversationData = data;
                }

                return this.WrapData(data);
            }
        }

        IBotDataBag IBotData.PerUserInConversationData
        {
            get
            {
                var data = (T)this.message.BotPerUserInConversationData;
                if (data == null)
                {
                    data = this.MakeData();
                    this.message.BotPerUserInConversationData = data;
                }

                return this.WrapData(data);
            }
        }

        IBotDataBag IBotData.UserData
        {
            get
            {
                var data = (T)this.message.BotUserData;
                if (data == null)
                {
                    data = this.MakeData();
                    this.message.BotUserData = data;
                }

                return this.WrapData(data);
            }
        }
    }

    public sealed class DictionaryBotData : BotDataBase<Dictionary<string, object>>
    {
        public DictionaryBotData(Message message)
            : base(message)
        {
        }

        protected override Dictionary<string, object> MakeData()
        {
            return new Dictionary<string, object>();
        }

        private sealed class Bag : IBotDataBag
        {
            private readonly Dictionary<string, object> bag;
            public Bag(Dictionary<string, object> bag)
            {
                SetField.NotNull(out this.bag, nameof(bag), bag);
            }

            int IBotDataBag.Count { get { return this.bag.Count; } }

            void IBotDataBag.SetValue<T>(string key, T value)
            {
                this.bag[key] = value;
            }

            bool IBotDataBag.TryGetValue<T>(string key, out T value)
            {
                object boxed;
                bool found = this.bag.TryGetValue(key, out boxed);
                if (found)
                {
                    if (boxed is T)
                    {
                        value = (T)boxed;
                        return true;
                    }
                }

                value = default(T);
                return false;
            }

            bool IBotDataBag.RemoveValue(string key)
            {
                return this.bag.Remove(key);
            }
        }

        protected override IBotDataBag WrapData(Dictionary<string, object> data)
        {
            return new Bag(data);
        }
    }

    public sealed class JObjectBotData : BotDataBase<JObject>
    {
        public JObjectBotData(Message message)
            : base(message)
        {
        }

        protected override JObject MakeData()
        {
            return new JObject();
        }
        private sealed class Bag : IBotDataBag
        {
            private readonly JObject bag;
            public Bag(JObject bag)
            {
                SetField.NotNull(out this.bag, nameof(bag), bag);
            }

            int IBotDataBag.Count { get { return this.bag.Count; } }

            void IBotDataBag.SetValue<T>(string key, T value)
            {
                var token = JToken.FromObject(value);
#if DEBUG
                var copy = token.ToObject<T>();
#endif
                this.bag[key] = token;
            }

            bool IBotDataBag.TryGetValue<T>(string key, out T value)
            {
                JToken token;
                bool found = this.bag.TryGetValue(key, out token);
                if (found)
                {
                    value = token.ToObject<T>();
                    return true;
                }

                value = default(T);
                return false;
            }

            bool IBotDataBag.RemoveValue(string key)
            {
                return this.bag.Remove(key);
            }
        }

        protected override IBotDataBag WrapData(JObject data)
        {
            return new Bag(data);
        }
    }

    public sealed class BotDataBagStream : MemoryStream
    {
        private readonly IBotDataBag bag;
        private readonly string key;
        public BotDataBagStream(IBotDataBag bag, string key)
        {
            SetField.NotNull(out this.bag, nameof(bag), bag);
            SetField.NotNull(out this.key, nameof(key), key);

            byte[] blob;
            if (this.bag.TryGetValue(key, out blob))
            {
                this.Write(blob, 0, blob.Length);
                this.Position = 0;
            }
        }

        public override void Flush()
        {
            base.Flush();

            var blob = this.ToArray();
            this.bag.SetValue(this.key, blob);
        }

        public override void Close()
        {
            this.Flush();
            base.Close();
        }
    }
}
