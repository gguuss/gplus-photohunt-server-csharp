<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="addTheme.aspx.cs" Inherits="PhotoHunt.admin.addTheme" %>
<!--
/*
 * Copyright (c) 2013 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except
 * in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed under the License
 * is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
 * or implied. See the License for the specific language governing permissions and limitations under
 * the License.
 */
-->
<!--
This is an administrative script that is included for convenience.
From this page, you can add a theme for a future date.
-->
<%@ Import Namespace="PhotoHunt.model" %>
<%@ Import Namespace="PhotoHunt.utils" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Administration: Theme creator</title>
</head>
<body>
    <form id="themeForm" runat="server">
        <div>
            Theme DisplayName: <input name="dname" type="text" />
            <br />
            Start date:
            <asp:Calendar ID="startdate" runat="server"></asp:Calendar>
            <br />
            <input type="submit" value="Add Theme" />
        </div>
    </form>
    <hr />
    <h1>Themes</h1>
    <%
        // List the themes and display the current theme
        PhotohuntContext dbthemes = new PhotohuntContext();
        var themeQuery = from b in dbthemes.Themes
                         select b;

        List<Theme> themes = new List<Theme>() ;
        foreach (Theme t in themeQuery)
        {
            themes.Add(t);
        }

        Theme currentTheme = ThemesHelper.GetCurrentTheme();
    %>
    Current Theme: <%=currentTheme.displayName %>
    <br /> <%
    foreach (Theme t in themes)
    {%>
        <%=t.displayName%> : Created - <%=t.created%> : Starts - <%=t.start%><br /><%
    }%>
</body>
</html>
