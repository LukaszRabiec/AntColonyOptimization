using System;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AntColonyOptimization.Tests
{
    public class AntColonyTests
    {
        private ITestOutputHelper _output;
        private Random _randomizer;

        public AntColonyTests(ITestOutputHelper output)
        {
            _output = output;
            _randomizer = new Random(0);
        }

        [Fact]
        public void SearchingPathWithGraphV1ShouldReturnBestTrail()
        {
            const int numberOfAnts = 4;
            const int alpha = 3;
            const int beta = 2;
            const double rhoFactor = 0.01;
            const double qFactor = 2.0;

            var map = Map.Create().ReadCitiesFromJsonFile("Data/v1Graph.json");
            var antColony = new AntColony(numberOfAnts, map, alpha, beta, rhoFactor, qFactor, _randomizer);

            var bestTrail = antColony.LetAntsToFindBestTrail(50);
            var bestDistance = 43;

            bestTrail.Distance.ShouldBeEquivalentTo(bestDistance);
            ShowTrail(bestTrail);
        }

        [Fact]
        public void SearchingPathWithRandomizedMapShouldReturnBestTrail()
        {
            const int numberOfAnts = 4;
            const int numberOfCities = 10;
            const int alpha = 3;
            const int beta = 2;
            const double rhoFactor = 0.01;
            const double qFactor = 2.0;

            var map = Map.Create().AddCitiesWithRandomDistance(numberOfCities, _randomizer, new Range(1, 8));
            var antColony = new AntColony(numberOfAnts, map, alpha, beta, rhoFactor, qFactor, _randomizer);

            var bestTrail = antColony.LetAntsToFindBestTrail(30);
            var bestDistance = 20;

            bestTrail.Distance.ShouldBeEquivalentTo(bestDistance);
            ShowTrail(bestTrail);
        }

        private void ShowTrail(BestTrail bestTrail)
        {
            foreach (int cityId in bestTrail.Trail)
            {
                _output.WriteLine(cityId.ToString());
            }
        }
    }
}
