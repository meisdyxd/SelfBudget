using System.Text.RegularExpressions;

namespace SelfBudget.API.Common.Constants;

public static partial class EmailConstants
{
    public const int MAX_LENGTH = 255;
    public const int MIN_LENGTH = 5;
    public static Regex Regex = EmailRegex();

    [GeneratedRegex("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$")]
    private static partial Regex EmailRegex();
}
