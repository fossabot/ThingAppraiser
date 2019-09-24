﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace ThingAppraiser.DesktopApp.Models.Toplists
{
    internal sealed class ScoreToplist : ToplistBase
    {
        // TODO: may be need to delete this dictionary later.
        private Dictionary<int, ToplistBlock> _blocks = new Dictionary<int, ToplistBlock>();


        public ScoreToplist(string name, ToplistFormat format)
            : base(name, ToplistType.Score, format)
        {
        }

        #region ToplistBase Overriden Methods

        public override bool AddBlock(ToplistBlock block)
        {
            block.ThrowIfNull(nameof(block));

            int insertIndex = CalculateInsertIndex(block.Number);
            Blocks.Insert(insertIndex, block);
            _blocks.Add(block.Number, block);

            return true;
        }

        public override bool RemoveBlock(ToplistBlock block)
        {
            block.ThrowIfNull(nameof(block));

            bool internalRemove = _blocks.Remove(block.Number);
            bool baseRemove = Blocks.Remove(block);

            if (internalRemove == baseRemove) return internalRemove;

            throw new InvalidOperationException("Removal operation in this and base class has " +
                                                "different results.");
        }

        public override void UpdateBlocks(IEnumerable<ToplistBlock> blocks)
        {
            blocks.ThrowIfNull(nameof(blocks));

            base.UpdateBlocks(blocks);

            _blocks = blocks.ToDictionary(block => block.Number, block => block);
        }

        #endregion

        private int CalculateInsertIndex(int blockNumber)
        {
            int insertIndex = Format switch
            {
                ToplistFormat.Forward => blockNumber - 1,

                ToplistFormat.Reverse => Blocks.Count - blockNumber + 1,

                _ => throw new InvalidOperationException(
                         $"Unknown toplist format: '{Format.ToString()}'."
                     )
            };

            // Additional checks to be sure that index is not out of range.
            if (insertIndex < 0)
            {
                insertIndex = 0;
            }
            if (insertIndex > Blocks.Count)
            {
                insertIndex = Blocks.Count;
            }

            return insertIndex;
        }
    }
}