# Synapse (GH_Eto)
## Overview
Grasshopper plugin that makes graphic user interfaces with the help of `Eto.Forms`. Currently compiled on a Windows machine.

First, check out this introductory [article](https://willwang6.wixsite.com/precision/post/2019/01/01/how-to-use-etoforms-in-rhinograsshopper-part-1) on `Eto.Forms` and its use within the Grasshopper context, as well as the [guide](https://developer.rhino3d.com/guides/rhinopython/eto-forms-python/) that McNeel has put together.

This plugin is meant to help not only build more intuitive GUIs with Grasshopper components, but also share scripts between Windows and Mac OS as Eto is a cross-platform tool. At the moment, this is a weekend project for fun so only core functionalities are added incrementally. However any questions and suggestions are welcome.

See [complete doc](http://pages.picoe.ca/docs/api/html/R_Project_EtoForms.htm) for more information on each Eto object. 

Eto itself does not have many of the graphic features found in other GUI libraries likely due to its cross-platform conformity. Synapse further simplifies the pieces one has to assemble to get a working GUI. Minimalist functionality will take precedent over ornate design. 
## Installation
This is your typical Grasshopper plugin. Drop the ".gha" file into your Grasshopper library folder. On a PC it's `%AppData%/Roaming/Grasshopper/Libraries/`. On a Mac it's <missing path>. The "Synapse.gha" is published on [food4rhino.com](https://www.food4rhino.com/app/synapse) as well as Rhino 7's package manager. v0.4 onwards, you can drag and drop the SynapseRCP.rhp onto Rhino to install the remote control panel. Type "SynapsePanel" to launch; "SynapseCleanse" to clear contents. 
## Starter's Guide
#### Controls
These are components that make the interactive elements themselves such as a button or an input text box. The component typically has two inputs, `Property` and `Property Value`. As long as the input lists match in number, the component will try to set the corresponding property of the `Eto.Forms` object.

For example, try puting "Text" and "BackgroundColor" as text strings into the "P" of a `SynapseButton` component, "ThisButton" and "255,0,0" text strings into the "V". You should get a red button labeled "ThisButton". Controls like this button will need to go onto a window, which is a container.
#### Containers
These components collect interactive elements and render them to the user. Some of these themselves are interactive such as an expander. The `SynapseWindow` component provides the overall window in which everything is placed. To get organized, it is recommended to first arrange individual interactive elements in an intermediate container before placing them on the main window.

For example, you can create 5 buttons (typically five separate `SynapseButton` components unless you really know what's happening with your data tree) and place them in a `SynapseStack`. The `SynapseStack` component has a "C" input in addition to the "P", "V" for controls. Afterwards, wire up this stack to the "C" of a `SynapseWindow` component. Put a toggle in the "S" input on `SynapseWindow` and it is ready to launch!

Containers can nest in other containers, because containers are also recognized as controls (but not vice versa). One can use a single `SynapseTable` to house every interactive elements, or a combination of a vertical `SynapseStacks` (columns of controls) nested in a horizontal stack (row). Both would produce similar GUIs - a grid of controls.  
#### Parameters
These are mostly helper components working in tandem. Some `Eto.Forms` objects require special property types. A moderate level of intelligence will be built in for common inputs such as automatically converting the text string "255,0,0" to an Eto-recognized `Color` type. However, sometimes the user must provide the exact kind of data type, which will likely be in this category of components. For advanced users, see example file on how to script explicit property objects in GhPython. 

A special component in parameters is the `ValueQuery` component. It listens to any control element and reports its value. This is the critical link between the GUI a user creates and the Grasshopper canvas. For example, when wired to a `SynapseTextBox`, this component will refresh and report the text content whenever a user types in the text box.
#### Examples
A walkthrough of Synapse component and how they are set up can be found [here](https://www.youtube.com/embed/tbC_d84EmuU). Skip to about 3:30 mark for the actual Synapse set-up. Download the [example file](https://github.com/visualizor/GH_Eto/raw/master/SynapseExample.gh) yourself and see thigns in action.
#### Fixed Interface
For simplicity in working with Grasshopper's data flow model, Synapse components were created to never be edited while they are shown. Each time the "S" is toggled to `true` on the `SynapseWindow`, a new `Eto.Forms.Form` is painted. If certain controls must be edited, the window should be closed first. Therefore controls cannot be modified dynamically while the GUI is in use.

*v0.3 onwards, there is an option to enable live property edit in the context menu of `SWindow` component. However edits through "P" and "V" parameters will trigger a window re-instantiation. Use `ValueOverride` if you wish to dynamically change control values.*
## FAQs
#### Why are controls disappearing in the GUI?
This is likely due to changing parameters on the Grasshopper component. Once a Synapse window is shown, controls upstream often cannot be updated without re-launching new windows.
#### Why does a container tell me to disconnect controls?
Usually by linking and re-linking Synapse components, some of them may remember the parent container. Disable control components and re-enable should solve this.
#### What is the difference between Synapse and HumanUI?
Two big differences. One is that HumanUI works on Windows only. Synapse works on Macs too. Two is that HumanUI seems to have a beautified skin and cool graphs. Synapse does not have any custom looks for controls other than the basic properties.
#### Why can't I set a property on a control?
There could be a few reasons. One is that the property can only be set with certain data types and the ones going into "V" aren't matching. Two is that the code for that Synapse element is missing something. Three is that the property is really meant to be set internally and the code does that and overrides your input. In any case, report errors. 
#### I see a type of control on Eto.Forms doc. But where is it on Synapse?
It will take a long time to translate all `Eto.Forms` controls to the Grasshopper ecosystem. Some of them may not be as useful so it's put on hold. Others may be "report" type of controls and will never be added. If you want to see certain controls included, make a request in any case.
#### How do I customize the look of my window?
Unfortunately this is not on my roadmap for Synapse. `Eto.Forms` doesn't seem to have an easy way of adding graphic skins. Synapse will be kept simple. As an alternative, use HTML/CSS to style web forms and access them via the `SnpWebCtrls` component.
#### Why does value query component not return values?
It is likely that the query is listening to a container object that doesn't yield any value. Try linking "C" with controls only.
#### How can I draw my own charts and illustrative graphics?
Limited GUI level drawings with `Eto.Drawing` library is provided. I recommend plugins such as ProvingGround's [Conduit](https://provingground.io/tools/conduit-for-grasshopper/) for more tailored dashboard graphics.
#### Why do my controls look different in RCP?
  Rhino color schemes dictate the controls' on a docking panel. You can simply disable and re-enable all controls in GH before you switch back to typical, standalone windows.
#### What are these mysterious out of range or null reference errors?
  Out-of-range errors happen when a list of things is expected to have certain number of items but doesn't. Null reference typically means the ingredients necessary to start something are bad. Contact author with bad script attached, if you need to trouble-shoot these errors.
