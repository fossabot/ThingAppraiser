﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using Acolyte.Assertions;
using Acolyte.Xml;
using Prism.Mvvm;

namespace ThingAppraiser.DesktopApp.Models.Toplists
{
    internal abstract class ToplistBase : BindableBase
    {
        private string _name = default!; // Initializes throught property.
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value.ThrowIfNullOrEmpty(nameof(value)));
        }

        public ToplistType Type { get; }

        public ToplistFormat Format { get; }

        public ObservableCollection<ToplistBlock> Blocks { get; private set; }
            = new ObservableCollection<ToplistBlock>();


        protected ToplistBase(string name, ToplistType type, ToplistFormat format)
        {
            Name = name.ThrowIfNullOrWhiteSpace(nameof(name));
            Type = type;
            Format = format;
        }

        public static string Serialize(ToplistBase toplist)
        {
            toplist.ThrowIfNull(nameof(toplist));

            var toplistXml = new ToplistXml(toplist);
            return XmlHelper.SerializeToStringXml(toplistXml);
        }

        public static ToplistXml Desirialize(string toplistData)
        {
            toplistData.ThrowIfNull(nameof(toplistData));

            return XmlHelper.DeserializeFromStringXml<ToplistXml>(toplistData);
        }

        public abstract bool AddBlock(ToplistBlock block);

        public abstract bool RemoveBlock(ToplistBlock block);

        public virtual void UpdateBlocks(IEnumerable<ToplistBlock> blocks)
        {
            blocks.ThrowIfNull(nameof(blocks));

            Blocks = new ObservableCollection<ToplistBlock>(blocks);
        }
    }
}
