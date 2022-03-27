# QuickNav - History's Favorites

## Introduction

QuickNav allows you to switch back and forth in your navigation history. You can record history items or current selections as favorites and jump to them or open them directly in the inspector. This works for the Scene as well as the Project.

##### Example

Here's a usage example video on youtube:

[![Usage](http://img.youtube.com/vi/pXDlusPfZyY/0.jpg)](https://www.youtube.com/watch?v=pXDlusPfZyY)

## Requirements

Supported Unity versions:

* Unity 2020.3
* Unity 2021.*
* Unity 2022.*

## Quick Setup

##### Installation

* add QuickNav via Package Manager:

  https://github.com/Roland09/QuickNav.git?path=/Assets/Rowlan/QuickNav

  
## The Idea

In Unity every now and then you have to switch back and forth between assets in order to configure them interdependently. Be it because of texture assignment, material assignment, you name it. 
With 2 assets you can help yourself with 2 inspectors and locking one. However that's already very tedious, limiting and will get you only so far. Another thing is that you occasionally want to switch back in history. 
Just like it's common in modern browsers. Besides Unity internal favorites mechanism is very limited as well.

So I created this tool to see the navigation history and in addition to that store some history items or currently selected items in a favorites list.

## Features

* Naviation History
* Favorites List
* Ping items, i. e. select them
* Inspect itmes, i. e. select them and open them in the Inspector
* Add history items to favorites
* Add current selection to favorites
* Support Project and Scene
* Favorites list is Reorderable

## Usage

Open QuickNav:

![Menu](https://user-images.githubusercontent.com/10963432/160269678-64683427-491a-4ccc-ae07-fd6df418d7df.png)

The keybind for it is Ctrl+H (H for History).

Use the arrow keys for navigation. The star for favoriting an item. Click on the magnifying glass to Ping an item. Click the Icon/Name button to open the item in the Inspector.

History:

![History](https://user-images.githubusercontent.com/10963432/160269854-62d67760-dea4-47ea-9fcf-227741f5f018.png)

Favorites:

![Favorites](https://user-images.githubusercontent.com/10963432/160269856-978c0b15-e0f5-44e5-aeb0-112e291291e4.png)


## Limitations

The Folder Structure pane isn't used in QuickNav, only the Detail pane.

##### What this is not

This isn't a full-featured history and bookmark manager like it's common in modern browser. I leave that to Unity, such a feature has been long overdue. 

This is just a means to help you save clicks while browsing around in your project. 

By the way, the history item limit is 20 and stored in the preferences file:

  `Assets/Rowlan/QuickNav/Preferences.asset`

You can easily adjust that in the inspector should the need for it arise.


