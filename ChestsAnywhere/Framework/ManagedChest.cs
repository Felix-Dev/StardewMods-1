using System.Text.RegularExpressions;
using StardewValley.Objects;

namespace ChestsAnywhere.Framework
{
    /// <summary>A chest with metadata.</summary>
    internal class ManagedChest
    {
        /*********
        ** Properties
        *********/
        /// <summary>A regular expression which matches a group of tags in the chest name.</summary>
        private const string TagGroupPattern = @"\|([^\|]+)\|";


        /*********
        ** Accessors
        *********/
        /// <summary>The chest instance.</summary>
        public Chest Chest { get; }

        /// <summary>The chest's display name.</summary>
        public string Name { get; private set; }

        /// <summary>The category name (if any).</summary>
        public string Category { get; private set; }

        /// <summary>Whether the chest should be ignored.</summary>
        public bool IsIgnored { get; private set; }

        /// <summary>The sort value (if any).</summary>
        public int? Order { get; private set; }

        /// <summary>The name of the location or building which contains the chest.</summary>
        public string LocationName { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="chest">The chest instance.</param>
        /// <param name="location">The name of the location or building which contains the chest.</param>
        /// <param name="defaultName">The default name if it hasn't been customised.</param>
        public ManagedChest(Chest chest, string location, string defaultName)
        {
            // save values
            this.Chest = chest;
            this.LocationName = location;
            this.Name = chest.Name != "Chest"
                ? chest.Name
                : defaultName;

            // extract tags
            foreach (Match match in Regex.Matches(this.Name, ManagedChest.TagGroupPattern))
            {
                string tag = match.Groups[1].Value;

                // ignore
                if (tag.ToLower() == "ignore")
                {
                    this.IsIgnored = true;
                    continue;
                }

                // category
                if (tag.ToLower().StartsWith("cat:"))
                {
                    this.Category = tag.Substring(4).Trim();
                    continue;
                }

                // order
                int order;
                if (int.TryParse(tag, out order))
                    this.Order = order;
            }
            this.Name = Regex.Replace(this.Name, ManagedChest.TagGroupPattern, "").Trim();

            // normalise
            if(this.Category == null)
                this.Category = "";
        }

        /// <summary>Get the grouping category for a chest.</summary>
        public string GetGroup()
        {
            return !string.IsNullOrWhiteSpace(this.Category)
                ? this.Category
                : this.LocationName;
        }

        /// <summary>Update the chest metadata.</summary>
        /// <param name="name">The chest's display name.</param>
        /// <param name="category">The category name (if any).</param>
        /// <param name="order">The sort value (if any).</param>
        /// <param name="ignored">Whether the chest should be ignored.</param>
        public void Update(string name, string category, int? order, bool ignored)
        {
            this.Name = !string.IsNullOrWhiteSpace(name) ? name.Trim() : this.Name;
            this.Category = category?.Trim() ?? "";
            this.Order = order;
            this.IsIgnored = ignored;

            this.Update();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Update the chest metadata.</summary>
        private void Update()
        {
            string name = this.Name;
            if (this.Order.HasValue)
                name += $" |{this.Order}|";
            if (this.IsIgnored)
                name += " |ignore|";
            if (!string.IsNullOrWhiteSpace(this.Category))
                name += $" |cat:{this.Category}|";

            this.Chest.name = name;
        }
    }
}