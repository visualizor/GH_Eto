# Synapse (GH_Eto)
## Overview
Grasshopper plugin that makes graphic user interfaces with the help of `Eto.Forms`

First, check out this introductory [article](https://willwang6.wixsite.com/precision/post/2019/01/01/how-to-use-etoforms-in-rhinograsshopper-part-1) on `Eto.Forms` and its use within the Grasshopper context, as well as the [guide](https://developer.rhino3d.com/guides/rhinopython/eto-forms-python/) that McNeel has put together.

This plugin is meant to help not only build more intuitive GUIs with Grasshopper components, but also share scripts between Windows and iOS as Eto is a cross-platform tool. At the moment, this is a weekend project for fun so only core functionalities are added incrementally. However any questions and suggestions are welcome.

Below are `Eto.Forms.Control` objects to add into Synapse. See [complete doc](http://pages.picoe.ca/docs/api/html/R_Project_EtoForms.htm) for more information on each. 
#### User controls
- [x] Label
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
- [x] TableLayout
- [ ] Expander
- [x] Form
- [ ] GridView
- [ ] GroupBox
- [ ] Scrollable
- [ ] TabControl

Eto itself does not have many of the graphic features found in other GUI libraries likely due to its cross-platform conformity. Synapse further simplifies the pieces one has to assemble to get a working GUI. Minimalist functionality will take precedent over ornate design. 
## Installation
This is your typical Grasshopper plugin. Drop the ".gha" file into your Grasshopper library folder. On a PC it's `%AppData%/Roaming/Grasshopper/Libraries/`. On a Mac it's <--insert path>. The "Synapse.gha" is published on [food4rhino.com](https://www.food4rhino.com/) as well as Rhino 7's package manager. 
## Starter's Guide
#### Controls
These are components that make the interactive elements themselves such as a button or an input text box. The component typically has two inputs, `Property` and `Property Value`. As long as the input lists match in number, the component will try to set the corresponding property of the `Eto.Forms` object.

For example, try puting "Text" and "BackgroundColor" as text strings into the "P" of a `SynapseButton` component, "ThisButton" and "255,0,0" text strings into the "V". You should get a red button labeled "ThisButton". Controls like this button will need to go onto a window, which is a container.
#### Containers
These components collect interactive elements and render them to the user. Some of these themselves are interactive such as an expander. The `SynapseWindow` component provides the overall window in which everything is placed. To get organized, it is recommended to first arrange individual interactive elements in an intermediate container before placing them on the main window.

For example, you can create 5 buttons (must be five separate `SynapseButton` components as one component creates a unique object and it will only be rendered once in a container) and place them in a `SynapseStack`. The `SynapseStack` component has a "C" input in addition to the "P", "V" for controls. Afterwards, wire up this stack to the "C" of a `SynapseWindow` component. Put a toggle in the "S" input on `SynapseWindow` and it is ready to launch!

Containers can nest in other containers. One can use a single `SynapseTable` to house every interactive elements, or a combination of a collection of vertical `SynapseStacks` nested in a horizontal stack. Both would produce similar GUIs.
#### Parameters
These are mostly helper components working in tandem. Some `Eto.Forms` objects require special property types. A moderate level of intelligence will be built in for common inputs such as automatically converting the text string "255,0,0" to an Eto-recognized `Color` type. However, sometimes the user must provide the right kind of data type, which will be this category of components. For advanced users, see example file on how to script explicit property objects in GhPython. 

A special component in parameters is the `ValueQuery` component. It listens to the any control element and reports its value. This is the critical link between the GUI a user create and the Grasshopper canvas. For example, when wired to a `SynapseTextBox`, this component will refresh and report the text content whenever a user types in the text box.
#### Examples
A walkthrough of Synapse component and how they are set up can be found [here](https://www.youtube.com/embed/tbC_d84EmuU). Skip to about 3:30 mark for the actual Synapse set-up.
<--insert link to example .gh>
#### Fixed Interface
For simplicity in working with Grasshopper's data flow model, Synapse components cannot be edited while they are shown. Each time the "S" is toggled to `true` on the `SynapseWindow`, a new `Eto.Forms.Form` is painted. If certain controls must be edited, the window should be closed first. Therefore controls cannot be modified dynamically while the GUI is in use. 