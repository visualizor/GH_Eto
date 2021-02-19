# Synapse (GH_Eto)
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
