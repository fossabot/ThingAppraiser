﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OMDbApiNet.Model;
using ThingAppraiser.Models.Data;

namespace ThingAppraiser.OmdbService.Mappers
{
    public sealed class DataMapperOmdbMovie : IDataMapper<Item, OmdbMovieInfo>
    {
        public DataMapperOmdbMovie()
        {
        }

        #region IDataMapper<Item, OmdbMovieInfo> Implementation

        public OmdbMovieInfo Transform(Item dataObject)
        {
            int thingId = int.Parse(dataObject.ImdbId.Substring(2));
            int voteCount = int.Parse(dataObject.ImdbVotes, NumberStyles.AllowThousands);
            double voteAverage = double.Parse(dataObject.ImdbRating);
            DateTime releaseDate = DateTime.Parse(dataObject.Released);

            int metascore = string.Equals(dataObject.Metascore, "N/A",
                                          StringComparison.InvariantCultureIgnoreCase)
                ? 0
                : int.Parse(dataObject.Metascore);

            List<string> genreIds = dataObject.Genre
                .Split(',')
                .Select(genre => genre.Trim())
                .ToList();

            return new OmdbMovieInfo(
                thingId:     thingId,
                title:       dataObject.Title,
                voteCount:   voteCount,
                voteAverage: voteAverage,
                overview:    dataObject.Plot,
                releaseDate: releaseDate,
                metascore:   metascore,
                rated:       dataObject.Rated,
                genreIds:    genreIds,
                posterPath:  dataObject.Poster
            );
        }

        #endregion
    }
}
