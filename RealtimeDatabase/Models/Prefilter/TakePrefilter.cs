﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealtimeDatabase.Models.Prefilter
{
    class TakePrefilter : IPrefilter
    {
        public int Number { get; set; }

        public IEnumerable<object> Execute(IEnumerable<object> array)
        {
            return array.Take(Number);
        }
    }
}