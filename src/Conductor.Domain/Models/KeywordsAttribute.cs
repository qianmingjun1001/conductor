using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace Conductor.Domain.Models
{
    /// <summary>
    /// 用于标识关键字
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class KeywordsAttribute : Attribute
    {
        public KeywordsAttribute([NotNull] string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
        }

        public string Name { get; }
    }
}