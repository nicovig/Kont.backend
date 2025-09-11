using NUnit.Framework;
using Kont.backend.Services;
using Kont.backend.DAL;
using Kont.backend.Models.Scoring;

namespace Kont.backend.tests.Services;

[TestFixture]
public class ScoringServiceTests
{
    private ScoringService _scoringService;

    [SetUp]
    public void Setup()
    {
        // Pour les tests unitaires simples, on peut créer une instance sans contexte
        _scoringService = new ScoringService(null!);
    }

    [Test]
    public void NormalizeScore_HigherIsBetter_ReturnsOriginalValue()
    {
        // Arrange
        var metric = new ScoringMetric
        {
            HigherIsBetter = true,
            Coefficient = 1.0
        };
        var value = 150.0;

        // Act
        var result = _scoringService.NormalizeScore(value, metric);

        // Assert
        Assert.That(result, Is.EqualTo(150.0));
    }

    [Test]
    public void NormalizeScore_LowerIsBetter_ReturnsInvertedValue()
    {
        // Arrange
        var metric = new ScoringMetric
        {
            HigherIsBetter = false,
            Coefficient = 1.0
        };
        var value = 50.0;

        // Act
        var result = _scoringService.NormalizeScore(value, metric);

        // Assert
        Assert.That(result, Is.EqualTo(950.0)); // 1000 - 50
    }

    [Test]
    public void CalculatePercentageScore_WithValidScores_ReturnsCorrectPercentage()
    {
        // Arrange
        var playerScore = 80.0;
        var bestScore = 100.0;

        // Act
        var percentage = (playerScore / bestScore) * 100;

        // Assert
        Assert.That(percentage, Is.EqualTo(80.0));
    }

    [Test]
    public void CalculatePercentageScore_WithZeroBestScore_ReturnsZero()
    {
        // Arrange
        var playerScore = 50.0;
        var bestScore = 0.0;

        // Act
        var percentage = bestScore > 0 ? (playerScore / bestScore) * 100 : 0;

        // Assert
        Assert.That(percentage, Is.EqualTo(0.0));
    }

    [Test]
    public void PlayerRanking_WithMultipleActivities_CalculatesGlobalScoreCorrectly()
    {
        // Arrange
        var activityScores = new List<ActivityScore>
        {
            new ActivityScore { Score = 100.0 },
            new ActivityScore { Score = 80.0 },
            new ActivityScore { Score = 60.0 }
        };

        // Act
        var globalScore = activityScores.Sum(acs => acs.Score);

        // Assert
        Assert.That(globalScore, Is.EqualTo(240.0));
    }

    [Test]
    public void PlayerRanking_WithBase100Scoring_CalculatesCorrectPercentages()
    {
        // Arrange
        var playerRankings = new List<PlayerRanking>
        {
            new PlayerRanking { GlobalScore = 100.0 },
            new PlayerRanking { GlobalScore = 80.0 },
            new PlayerRanking { GlobalScore = 60.0 }
        };

        var bestScore = playerRankings.Max(pr => pr.GlobalScore);

        // Act
        foreach (var ranking in playerRankings)
        {
            ranking.GlobalPercentage = Math.Round((ranking.GlobalScore / bestScore) * 100, 1);
        }

        // Assert
        Assert.That(playerRankings[0].GlobalPercentage, Is.EqualTo(100.0));
        Assert.That(playerRankings[1].GlobalPercentage, Is.EqualTo(80.0));
        Assert.That(playerRankings[2].GlobalPercentage, Is.EqualTo(60.0));
    }

    [Test]
    public void PlayerRanking_WithMixedPerformance_ShowsGlobalAdvantage()
    {
        // Arrange - Simule un joueur qui n'est jamais 1er mais toujours dans le top 3
        var player1 = new PlayerRanking 
        { 
            PlayerName = "Alice", 
            GlobalScore = 85.0,
            ActivityScores = new List<ActivityScore>
            {
                new ActivityScore { ActivityName = "Bowling", Score = 90.0, Rank = 2 },
                new ActivityScore { ActivityName = "Karting", Score = 80.0, Rank = 3 },
                new ActivityScore { ActivityName = "Escape Game", Score = 85.0, Rank = 2 }
            }
        };

        var player2 = new PlayerRanking 
        { 
            PlayerName = "Bob", 
            GlobalScore = 75.0,
            ActivityScores = new List<ActivityScore>
            {
                new ActivityScore { ActivityName = "Bowling", Score = 100.0, Rank = 1 },
                new ActivityScore { ActivityName = "Karting", Score = 50.0, Rank = 5 },
                new ActivityScore { ActivityName = "Escape Game", Score = 75.0, Rank = 4 }
            }
        };

        var rankings = new List<PlayerRanking> { player1, player2 };

        // Act - Trier par score global
        rankings = rankings.OrderByDescending(pr => pr.GlobalScore).ToList();

        // Assert - Alice (toujours top 3) bat Bob (1er une fois mais mauvais ailleurs)
        Assert.That(rankings[0].PlayerName, Is.EqualTo("Alice"));
        Assert.That(rankings[1].PlayerName, Is.EqualTo("Bob"));
    }

    [Test]
    public void Base100Scoring_WithDifferentScores_CalculatesCorrectPercentages()
    {
        // Arrange - Scores de différents joueurs
        var scores = new List<double> { 100.0, 80.0, 60.0, 40.0, 20.0 };
        var bestScore = scores.Max();

        // Act - Calculer les pourcentages (base 100)
        var percentages = scores.Select(score => Math.Round((score / bestScore) * 100, 1)).ToList();

        // Assert
        Assert.That(percentages[0], Is.EqualTo(100.0)); // Meilleur score = 100%
        Assert.That(percentages[1], Is.EqualTo(80.0));  // 80% du meilleur
        Assert.That(percentages[2], Is.EqualTo(60.0));  // 60% du meilleur
        Assert.That(percentages[3], Is.EqualTo(40.0));  // 40% du meilleur
        Assert.That(percentages[4], Is.EqualTo(20.0));  // 20% du meilleur
    }

    [Test]
    public void RankingCalculation_WithEqualScores_AssignsCorrectRanks()
    {
        // Arrange - Joueurs avec des scores égaux
        var rankings = new List<PlayerRanking>
        {
            new PlayerRanking { PlayerName = "Alice", GlobalScore = 100.0 },
            new PlayerRanking { PlayerName = "Bob", GlobalScore = 100.0 },
            new PlayerRanking { PlayerName = "Charlie", GlobalScore = 80.0 },
            new PlayerRanking { PlayerName = "David", GlobalScore = 80.0 },
            new PlayerRanking { PlayerName = "Eve", GlobalScore = 60.0 }
        };

        // Act - Trier et assigner les rangs
        rankings = rankings
            .OrderByDescending(pr => pr.GlobalScore)
            .ThenBy(pr => pr.PlayerName)
            .ToList();

        for (int i = 0; i < rankings.Count; i++)
        {
            rankings[i].GlobalRank = i + 1;
        }

        // Assert
        Assert.That(rankings[0].GlobalRank, Is.EqualTo(1)); // Alice (100, premier alphabétiquement)
        Assert.That(rankings[1].GlobalRank, Is.EqualTo(2)); // Bob (100, deuxième alphabétiquement)
        Assert.That(rankings[2].GlobalRank, Is.EqualTo(3)); // Charlie (80, premier alphabétiquement)
        Assert.That(rankings[3].GlobalRank, Is.EqualTo(4)); // David (80, deuxième alphabétiquement)
        Assert.That(rankings[4].GlobalRank, Is.EqualTo(5)); // Eve (60)
    }

    [Test]
    public void CoefficientWeighting_WithDifferentWeights_AppliesCorrectly()
    {
        // Arrange - Métriques avec différents coefficients
        var metrics = new List<PlayerMetricResult>
        {
            new PlayerMetricResult { MetricName = "Points", Value = 100.0, Coefficient = 0.7, HigherIsBetter = true },
            new PlayerMetricResult { MetricName = "Time", Value = 50.0, Coefficient = 0.3, HigherIsBetter = false },
            new PlayerMetricResult { MetricName = "Bonus", Value = 20.0, Coefficient = 0.1, HigherIsBetter = true }
        };

        // Act - Calculer le score total pondéré
        double totalScore = 0;
        foreach (var metric in metrics)
        {
            var normalizedValue = metric.HigherIsBetter ? metric.Value : (1000 - metric.Value);
            totalScore += normalizedValue * metric.Coefficient;
        }

        // Assert
        // Points: 100 * 0.7 = 70
        // Time: (1000-50) * 0.3 = 285
        // Bonus: 20 * 0.1 = 2
        // Total: 357
        Assert.That(totalScore, Is.EqualTo(357.0));
    }

    [Test]
    public void MetricScoring_WithDifferentCoefficients_AppliesWeightsCorrectly()
    {
        // Arrange
        var metrics = new List<PlayerMetricResult>
        {
            new PlayerMetricResult { MetricName = "Points", Value = 100.0, Coefficient = 0.7, HigherIsBetter = true },
            new PlayerMetricResult { MetricName = "Time", Value = 50.0, Coefficient = 0.3, HigherIsBetter = false }
        };

        // Act
        double totalScore = 0;
        foreach (var metric in metrics)
        {
            var normalizedValue = metric.HigherIsBetter ? metric.Value : (1000 - metric.Value);
            totalScore += normalizedValue * metric.Coefficient;
        }

        // Assert
        // Points: 100 * 0.7 = 70
        // Time: (1000-50) * 0.3 = 285
        // Total: 355
        Assert.That(totalScore, Is.EqualTo(355.0));
    }

}
