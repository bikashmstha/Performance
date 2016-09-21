Views from MusicStore
=====================

Test app for performance scenarios related to a few of the MVC views from the MusicStore repo. Removes some
dependencies of the originals such as EF and Session. Minimizes logging to eliminate its impact on performance runs.

This application includes the following scenarios and views:

- `AddressAndPayment`: First step in a music store checkout process. Users enter their address and promo code in this
view. Performance focus is on HTML helpers, specifically `Html.EditorForModel()` for a model (`Order`) with a fair
number of properties.
- `Create`: Site administration for a music store. Administrators enter information about new albums in this view.
Performance focus is on HTML helpers, especially `Html.DropDownList()` with a fairly large number of `SelectListItem`s.
- `Register`: First step in a music store user registration process. Users enter their email addresses and passwords in
this view. Performance focus is on tag helpers.

All of the views include validation summaries and client-side validation attributes (et cetera). All use the same
layout and that layout includes two view components and almost 20 anchor tag helpers.