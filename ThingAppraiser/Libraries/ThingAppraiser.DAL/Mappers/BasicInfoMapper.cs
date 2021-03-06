﻿using System.Data;
using ThingAppraiser.Models.Data;

namespace ThingAppraiser.DAL.Mappers
{
    public sealed class BasicInfoMapper : IMapper<BasicInfo>
    {
        public BasicInfoMapper()
        {
        }

        #region IMapper<BasicInfo> Implementation

        public BasicInfo ReadItem(IDataReader reader)
        {
            return new BasicInfo(
                thingId:     (int)    reader["thing_id"],
                title:       (string) reader["title"],
                voteCount:   (int)    reader["vote_count"],
                voteAverage: (double) reader["vote_average"]
            );
        }

        #endregion
    }
}
