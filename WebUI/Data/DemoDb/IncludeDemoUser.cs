﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SapphireDb.Attributes;

namespace WebUI.Data.DemoDb
{
    public class IncludeDemoUser
    {
        [Key]
        public Guid Id { get; set; }

        [Updatable]
        public List<IncludeDemoUserEntry> Entries { get; set; }

        [Updatable]
        public string Name { get; set; }
    }
}
