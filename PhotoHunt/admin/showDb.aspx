<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="showDb.aspx.cs" Inherits="PhotoHunt.showDb" %>
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
From this page, you can read data stored in the EF datastore and
can reset the database by passing the parameter ?clear=true.
-->
<%@ Import Namespace="PhotoHunt.model" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Test: Showing the model</title>
</head>
<body>
    <h1>Current user:</h1><%
    if (user != null)
    {
      %><%=user.id%> : <%=user.googleDisplayName%>
      <hr /><%
    }
    else
    {
      %>Signed out!<%
    }
    %>
    <h1>Themes</h1>
    Current Theme: <%=latestTheme.displayName%>
    <br /> <%
    foreach (Theme t in themes)
    {%>
      (<%=t.id %>) <%=t.displayName%> : [Created] <%=t.created%> : [Starts] <%= t.start %><br /><%
    }
    %><h1>Photos</h1><br /><%
    foreach (Photo photo in photos)
    {%>
      <img src="<%=photo.thumbnailUrl%>" />  : <%=photo.themeId %><br />
      Content URL : <%=photo.photoContentUrl%> <br />
      Vote CTA URL : <%=photo.voteCtaUrl %>
      <%
    }
    %>
    <h1>Users</h1>
    <%
    foreach (User u in usersList)
    {%>
      <div id="user"><%= u.id %>:<%= u.googleDisplayName %></strong></div>
      <%
    }

    if (showEdges)
    {
      %><h1>Edges</h1><%
      int count = 0;
      foreach (var edge in edges)
      {%>
        || <%=edge.friendUserId%> : <%=edge.photohuntUserId%> || <br /><%
        count++;
      }
      %>
      <div id="Edges"><strong><%=count%></strong> Edges total in storage</div><%
    }
%>
</body>
</html>
