//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;

namespace WmBridge.Web.Model
{
    public class PSPropertySelector
    {
        public string PSPropertyName { get; set; }

        public string Alias { get; set; }

        public Func<object, object> Transformation { get; set; }

        public string PSExpression { get; set; }

        public int? Tag { get; set; }

        public override string ToString()
        {
            return PSPropertyName;
        }
    }
}
