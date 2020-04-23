// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TemporaryVariableAllocator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    /// Responsible for allocating and keeping track of temporary variables.
    /// </summary>
    internal class TemporaryVariableAllocator
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly Dictionary<string, SortedSet<int>> _reserveSets = new Dictionary<string, SortedSet<int>>();
        private readonly Stack<IList<string>> _variablesReservedInScopes = new Stack<IList<string>>();

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void PushReservationScope()
        {
            _variablesReservedInScopes.Push(new List<string>());
        }

        public void PopReservationScope()
        {
            if (!_variablesReservedInScopes.TryPop(out IList<string> scope))
            {
                throw new InvalidOperationException("Cannot pop a reservation scope when the scope stack is empty.");
            }

            foreach (string variableName in scope)
            {
                Return(variableName);
            }
        }

        public string Reserve(string prefix)
        {
            int reservationNumber = 1;

            // find the next number to use
            if (_reserveSets.TryGetValue(prefix, out SortedSet<int> reserveSet))
            {
                reservationNumber = reserveSet.Max + 1;
            }
            else
            {
                _reserveSets.Add(prefix, new SortedSet<int>());
            }

            // reserve the number
            _reserveSets[prefix].Add(reservationNumber);

            string variableName = prefix + reservationNumber;

            // Add the variable to the current scope (if there is one).
            if (_variablesReservedInScopes.TryPeek(out IList<string> scope))
            {
                scope.Add(variableName);
            }

            return variableName;
        }

        public void Return(string variableName)
        {
            // find the last digit in the string
            int lastIndex = variableName.Length - 1;
            while (lastIndex >= 0 && char.IsNumber(variableName[lastIndex]))
            {
                lastIndex--;
            }

            lastIndex++;

            int reservationNumber = int.Parse(
                variableName.Substring(lastIndex),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture);

            string prefix = variableName.Substring(0, lastIndex);

            if (!_reserveSets.TryGetValue(prefix, out SortedSet<int> reserveSet))
            {
                throw new InvalidOperationException($"Prefix {prefix} does not have any reservations");
            }

            if (!reserveSet.Contains(reservationNumber))
            {
                throw new InvalidOperationException($"Variable {variableName} is not in the reservation set");
            }

            reserveSet.Remove(reservationNumber);
        }
    }
}
