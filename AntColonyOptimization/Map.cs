using System;
using System.IO;
using Newtonsoft.Json;

namespace AntColonyOptimization
{
    public struct Range
    {
        public int Min { get; }

        public int Max { get; }

        public Range(int min, int max)
        {
            Min = min;
            Max = max;
        }
    }

    public class Map
    {
        public int[,] Distances { get; private set; }
        public double[,] Pheromones { get; private set; }


        private const double _startedPheromones = 0.01;

        private Map()
        {
        }

        public static Map Create()
        {
            return new Map();
        }

        public Map AddCitiesWithRandomDistance(int numberOfCities, Random randomizer, Range distancesRange)
        {
            Distances = new int[numberOfCities, numberOfCities];
            Pheromones = new double[numberOfCities, numberOfCities];
            RandomizeDistances(randomizer, distancesRange);
            InitializePheromones();

            return this;
        }

        private void RandomizeDistances(Random randomizer, Range distancesRange)
        {
            const int diagonalOffset = 1;
            const int inclusiveOffset = 1;

            for (int row = 0; row < Distances.GetLength(0); row++)
            {
                for (int col = row + diagonalOffset; col < Distances.GetLength(1); col++)
                {
                    var distance = randomizer.Next(distancesRange.Min, distancesRange.Max + inclusiveOffset);
                    Distances[row, col] = distance;
                    Distances[col, row] = distance;
                }
            }
        }

        public Map ReadCitiesFromJsonFile(string jsonfilePath)
        {
            Distances = GetGraphFromJsonFile(jsonfilePath);
            var numberOfCities = Distances.GetLength(0);
            Pheromones = new double[numberOfCities, numberOfCities];
            InitializePheromones();

            return this;
        }

        private int[,] GetGraphFromJsonFile(string path)
        {
            using (var streamReader = new StreamReader(path))
            {
                return JsonConvert.DeserializeObject<int[,]>(streamReader.ReadToEnd());
            }
        }

        private void InitializePheromones()
        {
            const int diagonalOffset = 1;
            for (int row = 0; row < Pheromones.GetLength(0); row++)
            {
                for (int col = row + diagonalOffset; col < Pheromones.GetLength(1); col++)
                {
                    Pheromones[row, col] = _startedPheromones;
                    Pheromones[col, row] = _startedPheromones;
                }
            }
        }
    }
}
