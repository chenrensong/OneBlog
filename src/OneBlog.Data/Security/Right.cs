using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections.ObjectModel;
using OneBlog.Helpers;

namespace OneBlog.Security
{
    public class Right
    {
        private static readonly ReadOnlyCollection<Rights> rightFlagValues;
        private static readonly ReadOnlyCollection<Right> allRightInstances;

        private static readonly Dictionary<Rights, Right> rightsByFlag = new Dictionary<Rights, Right>();
        private static readonly Dictionary<string, Right> rightsByName = new Dictionary<string, Right>(StringComparer.OrdinalIgnoreCase);
        public Rights CurFlag { get; private set; }
        public string FlagName { get; private set; }



        /// <summary>
        /// Returns a display-friendly version of this Right's name.
        /// </summary>
        public string DisplayName
        {
            get { return StringHelper.FormatIdentifierForDisplay(FlagName); }
        }


        static Right()
        {
            // Initialize the various dictionaries to their starting state.

            var flagType = typeof(Rights);
            rightFlagValues = Enum.GetValues(flagType).Cast<Rights>().ToList().AsReadOnly();

            var allRights = new List<Right>();

            // Create a Right instance for each value in the Rights enum.
            foreach (var flag in rightFlagValues)
            {
                Rights curFlag = (Rights)flag;
                var flagName = Enum.GetName(flagType, curFlag);
                var curRight = new Right(curFlag, flagName);

                allRights.Add(curRight);

                // Use the Add function so if there are multiple flags with the same
                // value they can be caught quickly at runtime.
                rightsByFlag.Add(curFlag, curRight);

                rightsByName.Add(flagName, curRight);
            }

            allRightInstances = allRights.AsReadOnly();
        }

        public Right(Rights curFlag, string flagName)
        {
            this.CurFlag = curFlag;
            this.FlagName = flagName;
        }

        /// <summary>
        /// Returns an IEnumerable of all of the Rights that exist on BlogEngine.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Right> GetAllRights()
        {
            return Right.allRightInstances;
        }

        /// <summary>
        /// Returns a Right instance based on its name.
        /// </summary>
        /// <param name="rightName"></param>
        /// <returns></returns>
        public static Right GetRightByName(string rightName)
        {
            if (String.IsNullOrWhiteSpace(rightName))
            {
                throw new ArgumentNullException(nameof(rightName));
            }
            else
            {
                Right right = null;
                if (rightsByName.TryGetValue(rightName.Trim(), out right))
                {
                    return right;
                }
                else
                {
                    throw new KeyNotFoundException("No Right exists by the name '" + rightName + "'");
                }
            }
        }
    }
}
