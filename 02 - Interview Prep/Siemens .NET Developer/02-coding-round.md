# Coding Round — Siemens .NET Developer

> All problems below have been reported by actual Siemens candidates.

## Problem 1: Two Sum

```csharp
// Given array and target, return indices of two numbers that add up to target.
public int[] TwoSum(int[] nums, int target)
{
    var map = new Dictionary<int, int>();
    for (int i = 0; i < nums.Length; i++)
    {
        int complement = target - nums[i];
        if (map.ContainsKey(complement))
            return new[] { map[complement], i };
        map[nums[i]] = i;
    }
    throw new ArgumentException("No solution");
}
// Time: O(n), Space: O(n)
```

## Problem 2: Reverse a Linked List

```csharp
public ListNode ReverseList(ListNode head)
{
    ListNode prev = null, curr = head;
    while (curr != null)
    {
        ListNode next = curr.next;
        curr.next = prev;
        prev = curr;
        curr = next;
    }
    return prev;
}
// Time: O(n), Space: O(1)
```

## Problem 3: Detect Cycle in Linked List (Floyd's)

```csharp
public bool HasCycle(ListNode head)
{
    ListNode slow = head, fast = head;
    while (fast?.next != null)
    {
        slow = slow.next;
        fast = fast.next.next;
        if (slow == fast) return true;
    }
    return false;
}
// Time: O(n), Space: O(1)
```

## Problem 4: Check if Linked List is Palindrome

```csharp
public bool IsPalindrome(ListNode head)
{
    // Find middle
    ListNode slow = head, fast = head;
    while (fast?.next != null) { slow = slow.next; fast = fast.next.next; }

    // Reverse second half
    ListNode prev = null, curr = slow;
    while (curr != null)
    {
        var next = curr.next;
        curr.next = prev;
        prev = curr;
        curr = next;
    }

    // Compare both halves
    ListNode left = head, right = prev;
    while (right != null)
    {
        if (left.val != right.val) return false;
        left = left.next;
        right = right.next;
    }
    return true;
}
```

## Problem 5: Climbing Stairs (DP)

```csharp
// How many distinct ways to climb n stairs (1 or 2 steps)?
public int ClimbStairs(int n)
{
    if (n <= 2) return n;
    int prev2 = 1, prev1 = 2;
    for (int i = 3; i <= n; i++)
    {
        int curr = prev1 + prev2;
        prev2 = prev1;
        prev1 = curr;
    }
    return prev1;
}
// Time: O(n), Space: O(1) — essentially Fibonacci
```

## Problem 6: Reverse Words in a String

```csharp
// Built-in approach
public string ReverseWords(string s)
{
    return string.Join(" ", s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Reverse());
}

// Manual approach (interviewer may ask for this)
public string ReverseWordsManual(string s)
{
    var words = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    int left = 0, right = words.Length - 1;
    while (left < right)
    {
        (words[left], words[right]) = (words[right], words[left]);
        left++; right--;
    }
    return string.Join(" ", words);
}
```

## Problem 7: Sort 0s, 1s, 2s — Dutch National Flag

```csharp
public void SortColors(int[] nums)
{
    int low = 0, mid = 0, high = nums.Length - 1;
    while (mid <= high)
    {
        switch (nums[mid])
        {
            case 0:
                (nums[low], nums[mid]) = (nums[mid], nums[low]);
                low++; mid++; break;
            case 1:
                mid++; break;
            case 2:
                (nums[mid], nums[high]) = (nums[high], nums[mid]);
                high--; break;
        }
    }
}
// Time: O(n), Space: O(1)
```

## Problem 8: Binary Search

```csharp
public int BinarySearch(int[] arr, int target)
{
    int left = 0, right = arr.Length - 1;
    while (left <= right)
    {
        int mid = left + (right - left) / 2;
        if (arr[mid] == target) return mid;
        if (arr[mid] < target) left = mid + 1;
        else right = mid - 1;
    }
    return -1;
}
// Time: O(log n), Space: O(1)
```

## Problem 9: BFS & DFS Tree Traversal

```csharp
// BFS — Level order using Queue
public IList<IList<int>> LevelOrder(TreeNode root)
{
    var result = new List<IList<int>>();
    if (root == null) return result;
    var queue = new Queue<TreeNode>();
    queue.Enqueue(root);

    while (queue.Count > 0)
    {
        int size = queue.Count;
        var level = new List<int>();
        for (int i = 0; i < size; i++)
        {
            var node = queue.Dequeue();
            level.Add(node.val);
            if (node.left != null) queue.Enqueue(node.left);
            if (node.right != null) queue.Enqueue(node.right);
        }
        result.Add(level);
    }
    return result;
}

// DFS — Inorder using Stack
public IList<int> InorderTraversal(TreeNode root)
{
    var result = new List<int>();
    var stack = new Stack<TreeNode>();
    var curr = root;
    while (curr != null || stack.Count > 0)
    {
        while (curr != null) { stack.Push(curr); curr = curr.left; }
        curr = stack.Pop();
        result.Add(curr.val);
        curr = curr.right;
    }
    return result;
}
```

| Feature | BFS | DFS |
|---------|-----|-----|
| Data structure | Queue | Stack / Recursion |
| Order | Level by level | Depth first |
| Space | O(w) width | O(h) height |
| Use case | Shortest path, level order | Topological sort, cycle detection |

## Problem 10: Count Leaf Nodes in Binary Tree

```csharp
public int CountLeaves(TreeNode root)
{
    if (root == null) return 0;
    if (root.left == null && root.right == null) return 1;
    return CountLeaves(root.left) + CountLeaves(root.right);
}
```

## Problem 11: Longest Valid Parentheses (Hard)

```csharp
public int LongestValidParentheses(string s)
{
    var stack = new Stack<int>();
    stack.Push(-1);
    int maxLen = 0;
    for (int i = 0; i < s.Length; i++)
    {
        if (s[i] == '(')
            stack.Push(i);
        else
        {
            stack.Pop();
            if (stack.Count == 0)
                stack.Push(i);
            else
                maxLen = Math.Max(maxLen, i - stack.Peek());
        }
    }
    return maxLen;
}
// Time: O(n), Space: O(n)
```

## Problem 12: K Largest Elements (Min-Heap)

```csharp
public int[] KLargest(int[] nums, int k)
{
    var minHeap = new PriorityQueue<int, int>();
    foreach (int num in nums)
    {
        minHeap.Enqueue(num, num);
        if (minHeap.Count > k)
            minHeap.Dequeue();
    }
    var result = new int[k];
    for (int i = k - 1; i >= 0; i--)
        result[i] = minHeap.Dequeue();
    return result;
}
// Time: O(n log k), Space: O(k)
```

## Key Complexity Cheat Sheet

| Operation | Array | Dictionary | LinkedList | Stack/Queue |
|-----------|-------|-----------|------------|-------------|
| Access | O(1) | O(1) avg | O(n) | O(n) |
| Search | O(n) | O(1) avg | O(n) | O(n) |
| Insert | O(n) | O(1) avg | O(1) | O(1) |
| Delete | O(n) | O(1) avg | O(1) | O(1) |
