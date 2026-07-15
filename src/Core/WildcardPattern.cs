using System.Text.RegularExpressions;

namespace M00STFD.Core;

// A config-friendly name pattern: '*' matches any run of characters, all
// other characters match themselves, case-insensitively. "vault*" matches
// vaultDoor01 and vaultHatch01; "*GarageDoor*" matches every garage door.
public sealed class WildcardPattern
{
	private readonly Regex regex;

	public WildcardPattern(string pattern)
	{
		Text = pattern ?? string.Empty;
		string[] literals = Text.Split('*');
		for (int i = 0; i < literals.Length; i++)
		{
			literals[i] = Regex.Escape(literals[i]);
		}
		regex = new Regex("^" + string.Join(".*", literals) + "$",
			RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
	}

	public string Text { get; }

	public bool IsMatch(string candidate)
	{
		return candidate != null && regex.IsMatch(candidate);
	}
}
