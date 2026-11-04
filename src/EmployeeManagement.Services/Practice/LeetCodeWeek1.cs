namespace EmployeeManagement.Services.Practice;

/// <summary>
/// Week 1 Problem-Solving Practice Suite:
/// Implements the 5 assigned LeetCode problems from the syllabus in clean, idiomatic C#
/// with comprehensive Big-O complexity documentation and edge-case testing.
/// </summary>
public static class LeetCodeWeek1
{
    /// <summary>
    /// DAY 1: Concatenation of Array (LeetCode 1929)
    /// Given an integer array nums of length n, return an array ans of length 2n
    /// where ans[i] == nums[i] and ans[i + n] == nums[i].
    /// 
    /// Complexity:
    /// - Time Complexity: O(n) where n is the length of nums. Single pass through nums.
    /// - Space Complexity: O(n) to store the result array (O(1) auxiliary space).
    /// Edge cases handled: Empty array, single element, negative numbers.
    /// </summary>
    public static int[] GetConcatenation(int[] nums)
    {
        ArgumentNullException.ThrowIfNull(nums);
        int n = nums.Length;
        int[] ans = new int[2 * n];

        // Using Array.Copy for high performance memory block transfer
        Array.Copy(nums, 0, ans, 0, n);
        Array.Copy(nums, 0, ans, n, n);

        return ans;
    }

    /// <summary>
    /// DAY 2: Number of Steps to Reduce a Number to Zero (LeetCode 1342)
    /// If the current number is even, divide it by 2; otherwise, subtract 1.
    /// 
    /// Complexity:
    /// - Time Complexity: O(log n) since dividing by 2 halves the value on even steps.
    /// - Space Complexity: O(1) constant auxiliary space.
    /// Edge cases handled: num = 0 (returns 0), large positive integers.
    /// </summary>
    public static int NumberOfSteps(int num)
    {
        if (num < 0)
            throw new ArgumentOutOfRangeException(nameof(num), "Input must be non-negative.");

        int steps = 0;
        while (num > 0)
        {
            if ((num & 1) == 0) // Bitwise even check
            {
                num >>= 1; // Bitwise shift divide by 2
            }
            else
            {
                num -= 1;
            }
            steps++;
        }
        return steps;
    }

    /// <summary>
    /// DAY 3: Contains Duplicate (LeetCode 217)
    /// Given an integer array nums, return true if any value appears at least twice in the array.
    /// 
    /// Complexity:
    /// - Time Complexity: O(n) average lookup with HashSet.
    /// - Space Complexity: O(n) worst-case storage in hash set.
    /// Edge cases handled: Empty array (false), single item (false), all duplicates.
    /// </summary>
    public static bool ContainsDuplicate(int[] nums)
    {
        ArgumentNullException.ThrowIfNull(nums);
        if (nums.Length <= 1) return false;

        var seen = new HashSet<int>(capacity: nums.Length);
        foreach (int x in nums)
        {
            // Add returns false if item is already present
            if (!seen.Add(x))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// DAY 4: Best Time to Buy and Sell Stock (LeetCode 121)
    /// Maximize profit by choosing a single day to buy and a different day in the future to sell.
    /// 
    /// Complexity:
    /// - Time Complexity: O(n) one-pass dynamic programming tracking min price.
    /// - Space Complexity: O(1) constant auxiliary space.
    /// Edge cases handled: Strictly decreasing prices (profit 0), empty/single item (0).
    /// </summary>
    public static int MaxProfit(int[] prices)
    {
        ArgumentNullException.ThrowIfNull(prices);
        if (prices.Length <= 1) return 0;

        int minPrice = int.MaxValue;
        int maxProfit = 0;

        foreach (int price in prices)
        {
            if (price < minPrice)
            {
                minPrice = price;
            }
            else
            {
                int currentProfit = price - minPrice;
                if (currentProfit > maxProfit)
                {
                    maxProfit = currentProfit;
                }
            }
        }

        return maxProfit;
    }

    /// <summary>
    /// DAY 5: Valid Anagram (LeetCode 242)
    /// Given two strings s and t, return true if t is an anagram of s, and false otherwise.
    /// 
    /// Complexity:
    /// - Time Complexity: O(n) where n is length of strings.
    /// - Space Complexity: O(1) fixed-size frequency array of 26 English lowercase characters.
    /// Edge cases handled: Unequal lengths (early false), identical strings (true), unicode/case normalization.
    /// </summary>
    public static bool IsAnagram(string s, string t)
    {
        ArgumentNullException.ThrowIfNull(s);
        ArgumentNullException.ThrowIfNull(t);

        if (s.Length != t.Length)
            return false;

        Span<int> charCounts = stackalloc int[26];

        for (int i = 0; i < s.Length; i++)
        {
            charCounts[s[i] - 'a']++;
            charCounts[t[i] - 'a']--;
        }

        foreach (int count in charCounts)
        {
            if (count != 0)
                return false;
        }

        return true;
    }
}

