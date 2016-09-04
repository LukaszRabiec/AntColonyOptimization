using System;

namespace AntColonyOptimization
{
    public class Ant
    {
        public int[] Trail { get; set; }
        public bool[] VisitedCities { get; set; }

        private Ant()
        {
        }

        public static Ant Create()
        {
            return new Ant();
        }

        public Ant WithRandomTrail(int numberOfCities, Random randomizer)
        {
            var startingCity = randomizer.Next(0, numberOfCities);
            Trail = InitializeRandomTrail(startingCity, numberOfCities, randomizer);
            VisitedCities = new bool[numberOfCities];

            return this;
        }

        private int[] InitializeRandomTrail(int startingCity, int numberOfCities, Random randomizer)
        {
            var trail = new int[numberOfCities];

            for (int cityId = 0; cityId < numberOfCities; cityId++)
            {
                trail[cityId] = cityId;
            }

            ShuffleTrail(trail, randomizer);
            PutStartingCityAsFirstInTrail(startingCity, trail);

            return trail;
        }

        private void ShuffleTrail(int[] trail, Random randomizer)
        {
            for (int cityId = 0; cityId < trail.Length; cityId++)
            {
                var randId = randomizer.Next(cityId, trail.Length);
                var tmp = trail[randId];
                trail[randId] = trail[cityId];
                trail[cityId] = tmp;
            }
        }

        private void PutStartingCityAsFirstInTrail(int startingCity, int[] trail)
        {
            var startingCityId = GetIndexOfCityInTrail(startingCity, trail);
            var tmp = trail[0];
            trail[0] = trail[startingCityId];
            trail[startingCityId] = tmp;
        }

        private int GetIndexOfCityInTrail(int city, int[] trail)
        {
            for (int cityId = 0; cityId < trail.Length; cityId++)
            {
                if (trail[cityId] == city)
                {
                    return cityId;
                }
            }

            throw new Exception("Index of specified city not found [GetIndexOfCityInTrail(int, int[])].");
        }

        public double CalculateTotalTrailDistance(Map map)
        {
            const int lastCityOffset = 1;
            const int nextCityOffset = 1;
            double sum = 0;

            for (int cityId = 0; cityId < Trail.Length - lastCityOffset; cityId++)
            {
                var fromCityId = Trail[cityId];
                var toCityId = Trail[cityId + nextCityOffset];
                var distanceBetweenCities = map.Distances[fromCityId, toCityId];
                sum += distanceBetweenCities;
            }

            return sum;
        }

        public bool TrailContainsEdge(int city, int neighbourCity)
        {
            var lastIndex = Trail.Length - 1;
            
            // Checks beginning of Trail
            if (Trail[0] == city && Trail[1] == neighbourCity)
            {
                return true;
            }
            if (Trail[0] == city && Trail[lastIndex] == neighbourCity)
            {
                return true;
            }
            if (Trail[0] == city)
            {
                return false;
            }

            // Chcecks ending of Trail
            if (Trail[lastIndex] == city && Trail[lastIndex - 1] == neighbourCity)
            {
                return true;
            }
            if (Trail[lastIndex] == city && Trail[0] == neighbourCity)
            {
                return true;
            }
            if (Trail[lastIndex] == city)
            {
                return false;
            }

            var indexOfCity = GetIndexOfCityInTrail(city, Trail);

            // Chcecks somewhere in middle of Trail
            if (Trail[indexOfCity - 1] == neighbourCity)
            {
                return true;
            }
            if (Trail[indexOfCity + 1] == neighbourCity)
            {
                return true;
            }

            return false;
        }

        public void ResetVisited()
        {
            for (int visitedId = 0; visitedId < VisitedCities.Length; visitedId++)
            {
                VisitedCities[visitedId] = false;
            }
        }
    }
}
