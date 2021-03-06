﻿/*--------------------------------------------------------*\
|                                                          |
|                          hprose                          |
|                                                          |
| Official WebSite: https://hprose.com                     |
|                                                          |
|  Log.cs                                                  |
|                                                          |
|  Log plugin for C#.                                      |
|                                                          |
|  LastModified: Feb 8, 2019                               |
|  Author: Ma Bingyao <andot@hprose.com>                   |
|                                                          |
\*________________________________________________________*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hprose.RPC.Plugins.Log {
    public class Log {
        private static readonly Log instance = new Log();
        public bool Enabled { get; set; }
        public Log(bool enabled = true) {
            Enabled = enabled;
        }
        public static Task<Stream> IOHandler(Stream request, Context context, NextIOHandler next) {
            return LogExtensions.IOHandler(instance, request, context, next);
        }
        public static Task<object> InvokeHandler(string name, object[] args, Context context, NextInvokeHandler next) {
            return LogExtensions.InvokeHandler(instance, name, args, context, next);
        }
    }
    public static class LogExtensions {
        private static string ToString(MemoryStream stream) {
            var data = stream.GetArraySegment();
            try {
                return Encoding.UTF8.GetString(data.Array, data.Offset, data.Count);
            }
            catch {
                return Encoding.Default.GetString(data.Array, data.Offset, data.Count);
            }
        }
        private static string Stringify(object obj) {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }
        public static async Task<Stream> IOHandler(this Log log, Stream request, Context context, NextIOHandler next) {
            bool enabled = context.Contains("Log") ? (context as dynamic).Log : log.Enabled;
            if (!enabled) return await next(request, context).ConfigureAwait(false);
            var stream = await request.ToMemoryStream().ConfigureAwait(false);
            Trace.TraceInformation(ToString(stream));
            try {
                var response = await next(stream, context).ConfigureAwait(false);
                stream = await response.ToMemoryStream().ConfigureAwait(false);
                Trace.TraceInformation(ToString(stream));
                return stream;
            }
            catch (Exception e) {
                Trace.TraceError(e.StackTrace);
                throw;
            }
        }
        public static async Task<object> InvokeHandler(this Log log, string name, object[] args, Context context, NextInvokeHandler next) {
            bool enabled = context.Contains("Log") ? (context as dynamic).Log : log.Enabled;
            if (!enabled) return await next(name, args, context).ConfigureAwait(false);
            string a = "";
            try {
                a = (args.Length > 0) && typeof(Context).IsAssignableFrom(args.Last().GetType()) ? Stringify(new List<object>(args.Take(args.Length - 1))) : Stringify(args);
            }
            catch (Exception e) {
                Trace.TraceError(e.StackTrace);
            }
            try {
                var result = await next(name, args, context).ConfigureAwait(false);
                try {
                    Trace.TraceInformation(name + "(" + a.Substring(1, a.Length - 2) + ") = " + Stringify(result));
                }
                catch (Exception e) {
                    Trace.TraceError(e.StackTrace);
                }
                return result;
            }
            catch (Exception e) {
                Trace.TraceError(e.StackTrace);
                throw;
            }
        }
    }
}
