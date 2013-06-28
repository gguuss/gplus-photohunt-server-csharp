<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="invite.aspx.cs" Inherits="PhotoHunt.invite" %>

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
<%-- Generates schema.org microdata that can be parsed to populate snippet
     for "invite" interactive post. --%>
<%@ Import Namespace="PhotoHunt.model" %>
<%@ Import Namespace="PhotoHunt.utils" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<%

String imageUrl = "/images/interactivepost-icon.png";
String name = "";

Theme currentTheme = ThemesHelper.GetCurrentTheme();

PhotohuntContext db = new PhotoHunt.model.PhotohuntContext();
Photo featPhoto = db.Photos.FirstOrDefault(p => p.id == currentTheme.previewPhotoId);

if (featPhoto != null)
{
    imageUrl = featPhoto.thumbnailUrl;
    name = "Photo by " + featPhoto.ownerDisplayName + " for #" +
    currentTheme.displayName.ToLower().Replace("[\\s,]", "") +
        " | #photohunt";
}
%>
<!DOCTYPE html>
<html>
<head>
  <script type="text/javascript">
    window.location.href = 'index.html';
  </script>
  <title><%= name %></title>
</head>
<body itemscope itemtype="http://schema.org/Thing">
  <h1 itemprop="name"><%= name %></h1>
  <img itemprop="image" src="<%= imageUrl %>" />
</body>
</html>
