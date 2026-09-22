using BudeView.Core;

namespace BudeView;

internal static class SelfTests
{
    public static int Run()
    {
        var failures = new List<string>();

        CheckNaturalSort(failures);
        CheckImageTypes(failures);
        CheckStartupOptions(failures);

        if (failures.Count == 0)
        {
            Console.WriteLine("BudeView self-tests passed.");
            return 0;
        }

        Console.Error.WriteLine("BudeView self-tests failed:");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"- {failure}");
        }

        return 1;
    }

    private static void CheckNaturalSort(List<string> failures)
    {
        var values = new[] { "10.jpg", "2.jpg", "1.jpg", "001.jpg", "20.jpg" };
        Array.Sort(values, NaturalStringComparer.Instance);

        var expected = new[] { "1.jpg", "001.jpg", "2.jpg", "10.jpg", "20.jpg" };
        if (!values.SequenceEqual(expected))
        {
            failures.Add($"Natural sort mismatch: {string.Join(", ", values)}");
        }
    }

    private static void CheckImageTypes(List<string> failures)
    {
        if (!ImageFileTypes.IsSupported("a.jpg") ||
            !ImageFileTypes.IsSupported("a.JPEG") ||
            !ImageFileTypes.IsSupported("a.png") ||
            ImageFileTypes.IsSupported("a.gif"))
        {
            failures.Add("V0.2 image type filter is incorrect.");
        }
    }

    private static void CheckStartupOptions(List<string> failures)
    {
        var parsed = StartupOptions.Parse(
        [
            "--trace-file",
            "trace.jsonl",
            "image.jpg"
        ]);

        if (parsed.ImagePath is null ||
            !parsed.ImagePath.EndsWith("image.jpg", StringComparison.OrdinalIgnoreCase) ||
            parsed.TraceFile is null ||
            !parsed.TraceFile.EndsWith("trace.jsonl", StringComparison.OrdinalIgnoreCase))
        {
            failures.Add("Startup option parsing is incorrect.");
        }
    }
}
