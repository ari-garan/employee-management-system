using EmployeeManagement.Services.Practice;
using Xunit;

namespace EmployeeManagement.Tests;

public class LeetCodePracticeTests
{
    // DAY 1: Concatenation of Array
    [Theory]
    [InlineData(new int[] { 1, 2, 1 }, new int[] { 1, 2, 1, 1, 2, 1 })]
    [InlineData(new int[] { 1, 3, 2, 1 }, new int[] { 1, 3, 2, 1, 1, 3, 2, 1 })]
    [InlineData(new int[] { 42 }, new int[] { 42, 42 })]
    [InlineData(new int[] { }, new int[] { })]
    public void Day1_GetConcatenation_ReturnsDoubledArray(int[] input, int[] expected)
    {
        var result = LeetCodeWeek1.GetConcatenation(input);
        Assert.Equal(expected, result);
    }

    // DAY 2: Number of Steps to Reduce a Number to Zero
    [Theory]
    [InlineData(14, 6)]
    [InlineData(8, 4)]
    [InlineData(123, 12)]
    [InlineData(0, 0)]
    public void Day2_NumberOfSteps_ReturnsExpectedStepCount(int input, int expected)
    {
        var result = LeetCodeWeek1.NumberOfSteps(input);
        Assert.Equal(expected, result);
    }

    // DAY 3: Contains Duplicate
    [Theory]
    [InlineData(new int[] { 1, 2, 3, 1 }, true)]
    [InlineData(new int[] { 1, 2, 3, 4 }, false)]
    [InlineData(new int[] { 1, 1, 1, 3, 3, 4, 3, 2, 4, 2 }, true)]
    [InlineData(new int[] { 99 }, false)]
    [InlineData(new int[] { }, false)]
    public void Day3_ContainsDuplicate_IdentifiesDuplicatesAccurately(int[] input, bool expected)
    {
        var result = LeetCodeWeek1.ContainsDuplicate(input);
        Assert.Equal(expected, result);
    }

    // DAY 4: Best Time to Buy and Sell Stock
    [Theory]
    [InlineData(new int[] { 7, 1, 5, 3, 6, 4 }, 5)] // Buy at 1, sell at 6
    [InlineData(new int[] { 7, 6, 4, 3, 1 }, 0)]    // Decreasing: no profit
    [InlineData(new int[] { 2, 4, 1 }, 2)]          // Buy at 2, sell at 4
    [InlineData(new int[] { 5 }, 0)]                // Single day: 0 profit
    [InlineData(new int[] { }, 0)]                 // Empty array: 0 profit
    public void Day4_MaxProfit_FindsMaximumGain(int[] prices, int expectedProfit)
    {
        var result = LeetCodeWeek1.MaxProfit(prices);
        Assert.Equal(expectedProfit, result);
    }

    // DAY 5: Valid Anagram
    [Theory]
    [InlineData("anagram", "nagaram", true)]
    [InlineData("rat", "car", false)]
    [InlineData("listen", "silent", true)]
    [InlineData("a", "a", true)]
    [InlineData("a", "b", false)]
    [InlineData("ab", "a", false)]
    public void Day5_IsAnagram_EvaluatesCorrectly(string s, string t, bool expected)
    {
        var result = LeetCodeWeek1.IsAnagram(s, t);
        Assert.Equal(expected, result);
    }
}

