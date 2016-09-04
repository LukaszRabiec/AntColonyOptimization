using System;
using System.Collections.Generic;
using System.Linq;

namespace AntColonyOptimization
{
    public struct BestTrail
    {
        public int[] Trail { get; set; }
        public double Distance { get; set; }

        public BestTrail(int[] trail, double distance)
        {
            Trail = trail;
            Distance = distance;
        }
    }

    public class AntColony
    {
        private readonly Random _randomizer;
        private readonly int _numberOfCities;
        private readonly int _alpha;
        private readonly int _beta;
        private List<Ant> _ants;
        private Map _map;
        private double _rhoFactor;
        private double _qFactor;


        /// <summary>
        /// Creates colony with specified parameters.
        /// </summary>
        /// <param name="numberOfAnts">Number of ants in colony.</param>
        /// <param name="map">Map with cities info about distances and pheromones.</param>
        /// <param name="alpha">Influence of pheromone on direction.</param>
        /// <param name="beta">Influence of adjacent node distance.</param>
        /// <param name="rhoFactor">Pheromone decrease factor.</param>
        /// <param name="qFactor">Pheromone increase factor.</param>
        /// 
        public AntColony(int numberOfAnts, Map map, int alpha, int beta, double rhoFactor, double qFactor, Random randomizer)
        {
            _randomizer = randomizer;
            _map = map;
            _numberOfCities = map.Distances.GetLength(0);
            _alpha = alpha;
            _beta = beta;
            _rhoFactor = rhoFactor;
            _qFactor = qFactor;
            InitializeAnts(numberOfAnts);
        }

        private void InitializeAnts(int numberOfAnts)
        {
            _ants = new List<Ant>();

            for (int antId = 0; antId < numberOfAnts; antId++)
            {
                _ants.Add(Ant.Create().WithRandomTrail(_numberOfCities, _randomizer));
            }
        }

        public BestTrail LetAntsToFindBestTrail(int iterations)
        {
            Ant bestAnt = GetAntWithShortestDistance();
            var bestDistance = bestAnt.CalculateTotalTrailDistance(_map);
            var bestAntTrail = new int[_numberOfCities];
            bestAnt.Trail.CopyTo(bestAntTrail, 0);
            var bestTrail = new BestTrail(bestAntTrail, bestDistance);

            for (int iteration = 0; iteration < iterations; iteration++)
            {
                UpdateAnts();
                UpdatePheromones();
                
                var currentBestAnt = GetAntWithShortestDistance();
                var currentBestDistance = currentBestAnt.CalculateTotalTrailDistance(_map);

                if (currentBestDistance < bestDistance)
                {
                    bestDistance = currentBestDistance;
                    currentBestAnt.Trail.CopyTo(bestTrail.Trail, 0);
                    bestTrail.Distance = currentBestDistance;
                }
            }

            return bestTrail;
        }

        private void UpdateAnts()
        {
            foreach (var ant in _ants)
            {
                var newTrail = BuildTrail(ant);
                ant.Trail = newTrail;
            }
        }

        private int[] BuildTrail(Ant ant)
        {
            var trail = new int[_numberOfCities];
            ant.ResetVisited();
            var startingCity = _randomizer.Next(0, _numberOfCities);
            const int lastCityOffset = 1;

            trail[0] = startingCity;
            ant.VisitedCities[startingCity] = true;

            for (int cityId = 0; cityId < _numberOfCities - lastCityOffset; cityId++)
            {
                var currentCity = trail[cityId];
                var nextCity = GetNextCity(ant, currentCity);
                trail[cityId + 1] = nextCity;
                ant.VisitedCities[nextCity] = true;
            }

            return trail;
        }

        private int GetNextCity(Ant ant, int currentCity)
        {
            var probabilities = CalculateProbabilities(ant, currentCity);
            var cumulatedProbabilities = CumulateProbabilities(probabilities);
            var nextCity = SpinRoulette(cumulatedProbabilities);

            return nextCity;
        }

        private double[] CalculateProbabilities(Ant ant, int currentCity)
        {
            double sum = 0.0;

            var tauSigmaProduct = new double[_numberOfCities];

            for (int i = 0; i < tauSigmaProduct.Length; i++)
            {
                if (i == currentCity)
                {
                    tauSigmaProduct[i] = 0.0;
                }
                else if (ant.VisitedCities[i])
                {
                    tauSigmaProduct[i] = 0.0;
                }
                else
                {
                    var tauPow = Math.Pow(_map.Pheromones[currentCity, i], _alpha);
                    var sigmaPow = Math.Pow(1.0 / _map.Distances[currentCity, i], _beta);
                    tauSigmaProduct[i] = tauPow * sigmaPow;

                    if (tauSigmaProduct[i] < 0.0001)
                    {
                        tauSigmaProduct[i] = 0.0001;
                    }
                    else if (tauSigmaProduct[i] > double.MaxValue / (_numberOfCities * 100))
                    {
                        tauSigmaProduct[i] = double.MaxValue / (_numberOfCities * 100);
                    }
                }

                sum += tauSigmaProduct[i];
            }

            var probabilities = new double[_numberOfCities];

            for (int i = 0; i < probabilities.Length; i++)
            {
                probabilities[i] = tauSigmaProduct[i] / sum;    // Houston we have problem when the sum equals zero :/
            }

            return probabilities;
        }

        private double[] CumulateProbabilities(double[] probabilities)
        {
            var cumulatedOffset = 1;
            var cumulated = new double[probabilities.Length + cumulatedOffset];

            for (int i = 0; i < probabilities.Length; i++)
            {
                cumulated[i + 1] = cumulated[i] + probabilities[i];
            }
            //cumulated[cumulated.Distance - 1] = 1.0;

            return cumulated;
        }

        private int SpinRoulette(double[] cumulatedProbabilities)
        {
            var drawn = _randomizer.NextDouble();

            for (int index = 0; index < cumulatedProbabilities.Length - 1; index++)
            {
                if (drawn >= cumulatedProbabilities[index] && drawn < cumulatedProbabilities[index + 1])
                {
                    return index;
                }
            }

            throw new Exception("Failed to choose the next city [SpinRoulette(double[])].");
        }

        private void UpdatePheromones()
        {
            const int diagonalOffset = 1;
            for (int cityId = 0; cityId < _map.Pheromones.GetLength(0); cityId++)
            {
                for (int neighbourId = cityId + diagonalOffset; neighbourId < _map.Pheromones.GetLength(1); neighbourId++)
                {
                    foreach (var ant in _ants)
                    {
                        var trailLength = ant.CalculateTotalTrailDistance(_map);
                        var decreaseFactor = (1.0 - _rhoFactor) * _map.Pheromones[cityId, neighbourId];
                        var increaseFactor = 0.0;

                        if(ant.TrailContainsEdge(cityId, neighbourId))
                        {
                            increaseFactor = _qFactor / trailLength;
                        }

                        _map.Pheromones[cityId, neighbourId] = decreaseFactor + increaseFactor;

                        const double lowerBound = 0.0001;
                        const double higherBound = 100000.0;
                        SetPheromonesIfOutOfRanges(cityId, neighbourId, lowerBound, higherBound);

                        _map.Pheromones[neighbourId, cityId] = _map.Pheromones[cityId, neighbourId];
                    }
                }
            }
        }

        private void SetPheromonesIfOutOfRanges(int city, int neighbour, double lowerBound, double higherBound)
        {
            if (_map.Pheromones[city, neighbour] < lowerBound)
            {
                _map.Pheromones[city, neighbour] = lowerBound;
            }
            else if (_map.Pheromones[city, neighbour] > higherBound)
            {
                _map.Pheromones[city, neighbour] = higherBound;
            }
        }

        private Ant GetAntWithShortestDistance()
        {
            var bestDistance = _ants.First().CalculateTotalTrailDistance(_map);
            Ant bestAnt = _ants.First();

            for (int antId = 1; antId < _ants.Count; antId++)
            {
                var antDistance = _ants[antId].CalculateTotalTrailDistance(_map);

                if (antDistance < bestDistance)
                {
                    bestAnt = _ants[antId];
                    bestDistance = antDistance;
                }
            }

            return bestAnt;
        }
    }
}
