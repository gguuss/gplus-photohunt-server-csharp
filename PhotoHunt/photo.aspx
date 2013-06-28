<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="photo.aspx.cs" Inherits="PhotoHunt.photo" %>
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
<%-- Generates schema.org microdata that can be parsed to populate snippet for
     photos interactive posts. --%>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<%@ Import Namespace="PhotoHunt.model" %>
<%
String imageUrl = "";
String name = "";
String description = "";
String photoIdStr = Context.Request.Params["photoId"];
String redirectUrl = "/index.html?photoId=" + photoIdStr;

int photoId = -1;
bool validPhotoId = int.TryParse(photoIdStr, out photoId);

if (validPhotoId)
{
    // Return the photo using this id.
    PhotohuntContext db = new PhotohuntContext();
    Photo photo = db.Photos.FirstOrDefault(p => p.id == photoId);

    if (photo != null)
    {
        name = "Photo by " + photo.ownerDisplayName + " for " +
            photo.themeDisplayName + " | Photo Hunt";
        description = photo.ownerDisplayName +
            " needs your vote to win this hunt.";
        imageUrl = photo.thumbnailUrl;
    }
}

%>
<!DOCTYPE html>
<html>
<head>
  <script type="text/javascript">
    window.location.href = '<%= redirectUrl %>';
  </script>
  <title><%= name %></title>
</head>
<body itemscope itemtype="http://schema.org/Thing">
  <h1 itemprop="name"><%= name %></h1>
  <img itemprop="image" src="<%= imageUrl %>" />
  <p itemprop="description"><%= description %></p>
</body>
</html>
