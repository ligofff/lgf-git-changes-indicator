using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class GitStatusCache
{
    public static Dictionary<string, string> FileStatuses = new Dictionary<string, string>();

    public static void UpdateCache(string gitOutput)
    {
        ParseGitOutput(gitOutput);
    }
    
    private static void ParseGitOutput(string gitOutput)
    {
        var lines = gitOutput.Split("\n", StringSplitOptions.None);
        FileStatuses.Clear();
        foreach (var line in lines)
        {
            var values = line.Split(new[] {"\t"}, StringSplitOptions.None);
            if (values.Length < 2) continue;
            var status = values[0];
            var file = string.Join("\t", values, 1, values.Length - 1);
            FileStatuses[file] = status;
        }
    }
}