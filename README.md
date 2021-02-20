# Synapse (GH_Eto)
## Overview
Grasshopper plugin that makes graphic user interfaces with the help of `Eto.Forms`

First, check out this introductory [article](https://willwang6.wixsite.com/precision/post/2019/01/01/how-to-use-etoforms-in-rhinograsshopper-part-1) on `Eto.Forms` and its use within the Grasshopper context, as well as the [guide](https://developer.rhino3d.com/guides/rhinopython/eto-forms-python/) that McNeel has put together.

This plugin is meant to help not only build more intuitive GUIs with Grasshopper components, but also share scripts between Windows and iOS as Eto is a cross-platform tool. At the moment, this is a weekend project for fun so only core functionalities are added incrementally. However any questions and suggestions are welcome.

Below are `Eto.Forms.Control` objects to add into Synapse. See [complete doc](http://pages.picoe.ca/docs/api/html/R_Project_EtoForms.htm) for more information on each. 
#### User controls
- [ ] Label
- [x] TextBox
- [ ] MaskedTextBox
- [x] Button
- [x] Slider
- [x] NumericStepper
- [x] CheckBox
- [ ] ComboBox
- [ ] DropDown
- [ ] ListBox
- [ ] PasswordBox
- [ ] RadioButtonList
- [ ] TextArea
#### Containers
- [x] StackLayout
- [ ] DynamicLayout
- [ ] TableLayout
- [ ] Expander
- [x] Form
- [ ] GridView
- [ ] GroupBox
- [ ] Scrollable
- [ ] TabControl

Eto itself does not have many of the graphic features found in other GUI libraries likely due to its cross-platform conformity. Synapse further simplifies the pieces one has to assemble to get a working GUI. Minimalist functionality will take precedent over ornate design. 
## Installation
This is your typical Grasshopper plugin. There should be a "Synapse.gha" file in the "bin" folder on this repo. Drop that into your Grasshopper library folder. On a PC it's `%AppData%/Roaming/Grasshopper/Libraries/`. On a Mac it's </insert path>. The "Synapse.gha" will also be published on [food4rhino.com](https://www.food4rhino.com/) as well as Rhino 7's package manager. 
## Starter's Guide
#### Controls
These are components that make the interactive elements themselves such as a button or an input text box. The component typically has two inputs, `Property` and `Property Value`. As long as the input lists match in number, the component will try to set the corresponding property of the `Eto.Forms` object. For example, try puting "Text" and "BackgroundColor" as text strings into the "P" of a `SynapseButton` component, "ThisButton" and "255,0,0" text strings into the "V". You should get a red button labeled "ThisButton". Controls like this button will need to go onto a window, which is a container.
#### Containers
These components collect interactive elements and render them to the user. Some of these themselves are interactive such as an expander. The "
#### Parameters
