//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Collections;
using System.Management;
using System.Management.Automation;
using WmBridge.Web.Model;

namespace WmBridge.Web
{
    public static class PSPropertyExtensions
    {
        public static PSPropertySelector ToPropertySelector(this string source)
        {
            return new PSPropertySelector() { PSPropertyName = source, Alias = source, Transformation = (_ => _) };
        }

        public static PSPropertySelector Alias(this string source, string alias)
        {
            return source.ToPropertySelector().Alias(alias);
        }

        public static PSPropertySelector Transform(this string source, Func<object, object> transformation)
        {
            return source.ToPropertySelector().Transform(transformation);
        }

        public static PSPropertySelector Expression(this string source, string expression)
        {
            return source.ToPropertySelector().Expression(expression);
        }

        public static PSPropertySelector Alias(this PSPropertySelector source, string alias)
        {
            source.Alias = alias;
            return source;
        }

        public static PSPropertySelector Transform(this PSPropertySelector source, Func<object, object> transformation)
        {
            source.Transformation = transformation;
            return source;
        }

        public static PSPropertySelector Expression(this PSPropertySelector source, string expression)
        {
            source.PSExpression = expression;
            return source;
        }

        public static PSPropertySelector As<T>(this string source)
        {
            return source.ToPropertySelector().As<T>();
        }

        public static PSPropertySelector As<T>(this PSPropertySelector source)
        {
            return source.Expression(string.Format("[{0}]($_.{1})", typeof(T).FullName, source));
        }

        public static PSPropertySelector TransformArray(this PSPropertySelector source)
        {
            return source.Transform(x => (x != null) ? (ArrayList)((PSObject)x).ImmediateBaseObject : new ArrayList());
        }

        public static PSPropertySelector TransformDmtfDate(this PSPropertySelector source)
        {
            return source.Transform(x => (x != null) ? (object)new DateTime((long)(Convert.ToInt64(ManagementDateTimeConverter.ToDateTime(x.ToString()).ToUniversalTime().Ticks) / TimeSpan.TicksPerSecond) * TimeSpan.TicksPerSecond, DateTimeKind.Utc) : null);
        }

        public static PSPropertySelector TransformTruncUtcTicks(this PSPropertySelector source)
        {
            return source.Transform(x => x != null ? (object)new DateTime((long)(Convert.ToInt64(x) / TimeSpan.TicksPerSecond) * TimeSpan.TicksPerSecond, DateTimeKind.Utc) : null);
        }

        public static PSPropertySelector TransformArray(this string source)
        {
            return source.ToPropertySelector().TransformArray();
        }

        public static PSPropertySelector TransformDmtfDate(this string source)
        {
            return source.ToPropertySelector().TransformDmtfDate();
        }

        public static PSPropertySelector TransformTruncUtcTicks(this string source)
        {
            return source.ToPropertySelector().TransformTruncUtcTicks();
        }

    }
}
