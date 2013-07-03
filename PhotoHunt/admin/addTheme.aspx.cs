/*
 * Copyright 2013 Google Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

// For the theme helper.
using PhotoHunt.utils;

namespace PhotoHunt.admin
{
    /// <summary>
    /// This is an administrative utility for adding and listing themes. From
    /// the /admin/themes.aspx page you can list or add themes.
    /// </summary>
    public partial class addTheme : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string displayName = Context.Request["dname"];
            if (displayName != null && displayName.Length > 0)
            {
                ThemesHelper.AddTheme(displayName, startdate.SelectedDate);
            }
        }
    }
}
