﻿using OpenConstructionSet.Models;
using System.Collections.Generic;

namespace OpenConstructionSet.Collections
{
    public class ReferenceCollection : OcsRefDictionary<OcsDictionary<ReferenceValues>>
    {
        public ReferenceCollection()
        {
        }

        public ReferenceCollection(IDictionary<string, OcsDictionary<ReferenceValues>> items) : base(items)
        {
        }
    }
}
