using System;
using System.IO;
using System.Linq;

// For entity framework constants.
using System.Data;

// For data members.
using PhotoHunt.model;

namespace PhotoHunt.utils
{
    /// <summary>
    /// This class encapsulates utility functions for themes. These perform basic operations
    /// such as listing today's theme.
    /// </summary>
    static public class ThemesHelper
    {
        /// <summary>
        /// Retrieves a theme given the parameter string for the theme.
        /// </summary>
        /// <param name="themeIdParam">The identifier for the theme.</param>
        /// <returns>The specified theme or will fallback to a new default theme.</returns>
        static public Theme GetSelectedTheme(string themeIdParam)
        {
            // Select the current theme if we haven't been passed a theme
            if (themeIdParam == null)
            {
                Theme t = ThemesHelper.GetCurrentTheme();

                if (t != null)
                {
                    return t;
                }
            }
            else
            {
                // Retrieve the theme using the id from the request.
                int themeId = int.Parse(themeIdParam);

                PhotoHunt.model.PhotohuntContext dbthemes = new PhotoHunt.model.PhotohuntContext();
                return dbthemes.Themes.First(t => t.id == themeId);
            }

            // Fallback to the default theme if no themes exist
            return ThemesHelper.GenerateDefaultTheme();
        }

        /// <summary>
        /// Generates the default PhotoHunt theme for today, currently "Beautiful".
        /// </summary>
        /// <returns>The default PhotoHunt theme for today.</returns>
        static public Theme GenerateDefaultTheme()
        {
            DateTime today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            Theme defaultTheme = AddTheme("Beautiful", today);

            return defaultTheme;
        }

        /// <summary>
        /// Adds a new theme to PhotoHunt.
        /// </summary>
        /// <param name="displayName">The name shown on the theme.</param>
        /// <param name="startDate">The starting date for the theme.</param>
        /// <returns></returns>
        static public Theme AddTheme(string displayName, DateTime startDate)
        {
            PhotoHunt.model.PhotohuntContext db = new PhotoHunt.model.PhotohuntContext();

            Theme newTheme = new Theme();
            newTheme.createdTime = DateTime.Now;

            System.Globalization.DateTimeFormatInfo mfi = new
                System.Globalization.DateTimeFormatInfo();

            newTheme.created = mfi.GetMonthName(newTheme.createdTime.Month) + " " +
                newTheme.createdTime.Day + ", " + newTheme.createdTime.Year;
            newTheme.displayName = displayName;
            newTheme.start = startDate;
            newTheme.previewPhotoId = 0;
            db.Themes.Add(newTheme);
            db.SaveChanges();

            return newTheme;
        }

        /// <summary>
        /// Gets the current PhotoHunt theme for today.
        /// </summary>
        /// <returns>The current PhotoHunt theme on success; otherwise, returns null.</returns>
        static public Theme GetCurrentTheme()
        {
            // Select today's theme
            DateTime today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            DateTime tomorrow = today.AddDays(1);

            PhotoHunt.model.PhotohuntContext db = new PhotoHunt.model.PhotohuntContext();
            return db.Themes.First(theme => theme.start >= today && theme.start < tomorrow);
        }

    }
}
