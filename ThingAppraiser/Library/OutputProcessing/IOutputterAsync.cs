﻿using System.Collections.Generic;
using System.Threading.Tasks;
using ThingAppraiser.Models.Internal;

namespace ThingAppraiser.IO.Output
{
    public interface IOutputterAsync : IOutputterBase, ITagable
    {
        Task<bool> SaveResults(List<List<RatingDataContainer>> results, string storageName);
    }
}
