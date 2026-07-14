using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace LibraryCheckout.Web.Infrastructure;

public static class EnumExtensions
{
    /// <summary>
    /// Returns the [Display(Name = ...)] value for an enum member,
    /// falling back to the member name (e.g. Dvd -> "DVD").
    /// </summary>
    public static string GetDisplayName(this Enum value)
    {
        var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        var display = member?.GetCustomAttribute<DisplayAttribute>();
        return display?.GetName() ?? value.ToString();
    }
}
