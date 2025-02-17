﻿using ARKBreedingStats.species;
using System.Collections.Generic;

namespace ARKBreedingStats.uiControls
{
    public class Trough
    {
        //public readonly Dictionary<string, List<int>> foodStacks = new Dictionary<string, List<int>>();
        //public readonly Dictionary<string, int> spoilTimers = new Dictionary<string, int>();
        //private int ticks;
        //
        //public void Tick()
        //{
        //    ticks++;
        //    // subtract one item from each stack if spoiled
        //    foreach (KeyValuePair<string, int> s in spoilTimers)
        //    {
        //        if (s.Value % ticks == 0 && foodStacks.ContainsKey(s.Key))
        //        {
        //            for (int i = 0; i < foodStacks[s.Key].Count; i++)
        //            {
        //                foodStacks[s.Key][i]--;
        //                if (foodStacks[s.Key][i] == 0)
        //                    foodStacks[s.Key].RemoveAt(i--);
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Calculates the needed food from a specific maturation to a specific maturation.
        /// The returned value adds 10 % to compensate for spoiling.
        /// </summary>
        public static bool FoodAmountFromUntil(Species species, double babyFoodConsumptionSpeedMultiplier, double currentMaturation, double untilMaturation, out double totalFood)
        {
            totalFood = 0;
            if (currentMaturation == untilMaturation) return true;
            if (species?.taming == null || species.breeding == null || currentMaturation > untilMaturation || untilMaturation > 1) return false;

            // food rate in hunger units/s
            // max food rate at maturation 0 %
            var maxFoodRate = species.taming.foodConsumptionBase * species.taming.babyFoodConsumptionMult * babyFoodConsumptionSpeedMultiplier;
            const double baseMinFoodRate = 0.000155; // taken from crumplecorn
            // min food rate at maturation 100 %
            var minFoodRate = baseMinFoodRate * species.taming.babyFoodConsumptionMult * babyFoodConsumptionSpeedMultiplier;
            var foodRateDecay = minFoodRate - maxFoodRate;

            // to get the current food rate for a maturation value: maxFoodRate + maturation * foodRateDecay
            var foodRateStart = maxFoodRate + currentMaturation * foodRateDecay;
            var foodRateEnd = maxFoodRate + untilMaturation * foodRateDecay;

            // calculate area of rectangle and triangle on top to get the total food needed
            // assuming foodRateStart > foodRateEnd
            totalFood = species.breeding.maturationTimeAdjusted * ((untilMaturation - currentMaturation) * (foodRateEnd + 0.5 * (foodRateStart - foodRateEnd)));

            totalFood *= 1.1; // rough estimation of spoiling

            return true;
        }
    }
}
